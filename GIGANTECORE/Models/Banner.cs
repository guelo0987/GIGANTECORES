using System.ComponentModel.DataAnnotations;

namespace GIGANTECORE.Models;

public class Banner
{
    [Key]
    public int Id { get; set; }
        
    [Required]
    public string ImageUrl { get; set; }
        
    public bool Active { get; set; } = true;
        
    public int OrderIndex { get; set; }
        
    public DateTime CreatedAt { get; set; } = DateTime.Now;
        
   
}