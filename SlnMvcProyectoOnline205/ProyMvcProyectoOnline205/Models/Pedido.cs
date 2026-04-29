namespace ProyMvcProyectoOnline205.Models
{
    public class Pedido
    {
        public int IdVenta { get; set; }
        public int IdCliente { get; set; }
        public int? TotalProducto { get; set; }
        public decimal? MontoTotal { get; set; }
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaVenta { get; set; }
    }
}
