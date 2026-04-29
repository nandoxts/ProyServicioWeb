using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProyApiProyectoOnline2025.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    // Esta es la clave foránea, y es la que nos importa al insertar.
    public int IdRol { get; set; }

    public string? Nombres { get; set; }

    public string? Apellidos { get; set; }

    public string Correo { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public bool? Reestablecer { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaRegistro { get; set; }

    // SOLUCIÓN FINAL: 
    // 1. [JsonIgnore] evita ciclos en serialización (GET).
    // 2. 'Rol?' hace la propiedad NULABLE para que el Model Binder (POST) 
    //    no lance el error 400 cuando solo se envía el IdRol.
    [JsonIgnore]
    public virtual Rol? IdRolNavigation { get; set; } // <--- ¡CAMBIADO A NULABLE!

    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();

    public virtual ICollection<Ticket> TicketIdUsuarioGestionNavigations { get; set; } = new List<Ticket>();

    public virtual ICollection<Ticket> TicketIdUsuarioOrigenNavigations { get; set; } = new List<Ticket>();

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}