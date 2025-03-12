using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIGANTECORE.Models;

[Table("banner", Schema = "public")]
public class banner
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
        
    [Required]
    [Column("imageurl")]
    public string ImageUrl { get; set; }
        
    [Column("active")]
    public bool Active { get; set; } = true;
        
    [Column("orderindex")]
    public int OrderIndex { get; set; }
        
    [Column("createdat")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}