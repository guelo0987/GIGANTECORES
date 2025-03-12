using System;
using System.Collections.Generic;

namespace GIGANTECORE.Models;

public partial class Solicitud
{
    public int IdSolicitud { get; set; }

    public int UsuarioId { get; set; }

    public DateTime? FechaSolicitud { get; set; }

    public virtual ICollection<DetalleSolicitud> DetalleSolicituds { get; set; } = new List<DetalleSolicitud>();

    public virtual usuario_cliente Usuario { get; set; } = null!;
}
