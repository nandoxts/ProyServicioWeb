using System;
using System.Collections.Generic;

namespace ProyApiProyectoOnline2025.Models;

public partial class ListaDeseo
{
    public int IdListaDeseos { get; set; }

    public int IdCliente { get; set; }

    public int IdProducto { get; set; }

    public DateTime? FechaGuardado { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
