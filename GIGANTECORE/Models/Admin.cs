using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIGANTECORE.Models;

public partial class Admin
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Mail { get; set; } = null!;

    public string Password { get; set; } = null!;

    [ForeignKey("Role")]
    public int? RolId { get; set; }
    public virtual Roles? Role { get; set; }

    public DateTime? FechaIngreso { get; set; }

    public string? Telefono { get; set; }

    public bool? SoloLectura { get; set; }
}
