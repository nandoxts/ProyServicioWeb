namespace ProyMvcProyectoOnline205.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }

        public string? Nombre { get; set; }

        public string? Apellidos { get; set; }

        public string Correo { get; set; } = null!;

        public string? PasswordHash { get; set; }

        public bool? Reestablecer { get; set; }

        public string? Telefono { get; set; }

        public bool? Activo { get; set; }

        public DateTime? FechaRegistro { get; set; }
    }
}
