using System.Text;
using GIGANTECORE.Context;
using Serilog;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.AspNetCore.Filters;
using System.IO;
using GIGANTECORE.Utils;
using Microsoft.OpenApi.Models;
using DotEnv.Core;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

new EnvLoader()
.AddEnvFile("development.env")
.Load();

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();





// 1. Configuración de logs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("logs/GiganteCoreLogs.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Configuración de base de datos
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(Environment.GetEnvironmentVariable("DATA_BASE_CONNECTION_STRING")));

// 3. Controladores
builder.Services.AddControllers(option => option.ReturnHttpNotAcceptable = true)
    .AddNewtonsoftJson(options => 
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
    .AddXmlDataContractSerializerFormatters();

// 4. Configuración JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Cambiar a false para entorno de producción
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")))
    };
});

// 5. Políticas de autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", 
        policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireEmpleadoRole", 
        policy => policy.RequireRole("Empleado"));
});

// 6. Configuración Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GIGANTE CORE API", Version = "v1" });

    // Configuración JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    c.OperationFilter<SwaggerFileOperationFilter>();
});

// 7. Configuración CORS

// Fix CORS policy: Remove wildcard origin (*) when using credentials
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:5203"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Add this to your Program.cs where services are registered
builder.Services.AddScoped<AdminProductoMedia>();
builder.Services.AddScoped<AdminMultiMedia>();

// Configurar la escucha del puerto 8080 para Google Cloud Run
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();




// B. Swagger en todos los entornos (no solo desarrollo)
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GIGANTE CORE API v1");
    c.ConfigObject.DisplayRequestDuration = true;
    c.RoutePrefix = "swagger"; // Esto es importante
});


// C. Orden CRÍTICO de middlewares
// Comentar o eliminar esta línea en entorno de producción
// app.UseHttpsRedirection();

// Alternativa: solo usar redirección HTTPS en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RolePermissionMiddleware>();

// Agregar justo después de app.UseRouting();
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var exceptionHandlerFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (exceptionHandlerFeature != null)
        {
            var exception = exceptionHandlerFeature.Error;
            Log.Error(exception, "Error no manejado");
            
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new 
            {
                error = "Se produjo un error interno",
                detail = app.Environment.IsDevelopment() ? exception.ToString() : null
            }));
        }
    });
});

// D. Endpoints
app.MapControllers();

// E. Creación de carpetas si no existen (solo desarrollo)

