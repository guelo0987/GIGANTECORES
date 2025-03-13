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

            // Si se proporciona un nuevo ID diferente al actual
            if (categoria.NuevoId.HasValue && categoria.NuevoId.Value != categoria.Id)
            {
                // Verificar que el nuevo ID no exista ya
                if (await _db.categoria.AnyAsync(c => c.Id == categoria.NuevoId.Value))
                {
                    return Conflict(new { Message = $"Ya existe una categoría con el ID '{categoria.NuevoId.Value}'." });
                }

                // Usar una transacción para asegurar la integridad de los datos
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // Crear una nueva categoría con el nuevo ID
                    var newCategoria = new categoria
                    {
                        Id = categoria.NuevoId.Value,
                        Nombre = categoria.Nombre
                    };

                    // Actualizar todas las subcategorías que hacen referencia a esta categoría
                    var relatedSubcategorias = await _db.subcategoria
                        .Where(sc => sc.CategoriaId == existingCategoria.Id)
                        .ToListAsync();

                    foreach (var subcategoria in relatedSubcategorias)
                    {
                        subcategoria.CategoriaId = newCategoria.Id;
                    }

                    // Actualizar todos los productos que hacen referencia a esta categoría
                    var relatedProductos = await _db.productos
                        .Where(p => p.CategoriaId == existingCategoria.Id)
                        .ToListAsync();

                    foreach (var producto in relatedProductos)
                    {
                        producto.CategoriaId = newCategoria.Id;
                    }

                    // Agregar la nueva categoría
                    _db.categoria.Add(newCategoria);
                    
                    // Guardar los cambios para actualizar las referencias
                    await _db.SaveChangesAsync();
                    
                    // Eliminar la categoría antigua
                    _db.categoria.Remove(existingCategoria);
                    await _db.SaveChangesAsync();
                    
                    // Confirmar la transacción
                    await transaction.CommitAsync();
                    
                    return Ok(new { Message = "Categoría actualizada exitosamente con nuevo ID.", Categoria = newCategoria });
                }
                catch (Exception ex)
                {
                    // Revertir la transacción en caso de error
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al actualizar la categoría con nuevo ID");
                    return StatusCode(500, new { Message = "Error al actualizar la categoría.", Error = ex.Message });
                }
            }
            else
            {
                // Actualizar solo el nombre de la categoría
                existingCategoria.Nombre = categoria.Nombre;
                await _db.SaveChangesAsync();
                return Ok(new { Message = "Categoría actualizada exitosamente.", Categoria = existingCategoria });
            }
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
                Id = categoria.NuevoId ?? 0, // Usar el nuevo ID si se proporciona, de lo contrario dejar que la base de datos asigne uno
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
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Buscar la categoría existente
            var categoria = await _db.categoria
                .Include(c => c.Productos)
                .Include(c => c.SubCategoria)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound(new { Message = "La categoría no fue encontrada." });
            }

            // Verificar si hay productos asociados
            if (categoria.Productos.Any())
            {
                return Conflict(new { 
                    Message = "No se puede eliminar la categoría porque tiene productos asociados.",
                    ProductosAsociados = categoria.Productos.Select(p => new { p.Codigo, p.Nombre })
                });
            }

            // Eliminar primero las subcategorías asociadas
            if (categoria.SubCategoria.Any())
            {
                _db.subcategoria.RemoveRange(categoria.SubCategoria);
                await _db.SaveChangesAsync();
            }

            // Eliminar la categoría
            _db.categoria.Remove(categoria);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
            return Ok(new { Message = "Categoría y sus subcategorías eliminadas exitosamente." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al eliminar la categoría");
            return StatusCode(500, new { Message = "Error al eliminar la categoría.", Error = ex.Message });
        }
    }


    
    
    


    
    
    
    
    
    
    


}