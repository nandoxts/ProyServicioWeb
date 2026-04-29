namespace ProyMvcProyectoOnline205.Models
{
    public class Ticket
    {
        public int IdTicket { get; set; }

        public string Asunto { get; set; } = null!;

        public string? Descripcion { get; set; }

        public int? IdClienteOrigen { get; set; }

        public int? IdUsuarioOrigen { get; set; }

        public string? Estado { get; set; }

        public int? IdUsuarioGestion { get; set; }

        public string? RespuestaAdmin { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public DateTime? FechaCierre { get; set; }

    }
}
