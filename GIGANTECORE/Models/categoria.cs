using System;
using System.Collections.Generic;

namespace GIGANTECORE.Models;

public partial class categoria
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<productos> Productos { get; set; } = new List<productos>();

    public virtual ICollection<subcategoria> SubCategoria { get; set; } = new List<subcategoria>();
}
