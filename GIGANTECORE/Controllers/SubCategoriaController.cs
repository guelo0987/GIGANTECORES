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
public class SubCategoriaController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly ILogger<SubCategoriaController> _logger;

    public SubCategoriaController(ILogger<SubCategoriaController> logger, MyDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetSubCategorias()
    {
        return Ok(await _db.subcategoria.ToListAsync());
    }
    
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubCategoriaId(int id)
    {
        var subCategorium = await _db.subcategoria
            .FirstOrDefaultAsync(u => u.Id == id);

        if (subCategorium == null)
        {
            _logger.LogError($"Subcategoria con ID {id} no encontrada.");
            return NotFound("Subcategoria no encontrada.");
        }

        return Ok(subCategorium);
    }

    [HttpPost]
    public async Task<IActionResult> AddOrUpdateSubCategoria([FromBody] SubCategoriumDTO subCategoria)
    {
        if (subCategoria == null)
        {
            return BadRequest(new { Message = "El cuerpo de la solicitud no puede estar vacío." });
        }

        if (string.IsNullOrWhiteSpace(subCategoria.Nombre))
        {
            return BadRequest(new { Message = "El nombre de la subcategoría es obligatorio." });
        }

        if (subCategoria.CategoriaId <= 0 || !_db.categoria.Any(c => c.Id == subCategoria.CategoriaId))
        {
            return BadRequest(new { Message = "La categoría asociada no es válida." });
        }

        if (subCategoria.Id > 0) // Actualizar subcategoría existente
        {
            var existingSubCategoria = await _db.subcategoria.FirstOrDefaultAsync(sc => sc.Id == subCategoria.Id);

            if (existingSubCategoria == null)
            {
                return NotFound(new { Message = "La subcategoría no fue encontrada para su actualización." });
            }

            if (await _db.subcategoria.AnyAsync(sc => sc.Id != subCategoria.Id && sc.Nombre.ToLower() == subCategoria.Nombre.ToLower()))
            {
                return Conflict(new { Message = $"Ya existe otra subcategoría con el nombre '{subCategoria.Nombre}'." });
            }

            // Actualizar los datos de la subcategoría
            existingSubCategoria.Nombre = subCategoria.Nombre;
            existingSubCategoria.CategoriaId = subCategoria.CategoriaId;

            await _db.SaveChangesAsync();
            return Ok(new { Message = "Subcategoría actualizada exitosamente.", SubCategoria = existingSubCategoria });
        }
        else // Crear nueva subcategoría
        {
            if (await _db.subcategoria.AnyAsync(sc => sc.Nombre.ToLower() == subCategoria.Nombre.ToLower()))
            {
                return Conflict(new { Message = $"Ya existe una subcategoría con el nombre '{subCategoria.Nombre}'." });
            }

            var newSubCategoria = new subcategoria
            {
                Nombre = subCategoria.Nombre,
                CategoriaId = subCategoria.CategoriaId
            };

            await _db.subcategoria.AddAsync(newSubCategoria);
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Subcategoría creada exitosamente.", SubCategoria = newSubCategoria });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubCategoria(int id)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var subCategoria = await _db.subcategoria
                .Include(sc => sc.Productos)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (subCategoria == null)
            {
                return NotFound(new { Message = "La subcategoría no fue encontrada." });
            }

            // Verificar si hay productos asociados
            if (subCategoria.Productos.Any())
            {
                return Conflict(new { 
                    Message = "No se puede eliminar la subcategoría porque tiene productos asociados.",
                    ProductosAsociados = subCategoria.Productos.Select(p => new { p.Codigo, p.Nombre })
                });
            }

            _db.subcategoria.Remove(subCategoria);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
            return Ok(new { Message = "Subcategoría eliminada exitosamente." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al eliminar la subcategoría");
            return StatusCode(500, new { Message = "Error al eliminar la subcategoría.", Error = ex.Message });
        }
    }
}