// Redireccionar la raíz a Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Endpoint de diagnóstico mejorado
app.MapGet("/api/diagnostico", () => 
{
    try {
        var vars = new Dictionary<string, string>
        {
            ["JWT_KEY_LENGTH"] = (Environment.GetEnvironmentVariable("JWT_KEY")?.Length ?? 0).ToString(),
            ["JWT_ISSUER"] = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "No configurado",
            ["JWT_AUDIENCE"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "No configurado",
            ["JWT_SUBJECT"] = Environment.GetEnvironmentVariable("JWT_SUBJECT") ?? "No configurado",
            ["DB_CONNECTION"] = "Configurado: " + (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATA_BASE_CONNECTION_STRING"))).ToString(),
            ["ASPNETCORE_ENVIRONMENT"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "No configurado",
            ["PORT"] = Environment.GetEnvironmentVariable("PORT") ?? "No configurado"
        };
        return Results.Ok(vars);
    }
    catch (Exception ex) {
        return Results.Problem(ex.ToString());
    }
});

app.MapGet("/api/diagnostico/db", async (MyDbContext db) => 
{
    try {
        bool canConnect = false;
        string errorMessage = "";
        
        try {
            canConnect = await db.Database.CanConnectAsync();
        }
        catch (Exception ex) {
            errorMessage = ex.Message;
        }
        
        return Results.Ok(new { 
            CanConnect = canConnect,
            Error = errorMessage,
            ConnectionString = "***HIDDEN***" // No mostrar la cadena de conexión completa por seguridad
        });
    }
    catch (Exception ex) {
        return Results.Problem(ex.ToString());
    }
});

app.MapGet("/api/diagnostico/users", async (MyDbContext db) => 
{
    try {
        var userCount = await db.Admins.CountAsync();
        return Results.Ok(new { UserCount = userCount });
    }
    catch (Exception ex) {
        return Results.Problem(ex.ToString());
    }
});

app.MapGet("/api/diagnostico/auth", async (MyDbContext db) => 
{
    try {
        var adminWithRoles = await db.Admins
            .Include(a => a.Role)
            .Select(a => new { 
                a.Mail, 
                RoleName = a.Role.Name,
                RoleId = a.RolId 
            })
            .ToListAsync();

        return Results.Ok(new { 
            AdminCount = adminWithRoles.Count,
            Admins = adminWithRoles
        });
    }
    catch (Exception ex) {
        return Results.Problem(new ProblemDetails {
            Title = "Error al consultar admins",
            Detail = ex.Message + "\n" + ex.InnerException?.Message,
            Status = 500
        });
    }
});

app.MapGet("/api/diagnostico/connection", async () => 
{
    try {
        var connectionString = Environment.GetEnvironmentVariable("DATA_BASE_CONNECTION_STRING");
        var maskedConnectionString = "No configurado";
        var pingResult = false;
        var pingError = "";
        var tcpTestResult = false;
        var tcpTestError = "";

        if (!string.IsNullOrEmpty(connectionString))
        {
            try {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                var server = builder.DataSource.Split(',')[0];
                var port = 1433; // Puerto por defecto de SQL Server
                
                // Test ICMP (ping)
                using (var ping = new System.Net.NetworkInformation.Ping())
                {
                    var reply = await ping.SendPingAsync(server, 1000);
                    pingResult = reply.Status == System.Net.NetworkInformation.IPStatus.Success;
                    pingError = reply.Status.ToString();
                }

                // Test TCP
                try {
                    using (var tcpClient = new System.Net.Sockets.TcpClient())
                    {
                        await tcpClient.ConnectAsync(server, port);
                        tcpTestResult = tcpClient.Connected;
                    }
                }
                catch (Exception ex)
                {
                    tcpTestError = ex.Message;
                }
                
                builder.Password = "***HIDDEN***";
                maskedConnectionString = builder.ToString();
            }
            catch (Exception ex) {
                maskedConnectionString = $"INVÁLIDA: {ex.Message}";
            }
        }
        
        return Results.Ok(new { 
            ConnectionStringConfigured = !string.IsNullOrEmpty(connectionString),
            MaskedConnectionString = maskedConnectionString,
            ServerPingSuccess = pingResult,
            PingStatus = pingError,
            TcpTestSuccess = tcpTestResult,
            TcpTestError = tcpTestError
        });
    }
    catch (Exception ex) {
        return Results.Problem(ex.ToString());
    }
});

app.MapGet("/api/diagnostico/sqltest", async () => 
{
    try {
        var connectionString = Environment.GetEnvironmentVariable("DATA_BASE_CONNECTION_STRING");
        var result = "No se intentó la conexión";
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            try {
                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    result = "Conexión exitosa";
                    
                    // Intentar una consulta simple
                    using (var command = new Microsoft.Data.SqlClient.SqlCommand("SELECT @@VERSION", connection))
                    {
                        var version = await command.ExecuteScalarAsync();
                        result += $" - Versión: {version}";
                    }
                }
            }
            catch (Exception ex) {
                result = $"Error: {ex.Message}";
                if (ex.InnerException != null) {
                    result += $" | Inner: {ex.InnerException.Message}";
                }
            }
        }
        
        return Results.Ok(new { 
            TestResult = result
        });
    }
    catch (Exception ex) {
        return Results.Problem(ex.ToString());
    }
});

app.Run();