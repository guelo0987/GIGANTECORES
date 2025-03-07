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
    options.RequireHttpsMetadata = true;
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
            .WithOrigins("http://localhost:3000")  // Ensure this is properly set
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());  // Only use AllowCredentials if origins are explicitly listed
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

// Agregar antes de app.Run() para diagnóstico (eliminar en producción después)
app.MapGet("/api/diagnostico", (IWebHostEnvironment env) => 
{
    if (env.IsDevelopment())
    {
        var vars = new Dictionary<string, string>
        {
            ["JWT_KEY_LENGTH"] = (Environment.GetEnvironmentVariable("JWT_KEY")?.Length ?? 0).ToString(),
            ["JWT_ISSUER"] = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "No configurado",
            ["JWT_AUDIENCE"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "No configurado",
            ["DB_CONNECTION"] = "Configurado: " + (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATA_BASE_CONNECTION_STRING"))).ToString()
        };
        return Results.Ok(vars);
    }
    return Results.NotFound();
});

app.Run();