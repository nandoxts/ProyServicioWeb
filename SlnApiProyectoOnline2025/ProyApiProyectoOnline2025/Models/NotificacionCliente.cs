using System;
using System.Collections.Generic;

namespace ProyApiProyectoOnline2025.Models;

public partial class NotificacionCliente
{
    public int IdNotificacion { get; set; }

    public int IdCliente { get; set; }

    public string Mensaje { get; set; } = null!;

    public bool? EsLeido { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
