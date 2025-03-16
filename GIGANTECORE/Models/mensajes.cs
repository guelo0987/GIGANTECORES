namespace GIGANTECORE.Models;

public class mensajes
{
    
    
    public int id { get; set; }

   
    public string email { get; set; }


    public string descripcion { get; set; }

    public DateTime fecha_creada { get; set; } = DateTime.UtcNow;
    
    public string estado { get; set; } = "pendiente";
    
}