using System;
using System.Collections.Generic;

namespace GIGANTECORE.Models;

public partial class subcategoria
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int CategoriaId { get; set; }

    public virtual categoria Categoria { get; set; } = null!;

    public virtual ICollection<productos> Productos { get; set; } = new List<productos>();
}
