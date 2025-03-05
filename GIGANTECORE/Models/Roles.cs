using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIGANTECORE.Models;

public partial class Roles
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdRol { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = null!;
    
    public virtual ICollection<Admin> Admins { get; set; } = new List<Admin>();
    public virtual ICollection<UsuarioCliente> UsuarioClientes { get; set; } = new List<UsuarioCliente>();
    public virtual ICollection<RolePermiso> RolePermisos { get; set; } = new List<RolePermiso>();
}