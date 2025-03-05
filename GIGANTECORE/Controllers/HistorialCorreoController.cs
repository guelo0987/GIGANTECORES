using System.Security.Claims;
using GIGANTECORE.Context;
using GIGANTECORE.DTO;
using GIGANTECORE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GIGANTECORE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HistorialCorreoController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly ILogger<HistorialCorreoController> _logger;

    public HistorialCorreoController(ILogger<HistorialCorreoController> logger, MyDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet]
    public IActionResult GetHistorialCorreoController()
    {
        // Retorna todos los productos
        return Ok(_db.HistorialCorreos.ToList());
    }

 

    [HttpGet("{Id}")]
    public IActionResult GetHistorialCorreoControllerId(int Id)
    {
        var HistorialCorreoController = _db.HistorialCorreos.
            FirstOrDefault(u => u.Id == Id);

        if (HistorialCorreoController == null)
        {
            _logger.LogError($"Producto con ID {Id} no encontrada.");
            return NotFound("Producto no encontrada.");
        }

        return Ok(HistorialCorreoController);
    }

}