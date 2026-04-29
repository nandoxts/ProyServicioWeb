namespace ProyMvcProyectoOnline205.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public string Correo { get; set; } = "";
        public string? PasswordHash { get; set; }
        public bool? Reestablecer { get; set; }
        public bool? Activo { get; set; }
        public DateTime? FechaRegistro { get; set; }
    }
}
