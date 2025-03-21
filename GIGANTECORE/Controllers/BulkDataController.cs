using GIGANTECORE.Context;
using GIGANTECORE.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using OfficeOpenXml;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace GIGANTECORE.Controllers;






[ApiController]
[Route("api/[controller]")]
public class BulkDataController:ControllerBase
{

    private readonly MyDbContext _db;
    private readonly ILogger<BulkDataController> _logger;


    public BulkDataController(MyDbContext db, ILogger<BulkDataController> logger)
    {
        _db = db;
        _logger = logger;
    }



    
    
    [HttpGet("ProductsBulk")]
    public async Task<IActionResult> GetProductos()
    {
    
        var products =await  _db.productos.ToListAsync();
    
        return Ok(products);
    
    }
    

    [HttpPost("UploadProductos")]
    public async Task<IActionResult> UploadProductos(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("El archivo no puede estar vacío");
        }

        string fileExtension = Path.GetExtension(file.FileName).ToLower();
        List<productos> nuevosProductos = new List<productos>();

        try
        {
            if (fileExtension == ".csv")
            {
                nuevosProductos = await ProcessCsvFile(file);
            }
            else if (fileExtension == ".xlsx" || fileExtension == ".xls")
            {
                nuevosProductos = await ProcessExcelFile(file);
            }
            else
            {
                return BadRequest("Formato de archivo no soportado. Use CSV o Excel (xlsx/xls).");
            }

            // Si no hay productos después de procesar, algo salió mal
            if (nuevosProductos.Count == 0)
            {
                return BadRequest("No se pudieron extraer productos del archivo o el formato es incorrecto.");
            }

            // Verificar que las categorías y subcategorías existan
            var errorValidacion = await ValidarReferencias(nuevosProductos);
            if (!string.IsNullOrEmpty(errorValidacion))
            {
                return BadRequest(errorValidacion);
            }

            // Verificar productos duplicados por código
            var productosExistentes = await _db.productos
                .Where(p => nuevosProductos.Select(np => np.Codigo).Contains(p.Codigo))
                .ToListAsync();

            if (productosExistentes.Any())
            {
                var codigosExistentes = productosExistentes.Select(p => p.Codigo).ToList();
                nuevosProductos.RemoveAll(p => codigosExistentes.Contains(p.Codigo));
                
                // Si después de remover duplicados no queda nada, informar al usuario
                if (nuevosProductos.Count == 0)
                {
                    return BadRequest("Todos los productos en el archivo ya existen en la base de datos.");
                }
            }

            // Insertar los productos
            await _db.productos.AddRangeAsync(nuevosProductos);
            await _db.SaveChangesAsync();

            return Ok(new { 
                Message = $"Se importaron {nuevosProductos.Count} productos exitosamente.",
                ProductosCreados = nuevosProductos.Count,
                ProductosOmitidos = productosExistentes.Count,
                Productos = nuevosProductos.Select(p => new {
                    p.Codigo,
                    p.Nombre,
                    p.Marca,
                    p.Stock,
                    p.SubCategoriaId,
                    p.CategoriaId
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar archivo para carga de productos");
            return StatusCode(500, $"Error al procesar el archivo: {ex.Message}");
        }
    }

    private async Task<List<productos>> ProcessCsvFile(IFormFile file)
    {
        var productos = new List<productos>();

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                // Mapeo de las columnas CSV a propiedades del producto
                csv.Context.RegisterClassMap<ProductoCsvMap>();
                var records = csv.GetRecords<ProductoCsvDto>().ToList();

                foreach (var record in records)
                {
                    productos.Add(new productos
                    {
                        Codigo = record.Codigo,
                        Nombre = record.Nombre,
                        Marca = record.Marca,
                        Stock = record.Stock ?? true,
                        SubCategoriaId = record.SubCategoriaId,
                        CategoriaId = record.CategoriaId,
                        Descripcion = record.Descripcion,
                        EsDestacado = record.EsDestacado ?? false,
                        Medidas = record.Medidas
                    });
                }
            }
        }

        return productos;
    }

    private async Task<List<productos>> ProcessExcelFile(IFormFile file)
    {
        var productos = new List<productos>();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                // Suponemos que la primera hoja contiene los datos
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                // Asumimos que la primera fila tiene encabezados
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var codigo = GetIntValue(worksheet.Cells[row, 1].Value);
                        
                        var nombre = GetStringValue(worksheet.Cells[row, 2].Value);
                        
                        // Si no hay código o nombre, saltamos esta fila
                        if (codigo == 0 || string.IsNullOrEmpty(nombre))
                            continue;

                        productos.Add(new productos
                        {
                            Codigo = codigo,
                            Nombre = nombre,
                            Marca = GetStringValue(worksheet.Cells[row, 3].Value),
                            Stock = GetBoolValue(worksheet.Cells[row, 4].Value, true),
                            SubCategoriaId = GetIntValue(worksheet.Cells[row, 5].Value),
                            CategoriaId = GetIntValue(worksheet.Cells[row, 6].Value),
                            Descripcion = GetStringValue(worksheet.Cells[row, 7].Value),
                            EsDestacado = GetBoolValue(worksheet.Cells[row, 8].Value, false),
                            Medidas = GetStringValue(worksheet.Cells[row, 9].Value)
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error al procesar fila {row} del archivo Excel: {ex.Message}");
                        // Continuamos con la siguiente fila
                    }
                }
            }
        }

