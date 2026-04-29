using System;
using System.Collections.Generic;

namespace ProyApiProyectoOnline2025.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string? Nombre { get; set; }

    public string? Apellidos { get; set; }

    public string Correo { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public bool? Reestablecer { get; set; }

    public string? Telefono { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaRegistro { get; set; }    

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Direccion> Direccions { get; set; } = new List<Direccion>();

    public virtual ICollection<ListaDeseo> ListaDeseos { get; set; } = new List<ListaDeseo>();

    public virtual ICollection<NotificacionCliente> NotificacionClientes { get; set; } = new List<NotificacionCliente>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
