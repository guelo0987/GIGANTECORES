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
using System.Net.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

new EnvLoader()
.AddEnvFile("development.env")
.Load();

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();





// 1. Configuración de logs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("logs/GiganteCoreLogs.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Configuración de base de datos
// Google Cloud SQL Connection String Builder for PostgreSQL
string BuildGoogleCloudPostgreSqlConnectionString()
{
    var instanceConnectionName = Environment.GetEnvironmentVariable("INSTANCE_CONNECTION_NAME");
    var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "";
    var dbPass = Environment.GetEnvironmentVariable("DB_PASS");
    var dbName = Environment.GetEnvironmentVariable("DB_NAME");
    
    // For development local with Cloud SQL Proxy
    return $"Host=127.0.0.1;Port=5432;Database={dbName};Username={dbUser};Password={dbPass};";
}

// Use Google Cloud SQL connection if environment variables are set
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("INSTANCE_CONNECTION_NAME")))
{
    builder.Services.AddDbContext<MyDbContext>(options =>
        options.UseNpgsql(BuildGoogleCloudPostgreSqlConnectionString()));
    
    Log.Information("Using Google Cloud PostgreSQL connection");
}
else
{
    // Fall back to the standard PostgreSQL connection
    builder.Services.AddDbContext<MyDbContext>(options =>
        options.UseNpgsql(Environment.GetEnvironmentVariable("DATA_BASE_CONNECTION_STRING")));
}

Log.Information("Using standard PostgreSQL connection");


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
                "http://localhost:5203",
                "https://giganteadminfront-5oz6-3cg6rrosh-jessies-projects-a23b12ca.vercel.app",
                "https://giganteadminfront-5oz6.vercel.app"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Add this to your Program.cs where services are registered
builder.Services.AddScoped<AdminProductoMedia>();
builder.Services.AddScoped<AdminMultiMedia>();

// Configurar la escucha del puerto para desarrollo local
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
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

// Alternativa: solo usar redirección HTTPS en desarrollo //OJO BORRAR !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RolePermissionMiddleware>();
app.UseResponseCompression();

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
app.MapGet("/api/diagnostico/sqltest", async () => 
{
    try {
        string connectionString = "";
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("INSTANCE_CONNECTION_NAME")))
        {
            connectionString = BuildGoogleCloudPostgreSqlConnectionString();
        }
        else
        {
            connectionString = Environment.GetEnvironmentVariable("DATA_BASE_CONNECTION_STRING");
        }
        
        var result = "No se intentó la conexión";
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            try {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    result = "Conexión exitosa";
                    
                    // PostgreSQL version query
                    using (var command = new NpgsqlCommand("SELECT version()", connection))
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
            TestResult = result,
            IsCloudSql = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("INSTANCE_CONNECTION_NAME"))
        });
    }
    catch (Exception ex) {
        return Results.Problem(ex.ToString());
    }
});

app.MapGet("/api/diagnostico/external-ip", async () => 
{
    try {
        string externalIp = "No se pudo determinar";
        
        try {
            using (var httpClient = new HttpClient())
            {
                externalIp = await httpClient.GetStringAsync("https://api.ipify.org");
            }
        }
        catch (Exception ex) {
            externalIp = $"Error: {ex.Message}";
        }
        
        return Results.Ok(new { 
            ExternalIp = externalIp
        });
    }
    catch (Exception ex) {
        return Results.Problem(ex.ToString());
    }
});

app.Run();