using System.Security.Claims;
using GIGANTECORE.Context;
using GIGANTECORE.DTO;
using GIGANTECORE.Models;
using GIGANTECORE.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GIGANTECORE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly ILogger<ProductosController> _logger;
    private readonly IConfiguration _configuration;
    private readonly AdminProductoMedia _adminProductoMedia;

    public ProductosController(ILogger<ProductosController> logger, MyDbContext db, IConfiguration configuration)
    {
        _logger = logger;
        _db = db;
        _configuration = configuration;
        _adminProductoMedia = new AdminProductoMedia(db, configuration);
    }

    [HttpGet]
    public async Task<IActionResult> GetProductos()
    {
        return Ok(await _db.productos.ToListAsync());
    }
    
    
    
    [HttpGet("{codigo}")]
    public async Task<IActionResult> GetProductoId(int codigo)
    {
        try 
        {
            var totalProductos = await _db.productos.CountAsync();
            var producto = await _db.productos
                .FirstOrDefaultAsync(u => u.Codigo == codigo);

            if (producto == null)
            {
                _logger.LogError($"Producto con ID {codigo} no encontrado. Total de productos en BD: {totalProductos}");
                return NotFound(new { Message = "Producto no encontrado.", TotalProductos = totalProductos });
            }

            return Ok(producto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al buscar producto: {ex.Message}");
            return StatusCode(500, new { Message = "Error interno al buscar el producto", Error = ex.Message });
        }
    }

    
    
    
    
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddOrUpdateProducto([FromForm] ProductoDTO producto, IFormFile? imageFile)
    {
        if (producto == null || string.IsNullOrWhiteSpace(producto.Nombre))
        {
            return BadRequest(new { Message = "Los datos del producto son obligatorios." });
        }
        
        var existingProducto = await _db.productos.FirstOrDefaultAsync(p => p.Codigo == producto.Codigo);

        if (existingProducto != null)
        {
            if (imageFile != null)
            {
                var uploadResult = await _adminProductoMedia.Update(imageFile, existingProducto.ImageUrl);
                if (uploadResult is { } result && result.GetType().GetProperty("success")?.GetValue(result) is bool success)
                {
                    if (!success)
                    {
                        return BadRequest(new { Message = "Error al actualizar la imagen del producto." });
                    }
                    existingProducto.ImageUrl = result.GetType().GetProperty("fileName")?.GetValue(result)?.ToString();
                }
            }

            existingProducto.Nombre = producto.Nombre;
            existingProducto.Marca = producto.Marca;
            existingProducto.Stock = producto.Stock;
            existingProducto.Descripcion = producto.Descripcion;
            existingProducto.SubCategoriaId = producto.SubCategoriaId;
            existingProducto.CategoriaId = producto.CategoriaId;
            existingProducto.EsDestacado = producto.EsDestacado;
            existingProducto.Medidas = producto.Medidas;

            await _db.SaveChangesAsync();
            return Ok(new { Message = "Producto actualizado exitosamente.", Producto = existingProducto });
        }
        else
        {
            if (await _db.productos.AnyAsync(p => p.Nombre.ToLower() == producto.Nombre.ToLower()))
            {
                return Conflict(new { Message = $"Ya existe un producto con el nombre '{producto.Nombre}'." });
            }

            string fileName = null;
            if (imageFile != null)
            {
                var uploadResult = await _adminProductoMedia.Upload(imageFile);
                if (uploadResult is { } result && result.GetType().GetProperty("success")?.GetValue(result) is bool success)
                {
                    if (!success)
                    {
                        return BadRequest(new { Message = "Error al subir la imagen del producto." });
                    }
                    fileName = result.GetType().GetProperty("fileName")?.GetValue(result)?.ToString();
                }
            }

            var newProducto = new productos
            {
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                Marca = producto.Marca,
                Stock = producto.Stock,
                Descripcion = producto.Descripcion,
                SubCategoriaId = producto.SubCategoriaId,
                CategoriaId = producto.CategoriaId,
                ImageUrl = fileName,
                EsDestacado = producto.EsDestacado,
                Medidas = producto.Medidas
            };

            await _db.productos.AddAsync(newProducto);
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Producto creado exitosamente.", Producto = newProducto });
        }
    }
    
    
    [HttpDelete("{codigo}")]
    public async Task<IActionResult> DeleteProducto(int codigo)
    {
        var producto = await _db.productos.FirstOrDefaultAsync(p => p.Codigo == codigo);
        if (producto == null)
        {
            return NotFound(new { Message = "El producto no fue encontrado." });
        }

        await _adminProductoMedia.Delete(producto.ImageUrl);

        _db.productos.Remove(producto);
        await _db.SaveChangesAsync();

        return Ok(new { Message = "Producto eliminado exitosamente." });
    }
    
    
    
    
    
    
    // NUEVOS ENDPOINTS DESPUES DEL BULK TEST
    // 1. Filtrar por Categoría y Subcategoría
    [HttpGet("categoria/{categoriaId}/subcategoria/{subcategoriaId}")]
    public async Task<IActionResult> GetByCategoriaAndSubcategoria(int categoriaId, int subcategoriaId)
    {
        var productos = await _db.productos
            .Where(p => p.CategoriaId == categoriaId && p.SubCategoriaId == subcategoriaId)
            .ToListAsync();

        return Ok(productos);
    }
    
    // 2. Mostrar todos los productos de una Categoría
    [HttpGet("categoria/{categoriaId}")]
    public async Task<IActionResult> GetByCategoria(int categoriaId)
    {
        var productos = await _db.productos
            .Where(p => p.CategoriaId == categoriaId)
            .ToListAsync();

        return Ok(productos);
    }
    
    // 3. Listar todas las marcas disponibles
    [HttpGet("marcas")]
    public async Task<IActionResult> GetMarcas()
    {
        var marcas = await _db.productos
            .Select(p => p.Marca)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync();

        return Ok(marcas);
    }

    // 4. Filtrar productos por Marca
    [HttpGet("marca/{marca}")]
    public async Task<IActionResult> GetByMarca(string marca)
    {
        if (string.IsNullOrEmpty(marca))
            return BadRequest("La marca es requerida");

        var productos = await _db.productos
            .Where(p => p.Marca != null && 
                        p.Marca.Trim().ToLower() == marca.Trim().ToLower())
            .ToListAsync();

        return Ok(productos);
    }



}