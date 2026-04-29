namespace ProyMvcProyectoOnline205.Models
{
    public class DetalleVentum
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }

        public Producto? IdProductoNavigation { get; set; }
    }
}
