using System;
using System.Collections.Generic;

namespace GIGANTECORE.Models;

public partial class carrito
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int ProductoId { get; set; }

    public int Cantidad { get; set; }

    public virtual productos Productos { get; set; } = null!;

    public virtual usuario_cliente Usuario { get; set; } = null!;
}
