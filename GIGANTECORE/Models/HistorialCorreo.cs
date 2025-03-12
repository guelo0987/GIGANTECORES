using System;
using System.Collections.Generic;

namespace GIGANTECORE.Models;

public partial class HistorialCorreo
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int DetalleSolicitudId { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public string? Estado { get; set; }

    public virtual DetalleSolicitud DetalleSolicitud { get; set; } = null!;

    public virtual usuario_cliente Usuario { get; set; } = null!;
}
