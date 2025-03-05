using Microsoft.AspNetCore.Mvc;
using GIGANTECORE.Context;
using GIGANTECORE.Models;
using GIGANTECORE.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace GIGANTECORE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BannerController : ControllerBase
    {
        private readonly MyDbContext _db;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly AdminMultiMedia _adminMultiMedia;

        public BannerController(MyDbContext db, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _db = db;
            _environment = environment;
            _configuration = configuration;
            _adminMultiMedia = new AdminMultiMedia(db, configuration);
        }

        [HttpGet]
        public IActionResult GetBanner()
        {
            return Ok(_adminMultiMedia.GetImages());
        }

        [HttpPost]
        public async Task<IActionResult> UploadBanner(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest(new { success = false, message = "No se ha proporcionado ningún archivo" });
            }

            var result = await _adminMultiMedia.Upload(file);
            return Ok(result);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var result = await _adminMultiMedia.Delete(id);
            
            if (!result)
            {
                return NotFound(new { success = false, message = "Banner no encontrado o no se pudo eliminar" });
            }

            return Ok(new { success = true, message = "Banner eliminado exitosamente" });
        }
        
        [HttpPut]
        public IActionResult ReorderBanners([FromBody] List<BannerOrderDto> newOrders)
        {
            var ordersList = newOrders.Select(x => (x.Id, x.NewOrder)).ToList();
            var result = _adminMultiMedia.ReorderImages(ordersList);
        
            if (!result)
            {
                return BadRequest("No se pudieron reordenar las imágenes");
            }
        
            return Ok("Imágenes reordenadas exitosamente");
        }

        [HttpPut("{id}")]
        public IActionResult ToggleActiveBanner(int id)
        {
            var result = _adminMultiMedia.ToggleActive(id);
        
            if (!result)
            {
                return NotFound("Banner no encontrado");
            }
        
            return Ok("Estado del banner actualizado exitosamente");
        }
    }
}