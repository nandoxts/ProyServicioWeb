namespace ProyMvcProyectoOnline205.Models
{
    public class Notificacion
    {
        public int IdNotificacion { get; set; }
        public int? IdUsuario { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public bool? EsLeido { get; set; }
        public DateTime? FechaCreacion { get; set; }
    }
}
