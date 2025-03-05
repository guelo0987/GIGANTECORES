using GIGANTECORE.Context;
using Microsoft.AspNetCore.Http;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GIGANTECORE.Utils
{
    public class AdminProductoMedia
    {
        private readonly MyDbContext _context;
        private readonly string _bucketName;
        private readonly string _folderPath;

        public AdminProductoMedia(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _bucketName = Environment.GetEnvironmentVariable("GOOGLE_STORAGE_BUCKET");
                
            _folderPath = Environment.GetEnvironmentVariable("PRODUCT_IMAGES_PATH");

        }

        public async Task<object> Upload(IFormFile file)
        {
            try
            {
                List<string> validExtensions = new List<string> { ".jpg", ".png", ".jpeg" };
                string extension = Path.GetExtension(file.FileName);

                if (!validExtensions.Contains(extension))
                {
                    return new { success = false, message = $"Archivo no permitido: {string.Join(", ", validExtensions)}" };
                }

                if (file.Length > (5 * 1024 * 1024))
                {
                    return new { success = false, message = "El archivo no puede sobrepasar los 5MB." };
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

                return new { success = true, fileName = fileName };
            }
            catch (Exception ex)
            {
                return new { success = false, message = $"Error al subir archivo: {ex.Message}" };
            }
        }

        public async Task<bool> Delete(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName)) return true;

                var storageClient = await StorageClient.CreateAsync();
                await storageClient.DeleteObjectAsync(_bucketName, fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<object> Update(IFormFile newFile, string oldFileName)
        {
            if (!string.IsNullOrEmpty(oldFileName))
            {
                await Delete(oldFileName);
            }

            return await Upload(newFile);
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
    }
}