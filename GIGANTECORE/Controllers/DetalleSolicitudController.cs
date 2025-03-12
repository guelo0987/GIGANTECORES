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
public class DetalleSolicitudController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly ILogger<DetalleSolicitudController> _logger;

    public DetalleSolicitudController(ILogger<DetalleSolicitudController> logger, MyDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet]
    public IActionResult GetDetalleSolicitudes()
    {
        // Retorna todos los productos
        return Ok(_db.DetalleSolicituds.
            Include(o=>o.IdSolicitudNavigation)
            .Include(o=>o.Productos)
            .ToList());
    }

 

    [HttpGet("{Id}")]
    public IActionResult GetDetalleSolicitudesId(int Id)
    {
        var Detallesolicitud = _db.DetalleSolicituds.
            Include(o=>o.IdSolicitudNavigation)
            .Include(o=>o.Productos)
            .FirstOrDefault(u => u.IdSolicitud == Id);

        if (Detallesolicitud == null)
        {
            _logger.LogError($"Producto con ID {Id} no encontrada.");
            return NotFound("Producto no encontrada.");
        }

        return Ok(Detallesolicitud);
    }

}