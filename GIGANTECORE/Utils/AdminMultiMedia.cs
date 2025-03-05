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

            var banner = new Banner
            {
                ImageUrl = fileName,
                Active = true,
                OrderIndex = _context.Banners.Max(b => (int?)b.OrderIndex) + 1 ?? 1
            };

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();

            return new { success = true, fileName = fileName };
        }
        catch (Exception ex)
        {
            return new { success = false, message = $"Error al subir archivo: {ex.Message}" };
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return false;

            var storageClient = await StorageClient.CreateAsync();
            await storageClient.DeleteObjectAsync(_bucketName, banner.ImageUrl);

            _context.Banners.Remove(banner);
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
                var banner = _context.Banners.Find(id);
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

    public List<Banner> GetImages()
    {
        return _context.Banners
            .OrderBy(b => b.OrderIndex)
            .ToList();
    }

    public bool ToggleActive(int id)
    {
        try
        {
            var banner = _context.Banners.Find(id);
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