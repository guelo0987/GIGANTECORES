using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIGANTECORE.Models;

public partial class roles
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdRol { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = null!;
    
    public virtual ICollection<admin> Admins { get; set; } = new List<admin>();
    public virtual ICollection<usuario_cliente> UsuarioClientes { get; set; } = new List<usuario_cliente>();
    public virtual ICollection<rolepermisos> RolePermisos { get; set; } = new List<rolepermisos>();
}