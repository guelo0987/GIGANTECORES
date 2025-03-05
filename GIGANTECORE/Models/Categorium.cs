using System;
using System.Collections.Generic;

namespace GIGANTECORE.Models;

public partial class Categorium
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    public virtual ICollection<SubCategorium> SubCategoria { get; set; } = new List<SubCategorium>();
}
