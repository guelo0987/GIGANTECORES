namespace GIGANTECORE.DTO;

public class HistorialCorreoDTO
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int SolicitudId { get; set; }
    public DateTime? FechaEnvio { get; set; }
    public string? Estado { get; set; }
}
