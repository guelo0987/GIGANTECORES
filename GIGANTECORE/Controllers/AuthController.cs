using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GIGANTECORE.Context;
using GIGANTECORE.DTO;
using GIGANTECORE.Models;
using GIGANTECORE.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GIGANTECORE.Controllers;



[Route("api/[controller]")]
[ApiController]
public class AuthController:ControllerBase
{


    private readonly MyDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger, MyDbContext db, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _db = db;
    }
    
    
    
    // Autenticar Admin con DTO
    [HttpPost("login")]
    public async Task<IActionResult> AuthenticateAdmin([FromBody] LoginRequest loginRequest)
    {
        try
        {
            _logger.LogInformation($"Intento de login para: {loginRequest.Mail}");
            
            var admin = await _db.Admins
                .Include(o => o.Role)
                .FirstOrDefaultAsync(a => a.Mail == loginRequest.Mail);

            _logger.LogInformation($"Admin encontrado: {admin != null}, Tiene rol: {admin?.Role != null}");

            if (admin == null)
            {
                _logger.LogError("Usuario no encontrado.");
                return Unauthorized("Credenciales inválidas.");
            }

            // Verificar contraseña
            if (loginRequest.Password != admin.Password)
            {
                _logger.LogError("Contraseña incorrecta.");
                return Unauthorized("Credenciales inválidas.");
            }

            // Validar rol
            if (admin.Role.Name != "Admin" && admin.Role.Name != "Empleado")
            {
                _logger.LogError("Rol nos válido.");
                return Unauthorized("El usuario no tiene un rols válido.");
            }

            // Crear los claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, Environment.GetEnvironmentVariable("JWT_SUBJECT")),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Id", admin.Id.ToString()),
                new Claim(ClaimTypes.Role, admin.Role.Name) // Rol dinámico
            };
            
            _logger.LogInformation("Log in Exitoso");

            // Generar y devolver el token
            return await GenerateToken(claims, new AdminDTO
            {
                Nombre = admin.Nombre,
                Telefono = admin.Telefono,
                Mail = admin.Mail,
                Rol = admin.RolId,
                SoloLectura = admin.SoloLectura
            }, admin.Role.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el proceso de login");
            throw; // Esto nos permitirá ver el error completo en el middleware de excepciones
        }
    }


    // Generar Token JWT
    private async Task<IActionResult> GenerateToken(Claim[] claims, AdminDTO adminDto, string role)
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? 
            throw new InvalidOperationException("JWT_KEY not found in environment variables");
            
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("JWT_ISSUER"),
            audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: signIn
        );

        string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation("Login exitoso");

        return Ok(new { Token = tokenValue, User = adminDto, Role = role });
    }
    
    

}

public class LoginRequest
{
    public string Mail { get; set; } = null!;
    public string Password { get; set; } = null!;
}