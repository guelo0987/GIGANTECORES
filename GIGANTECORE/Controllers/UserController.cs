using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GIGANTECORE.Context;
using GIGANTECORE.DTO;
using GIGANTECORE.Models;
using GIGANTECORE.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace GIGANTECORE.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "RequireAdministratorRole")]
public class UserController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger, MyDbContext db, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _db = db;
    }

    // Crear o Editar un  usuario
    
  [HttpPost]  
public async Task<IActionResult> AddOrUpdate([FromBody] AdminDTO adminDto)
{
    if (adminDto == null)
    {
        _logger.LogError("El cuerpo de la solicitud está vacío.");
        return BadRequest("El cuerpo de la solicitud no puede estar vacío.");
    }

    if (string.IsNullOrEmpty(adminDto.Mail))
    {
        _logger.LogError("El correo es obligatorio.");
        return BadRequest("El correo es obligatorio.");
    }

    // Verificar si el rol existe
    var role = await _db.roles.FirstOrDefaultAsync(r => r.IdRol == adminDto.Rol);
    if (role == null)
    {
        _logger.LogError($"El rol con ID {adminDto.Rol} no existe.");
        return BadRequest("El rol especificado no existe.");
    }

    // Verificamos si es una actualización (ID presente y válido)
    if (adminDto.Id > 0)
    {
        var existingUser = await _db.admin
            .Include(a => a.Role)
            .FirstOrDefaultAsync(u => u.Id == adminDto.Id);

        if (existingUser == null)
        {
            _logger.LogError($"Usuario con ID {adminDto.Id} no encontrado.");
            return NotFound("Usuario no encontrado.");
        }

        // Actualizamos los valores
        existingUser.Nombre = adminDto.Nombre;
        existingUser.Mail = adminDto.Mail;
        existingUser.RolId = adminDto.Rol;
        existingUser.Telefono = adminDto.Telefono;
        existingUser.SoloLectura = adminDto.SoloLectura;

        // Solo actualizamos la contraseña si se envía una nueva
        if (!string.IsNullOrEmpty(adminDto.Password))
        {
            existingUser.Password = PassHasher.HashPassword(adminDto.Password);
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation($"Usuario con ID {adminDto.Id} actualizado exitosamente.");
        return Ok(new { Message = "Usuario actualizado exitosamente.", UserId = existingUser.Id });
    }
    else
    {
        // Validaciones para un nuevo registro
        if (await _db.admin.AnyAsync(a => a.Mail == adminDto.Mail))
        {
            _logger.LogError("El correo ya está registrado.");
            return BadRequest("El correo ya está registrado.");
        }

        var newAdmin = new admin
        {
            Nombre = adminDto.Nombre,
            Mail = adminDto.Mail,
            Password = PassHasher.HashPassword(adminDto.Password),
            RolId = adminDto.Rol,  // Usar el rol proporcionado
            FechaIngreso = DateTime.Now,
            Telefono = adminDto.Telefono,
            SoloLectura = adminDto.SoloLectura
        };

        await _db.admin.AddAsync(newAdmin);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Usuario creado exitosamente.");

        return Ok(new { Message = "Usuario registrado exitosamente.", UserId = newAdmin.Id });
    }
}


    // Obtener todos los usuarios
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _db.admin.ToListAsync();

        return Ok(users);
    }

    // Obtener un usuario por ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _db.admin.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            _logger.LogError($"Usuario con ID {id} no encontrado.");
            return NotFound("Usuario no encontrado.");
        }

        return Ok(user);
    }

    
   

    // Eliminar un usuario
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.admin.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            _logger.LogError($"Usuario con ID {id} no encontrado.");
            return NotFound("Usuario no encontrado.");
        }

        _db.admin.Remove(user);
        await _db.SaveChangesAsync();
        _logger.LogInformation($"Usuario con ID {id} eliminado exitosamente.");

        return Ok("Usuario eliminado exitosamente.");
    }
}