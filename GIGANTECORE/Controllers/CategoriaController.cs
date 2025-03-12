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
public class CategoriaController:ControllerBase
{



    private readonly MyDbContext _db;
    private readonly ILogger<CategoriaController> _logger;



    public CategoriaController(ILogger<CategoriaController> logger, MyDbContext db)
    {
        _logger = logger;
        _db = db;
    }



    
    [HttpGet]
    public async Task<IActionResult> GetCategorias()
    {
        return Ok(await _db.categoria.ToListAsync());
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoriaId(int id)
    {
        var categorium = await _db.categoria
            .FirstOrDefaultAsync(u => u.Id == id);

        if (categorium == null)
        {
            _logger.LogError($"categoria con ID {id} no encontrada.");
            return NotFound("categoria no encontrada.");
        }

        return Ok(categorium);
    }

    
    [HttpPost]
    public async Task<IActionResult> AddOrUpdateCategoria([FromBody] CategoriumDTO categoria)
    {
        // Validar que el objeto no sea nulo
        if (categoria == null)
        {
            return BadRequest(new { Message = "El cuerpo de la solicitud no puede estar vacío." });
        }

        // Validar que el nombre no esté vacío
        if (string.IsNullOrWhiteSpace(categoria.Nombre))
        {
            return BadRequest(new { Message = "El nombre de la categoría es obligatorio." });
        }

        if (categoria.Id > 0) // Actualizar categoría existente
        {
            // Buscar la categoría por ID
            var existingCategoria = await _db.categoria.FirstOrDefaultAsync(c => c.Id == categoria.Id);

            if (existingCategoria == null)
            {
                return NotFound(new { Message = "La categoría no fue encontrada para su actualización." });
            }

            // Verificar si el nuevo nombre ya existe en otra categoría (case insensitive)
            if (await _db.categoria.AnyAsync(c => c.Id != categoria.Id && c.Nombre.ToLower() == categoria.Nombre.ToLower()))
            {
                return Conflict(new { Message = $"Ya existe otra categoría con el nombre '{categoria.Nombre}'." });
            }

            // Actualizar los datos de la categoría
            existingCategoria.Nombre = categoria.Nombre;

            await _db.SaveChangesAsync();
            return Ok(new { Message = "Categoría actualizada exitosamente.", Categoria = existingCategoria });
        }
        else // Crear nueva categoría
        {
            // Verificar si ya existe una categoría con el mismo nombre
            if (await _db.categoria.AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.ToLower()))
            {
                return Conflict(new { Message = $"Ya existe una categoría con el nombre '{categoria.Nombre}'." });
            }

            // Crear nueva categoría
            var newCategoria = new categoria
            {
                Nombre = categoria.Nombre
            };

            _db.categoria.Add(newCategoria);
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Categoría creada exitosamente.", Categoria = newCategoria });
        }
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategoria(int id)
    {
        // Buscar la categoría existente
        var categoria = await _db.categoria.FirstOrDefaultAsync(c => c.Id == id);
        if (categoria == null)
        {
            return NotFound(new { Message = "La categoría no fue encontrada." });
        }

        // Eliminar la categoría
        _db.categoria.Remove(categoria);
        await _db.SaveChangesAsync();

        return Ok(new { Message = "Categoría eliminada exitosamente." });
    }


    
    
    


    
    
    
    
    
    
    


}