namespace ProyMvcProyectoOnline205.Models
{
    public class Producto
    {
        public int IdProducto { get; set; }

        public string? Nombre { get; set; }

        public string? Descripcion { get; set; }

        public int? IdMarca { get; set; }

        public int? IdCategoria { get; set; }

        public decimal? Precio { get; set; }

        public int? Stock { get; set; }

        public string? RutaImagen { get; set; }

        public string? NombreImagen { get; set; }

        public bool? Activo { get; set; }

        public DateTime? FechaRegistro { get; set; }
        public Categorium? IdCategoriaNavigation { get; set; }
        public Marca? IdMarcaNavigation { get; set; }
    }
}

