using ProyApiProyectoOnline2025.Models;
using System.ComponentModel.DataAnnotations.Schema;

public partial class Ventum
{
    public int IdVenta { get; set; }

    public int IdCliente { get; set; }

    public int? TotalProducto { get; set; }

    public decimal? MontoTotal { get; set; }

    public string? Contacto { get; set; }

    public string? IdDistrito { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public string? IdTransaccion { get; set; }

    public DateTime? FechaVenta { get; set; }

    public int? IdUsuarioEmpleado { get; set; }


    public int IdEstadoPedido { get; set; }   // 👈 FK REAL

    [ForeignKey(nameof(IdEstadoPedido))]      // 👈 CLAVE
    public virtual EstadoPedido IdEstadoPedidoNavigation { get; set; }   // 👈
    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } // 👈


    public virtual ICollection<Pago> Pagos { get; set; }
        = new List<Pago>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Distrito? IdDistritoNavigation { get; set; }

    public virtual Usuario? IdUsuarioEmpleadoNavigation { get; set; }
}
