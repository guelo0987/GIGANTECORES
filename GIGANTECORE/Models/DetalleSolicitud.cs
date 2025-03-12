using System;
using System.Collections.Generic;

namespace GIGANTECORE.Models;

public partial class DetalleSolicitud
{
    public int IdDetalle { get; set; }

    public int IdSolicitud { get; set; }

    public int ProductoId { get; set; }

    public int Cantidad { get; set; }

    public virtual Solicitud IdSolicitudNavigation { get; set; } = null!;

    public virtual productos Productos { get; set; } = null!;

    public virtual ICollection<HistorialCorreo> HistorialCorreos { get; set; } = new List<HistorialCorreo>();
}
