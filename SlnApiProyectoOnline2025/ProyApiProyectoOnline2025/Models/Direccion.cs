using System;
using System.Collections.Generic;

namespace ProyApiProyectoOnline2025.Models;

public partial class Direccion
{
    public int IdDireccion { get; set; }

    public int IdCliente { get; set; }

    public string? Line1 { get; set; }

    public string? Line2 { get; set; }

    public string? IdDepartamento { get; set; }

    public string? IdProvincia { get; set; }

    public string? IdDistrito { get; set; }

    public string? CodigoPostal { get; set; }

    public bool? EsPredeterminada { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
