using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GIGANTECORE.Models;

public partial class productos
{
    public int Codigo { get; set; }

    
    public string Nombre { get; set; } = null!;

    public string? Marca { get; set; }

    public bool? Stock { get; set; }

    public int SubCategoriaId { get; set; }

    public string? ImageUrl { get; set; }

    public int? CategoriaId { get; set; }
    
    public string? Descripcion { get; set; }
    
    public bool? EsDestacado { get; set; }

    public string? Medidas { get; set; }
    public virtual ICollection<Carrito> Carritos { get; set; } = new List<Carrito>();

    public virtual categoria? Categoria { get; set; }

    public virtual ICollection<DetalleSolicitud> DetalleSolicituds { get; set; } = new List<DetalleSolicitud>();

    public virtual subcategoria SubCategoria { get; set; } = null!;
}
