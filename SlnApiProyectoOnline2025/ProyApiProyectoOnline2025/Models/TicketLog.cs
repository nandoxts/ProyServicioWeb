using System;
using System.Collections.Generic;

namespace ProyApiProyectoOnline2025.Models;

public partial class TicketLog
{
    public int IdTicketLog { get; set; }

    public int IdTicket { get; set; }

    public int? IdUsuario { get; set; }

    public string? Accion { get; set; }

    public string? Comentario { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual Ticket IdTicketNavigation { get; set; } = null!;
}
