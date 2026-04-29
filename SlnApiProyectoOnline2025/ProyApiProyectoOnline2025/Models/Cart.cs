using System;
using System.Collections.Generic;

namespace ProyApiProyectoOnline2025.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public int? IdCliente { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Cliente? IdClienteNavigation { get; set; }
}