        return productos;
    }

    private async Task<string> ValidarReferencias(List<productos> productos)
    {
        // Obtener todos los IDs de categorías y subcategorías presentes en los productos
        var categoriaIds = productos.Where(p => p.CategoriaId.HasValue)
            .Select(p => p.CategoriaId.Value).Distinct().ToList();
        var subcategoriaIds = productos.Select(p => p.SubCategoriaId).Distinct().ToList();

        // Verificar existencia de categorías
        var categoriasExistentes = await _db.categoria
            .Where(c => categoriaIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        // Verificar existencia de subcategorías
        var subcategoriasExistentes = await _db.subcategoria
            .Where(s => subcategoriaIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();

        // Identificar categorías y subcategorías que no existen
        var categoriasNoExistentes = categoriaIds.Except(categoriasExistentes).ToList();
        var subcategoriasNoExistentes = subcategoriaIds.Except(subcategoriasExistentes).ToList();

        // Construir mensaje de error si hay elementos no existentes
        if (categoriasNoExistentes.Any() || subcategoriasNoExistentes.Any())
        {
            var mensaje = "Se encontraron referencias a elementos que no existen en la base de datos:";
            
            if (categoriasNoExistentes.Any())
            {
                mensaje += $" Categorías: {string.Join(", ", categoriasNoExistentes)}.";
            }
            
            if (subcategoriasNoExistentes.Any())
            {
                mensaje += $" Subcategorías: {string.Join(", ", subcategoriasNoExistentes)}.";
            }
            
            return mensaje;
        }

        return string.Empty;
    }

    // Métodos auxiliares para procesar valores de Excel
    private int GetIntValue(object value)
    {
        if (value == null)
            return 0;

        if (int.TryParse(value.ToString(), out int result))
            return result;

        return 0;
    }

    private string GetStringValue(object value)
    {
        return value?.ToString() ?? string.Empty;
    }

    private bool GetBoolValue(object value, bool defaultValue)
    {
        if (value == null)
            return defaultValue;

        if (bool.TryParse(value.ToString(), out bool result))
            return result;

        // También verificar si es un valor numérico (1=true, 0=false)
        if (int.TryParse(value.ToString(), out int numericResult))
            return numericResult != 0;

        return defaultValue;
    }
}

// Clase auxiliar para mapeo de CSV
public class ProductoCsvDto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Marca { get; set; }
    public bool? Stock { get; set; }
    public int SubCategoriaId { get; set; }
    public int? CategoriaId { get; set; }
    public string? Descripcion { get; set; }
    public bool? EsDestacado { get; set; }
    public string? Medidas { get; set; }
}

// Mapeo de CSV a clase DTO
public class ProductoCsvMap : ClassMap<ProductoCsvDto>
{
    public ProductoCsvMap()
    {
        Map(m => m.Codigo).Name("Codigo");
        Map(m => m.Nombre).Name("Nombre");
        Map(m => m.Marca).Name("Marca");
        Map(m => m.Stock).Name("Stock");
        Map(m => m.SubCategoriaId).Name("SubCategoriaId");
        Map(m => m.CategoriaId).Name("CategoriaId");
        Map(m => m.Descripcion).Name("Descripcion");
        Map(m => m.EsDestacado).Name("EsDestacado");
        Map(m => m.Medidas).Name("Medidas");
    }
}