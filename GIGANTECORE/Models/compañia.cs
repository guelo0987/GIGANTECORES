using System;
using System.Collections.Generic;

namespace GIGANTECORE.Models;

public partial class compañia
{
    public string Rnc { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<usuario_cliente> UsuarioClientes { get; set; } = new List<usuario_cliente>();
}
