namespace ProyMvcProyectoOnline205.Models
{
    public class CartItem
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }

        public decimal Subtotal => Precio * Cantidad;
    }
}
