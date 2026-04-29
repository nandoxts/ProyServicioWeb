using System;
using System.Collections.Generic;

namespace ProyApiProyectoOnline2025.Models;

public partial class Pago
{
    public int IdPago { get; set; }

    public int IdVenta { get; set; }

    public string? MetodoPago { get; set; }

    public decimal Monto { get; set; }

    public string? Moneda { get; set; }

    public string? Estado { get; set; }

    public string? ReferenciaPago { get; set; }

    public DateTime? FechaPago { get; set; }

    public virtual Ventum IdVentaNavigation { get; set; } = null!;
}
