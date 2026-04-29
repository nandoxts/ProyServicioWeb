namespace ProyMvcProyectoOnline205.Models
{
    public class TicketResponseRequest
    {
        public int IdTicket { get; set; }
        public string Respuesta { get; set; } = string.Empty;
        public int IdUsuarioGestion { get; set; }
    }
}