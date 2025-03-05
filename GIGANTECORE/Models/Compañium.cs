using System;
using System.Collections.Generic;

namespace GIGANTECORE.Models;

public partial class Compañium
{
    public string Rnc { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<UsuarioCliente> UsuarioClientes { get; set; } = new List<UsuarioCliente>();
}
