using System;
using System.Collections.Generic;

namespace ProyApiProyectoOnline2025.Models;

public partial class ProductoImagen
{
    public int IdImagen { get; set; }

    public int IdProducto { get; set; }

    public string Url { get; set; } = null!;

    public string? NombreArchivo { get; set; }

    public bool? EsPrincipal { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
