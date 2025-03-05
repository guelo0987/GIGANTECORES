using System;
using System.Collections.Generic;

namespace GIGANTECORE.Models;

public partial class Carrito
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int ProductoId { get; set; }

    public int Cantidad { get; set; }

    public virtual Producto Producto { get; set; } = null!;

    public virtual UsuarioCliente Usuario { get; set; } = null!;
}
