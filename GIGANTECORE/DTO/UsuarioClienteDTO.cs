namespace GIGANTECORE.DTO;

public class UsuarioClienteDTO
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Apellidos { get; set; }
    public string? Telefono { get; set; }
    public string? Rnc { get; set; }
}
