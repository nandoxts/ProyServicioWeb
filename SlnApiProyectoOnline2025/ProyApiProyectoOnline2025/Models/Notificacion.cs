using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProyApiProyectoOnline2025.Models;

public partial class Notificacion
{
    public int IdNotificacion { get; set; }

    public int IdUsuario { get; set; }

    public string Mensaje { get; set; } = null!;

    public bool? EsLeido { get; set; }

    public DateTime? FechaCreacion { get; set; }

    [JsonIgnore]
    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
