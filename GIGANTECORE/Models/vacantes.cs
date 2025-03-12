namespace GIGANTECORE.Models;

public class vacantes
{
    public int id { get; set; }
    
    public string nombre { get; set; }
    public string cedula { get; set; }
    public string Correo { get; set; }
    public string telefono { get; set; }
    public char sexo { get; set; }
    public string NivelAcademico { get; set; }
    public int? AnosExperiencia { get; set; }
    public string? FuncionLaboral { get; set; }
    public string? OtraFuncionLaboral { get; set; }
    public decimal? UltimoSalario { get; set; }
    public string? NivelLaboral { get; set; }
    public string? OtroNivelLaboral { get; set; }
    public string? CurriculumUrl { get; set; }
    public DateTime FechaAplicacion { get; set; }
    public string? estado { get; set; }
}