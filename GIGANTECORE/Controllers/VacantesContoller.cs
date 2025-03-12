using GIGANTECORE.Context;
using GIGANTECORE.DTO;
using GIGANTECORE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GIGANTECORE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VacantesController : ControllerBase
    {
        private readonly ILogger<VacantesController> _logger;
        private readonly MyDbContext _db;
        
        public VacantesController(MyDbContext db, ILogger<VacantesController> logger)
        {
            _db = db;
            _logger = logger;
        }
        
        // Nuevo método GET para obtener todas las vacantes
        [HttpGet]
        public async Task<IActionResult> GetAllVacantes()
        {
            var vacantes = await _db.vacantes.ToListAsync();
            return Ok(vacantes);
        }

        
        // GET: api/vacantes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVacanteById(int id)
        {
            var vacante = await _db.vacantes.FirstOrDefaultAsync(v => v.id == id);
            if (vacante == null)
            {
                _logger.LogError("Vacante con Id {Id} no encontrada", id);
                return NotFound($"Vacante con Id {id} no encontrada.");
            }
            
            return Ok(MapToDto(vacante));
        }
        
        // GET: api/vacantes/filtrar
        // Se pueden filtrar por: nombre, cedula, estado, nivel academico y funcion laboral.
        [HttpGet("filtrar")]
        public async Task<IActionResult> FiltrarVacantes(
            [FromQuery] string nombre,
            [FromQuery] string cedula,
            [FromQuery] string estado,
            [FromQuery] string nivelAcademico,
            [FromQuery] string funcionLaboral)
        {
            var query = _db.vacantes.AsQueryable();
            
            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(v => v.nombre.Contains(nombre));
            
            if (!string.IsNullOrEmpty(cedula))
                query = query.Where(v => v.cedula.Contains(cedula));
            
            if (!string.IsNullOrEmpty(estado))
                query = query.Where(v => v.estado.Equals(estado, StringComparison.OrdinalIgnoreCase));
            
            if (!string.IsNullOrEmpty(nivelAcademico))
                query = query.Where(v => v.NivelAcademico.Contains(nivelAcademico));
            
            if (!string.IsNullOrEmpty(funcionLaboral))
                query = query.Where(v => v.FuncionLaboral.Contains(funcionLaboral));
            
            var vacantes = await query.ToListAsync();
            
            if (!vacantes.Any())
            {
                _logger.LogInformation("No se encontraron vacantes con los filtros proporcionados.");
                return NotFound("No se encontraron vacantes con los filtros proporcionados.");
            }
            
            var vacantesDto = vacantes.Select(v => MapToDto(v)).ToList();
            return Ok(vacantesDto);
        }
        
        // Método privado para mapear de Vacante a VacanteDto
        private VacanteDto MapToDto(vacantes vacante)
        {
            return new VacanteDto
            {
                id = vacante.id,
                nombre = vacante.nombre,
                cedula = vacante.cedula,
                Correo = vacante.Correo,
                telefono = vacante.telefono,
                sexo = vacante.sexo,
                NivelAcademico = vacante.NivelAcademico,
                AnosExperiencia = vacante.AnosExperiencia,
                FuncionLaboral = vacante.FuncionLaboral,
                OtraFuncionLaboral = vacante.OtraFuncionLaboral,
                UltimoSalario = vacante.UltimoSalario,
                NivelLaboral = vacante.NivelLaboral,
                OtroNivelLaboral = vacante.OtroNivelLaboral,
                CurriculumUrl = vacante.CurriculumUrl,
                FechaAplicacion = vacante.FechaAplicacion,
                estado = vacante.estado
            };
        }
    }
}
