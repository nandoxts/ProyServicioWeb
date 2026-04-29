namespace ProyMvcProyectoOnline205.Models
{
    public class NotificacionCliente
    {
        public int IdNotificacion { get; set; }
        public int IdCliente { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public bool EsLeido { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

}
