using System;
using System.Collections.Generic;

namespace ProyApiProyectoOnline2025.Models;

public partial class ActivityLog
{
    public int IdLog { get; set; }

    public int? IdUsuario { get; set; }

    public string? Accion { get; set; }

    public string? Detalles { get; set; }

    public DateTime? Fecha { get; set; }
}
