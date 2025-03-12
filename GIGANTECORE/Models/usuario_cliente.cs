using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIGANTECORE.Models;

public partial class usuario_cliente
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? Ciudad { get; set; }

    public string? Apellidos { get; set; }

    public string? Telefono { get; set; }

    public string? Rnc { get; set; }

    public DateOnly? Dob { get; set; }

    public DateTime? FechaIngreso { get; set; }

    [ForeignKey("Role")]
    public int RolId { get; set; }
    public virtual roles Role { get; set; } = null!;

    public virtual ICollection<Carrito> Carritos { get; set; } = new List<Carrito>();

    public virtual ICollection<HistorialCorreo> HistorialCorreos { get; set; } = new List<HistorialCorreo>();

    public virtual Compañium? RncNavigation { get; set; }

    public virtual ICollection<Solicitud> Solicituds { get; set; } = new List<Solicitud>();
}
