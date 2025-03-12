using GIGANTECORE.Context;
using GIGANTECORE.Models;
using Microsoft.AspNetCore.Http;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GIGANTECORE.Utils;

public class AdminMultiMedia
{
    private readonly MyDbContext _context;
    private readonly string _bucketName;
    private readonly string _folderPath;

    public AdminMultiMedia(MyDbContext context, IConfiguration configuration)
    {
        _context = context;
        _bucketName = Environment.GetEnvironmentVariable("GOOGLE_STORAGE_BUCKET");
        _folderPath = Environment.GetEnvironmentVariable("BANNER_IMAGES_PATH");

    }

    public async Task<object> Upload(IFormFile file)
    {
        try
        {
            List<string> validar = new List<string>{".jpg", ".png", ".jpeg"};
            string extension = Path.GetExtension(file.FileName);

            if (!validar.Contains(extension))
            {
                return new { success = false, message = $"Archivo no permitido: {string.Join(',', validar)}" };
            }

            long size = file.Length;
            if (size > (5 * 1024 * 1024))
            {
                return new { success = false, message = "El archivo no puede sobrepasar los 5mb" };
            }

            string fileName = $"{_folderPath}{Guid.NewGuid().ToString()}{extension}";
            var storageClient = await StorageClient.CreateAsync();

            using (var stream = file.OpenReadStream())
            {
                await storageClient.UploadObjectAsync(
                    _bucketName,
                    fileName,
                    GetContentType(extension),
                    stream);
            }

            int newOrderIndex = 1;
            try {
                newOrderIndex = _context.banner.Max(b => (int?)b.OrderIndex) + 1 ?? 1;
            }
            catch {
                // Si hay algún error, usar 1 como valor predeterminado
            }

            var banner = new banner()
            {
                ImageUrl = fileName,
                Active = true,
                OrderIndex = newOrderIndex,
                CreatedAt = DateTime.UtcNow
            };

            _context.banner.Add(banner);
            await _context.SaveChangesAsync();

            return new { success = true, fileName = fileName };
        }
        catch (Exception ex)
        {
            string errorMessage = ex.Message;
            if (ex.InnerException != null)
            {
                errorMessage += $" Inner exception: {ex.InnerException.Message}";
            }
            return new { success = false, message = $"Error al subir archivo: {errorMessage}" };
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var banner = await _context.banner.FindAsync(id);
            if (banner == null) return false;

            var storageClient = await StorageClient.CreateAsync();
            await storageClient.DeleteObjectAsync(_bucketName, banner.ImageUrl);

            _context.banner.Remove(banner);
            await _context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GetContentType(string extension)
    {
        return extension.ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }

    // Los demás métodos permanecen igual ya que no manejan archivos
    public bool ReorderImages(List<(int id, int newOrder)> newOrders)
    {
        try
        {
            foreach (var (id, newOrder) in newOrders)
            {
                var banner = _context.banner.Find(id);
                if (banner != null)
                {
                    banner.OrderIndex = newOrder;
                }
            }
            
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public List<banner> GetImages()
    {
        return _context.banner
            .OrderBy(b => b.OrderIndex)
            .ToList();
    }

    public bool ToggleActive(int id)
    {
        try
        {
            var banner = _context.banner.Find(id);
            if (banner == null) return false;

            banner.Active = !banner.Active;
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }
}