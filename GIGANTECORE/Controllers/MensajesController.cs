using GIGANTECORE.Context;
using GIGANTECORE.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GIGANTECORE.Controllers;



[ApiController]
[Route("api/[controller]")]
public class MensajesController:ControllerBase
{

    private readonly ILogger<MensajesController> _logger;
    private readonly MyDbContext _db;
    

    public MensajesController(MyDbContext db, ILogger<MensajesController> logger)
    {
        _db = db;
        _logger = logger;
    }



    [HttpGet]
    public async Task<IActionResult> ObtenerMensajes()
    {
        try
        {
            var mensajes = await _db.mensajes.ToListAsync();
            return Ok(mensajes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mensajes");
            return StatusCode(500, "Error interno del servidor al obtener mensajes");
        }
    }

   
}