using System;
using System.Collections.Generic;

namespace ProyApiProyectoOnline2025.Models;

public partial class CartItem
{
    public int CartItemId { get; set; }

    public int CartId { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal UnitPrice { get; set; }

    public DateTime? AddedAt { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
