using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIGANTECORE.Models;

public partial class RolePermiso
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdPermiso { get; set; }
    
    [ForeignKey("Role")]
    public int RoleId { get; set; }
    public virtual Roles Role { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string TableName { get; set; } = null!;
    public bool CanCreate { get; set; }
    public bool CanRead { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
}