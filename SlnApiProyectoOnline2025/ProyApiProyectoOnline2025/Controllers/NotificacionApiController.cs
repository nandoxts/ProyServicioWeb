using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyApiProyectoOnline2025.Models;

namespace ProyApiProyectoOnline2025.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionApiController : ControllerBase
    {
        private readonly Proyectodiciembre2025Context db;

        public NotificacionApiController(Proyectodiciembre2025Context _db)
        {
            db = _db;
        }

        // ============================================================
        // 1) LISTAR TODAS LAS NOTIFICACIONES
        // ============================================================
        [HttpGet("GetNotificaciones")]
        public async Task<ActionResult<List<Notificacion>>> GetNotificaciones()
        {
            return await db.Notificacions
                           .OrderByDescending(n => n.FechaCreacion)
                           .ToListAsync();
        }

        // ============================================================
        // 2) LISTAR NOTIFICACIONES POR USUARIO
        // ============================================================
        [HttpGet("GetNotificacionesPorUsuario/{idUsuario}")]
        public async Task<ActionResult<List<Notificacion>>> GetNotificacionesPorUsuario(int idUsuario)
        {
            var lista = await db.Notificacions
                .Where(n => n.IdUsuario == idUsuario)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();

            return lista;
        }

        // ============================================================
        // 3) OBTENER NOTIFICACIÓN POR ID
        // ============================================================
        [HttpGet("GetNotificacion/{id}")]
        public async Task<ActionResult<Notificacion>> GetNotificacion(int id)
        {
            var buscado = await db.Notificacions.FindAsync(id);

            if (buscado == null)
                return NotFound("No existe la notificación con ID: " + id);

            return buscado;
        }

        // ============================================================
        // 4) CREAR NOTIFICACIÓN
        // ============================================================
        [HttpPost("PostNotificacion")]
        public async Task<ActionResult<string>> PostNotificacion([FromBody] Notificacion value)
        {
            try
            {
                value.FechaCreacion = DateTime.Now;
                value.EsLeido = false;

                await db.Notificacions.AddAsync(value);
                await db.SaveChangesAsync();

                return "Notificación registrada correctamente.";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.InnerException?.Message;
            }
        }

        // ============================================================
        // 5) MARCAR NOTIFICACIÓN COMO LEÍDA
        // ============================================================
        [HttpPut("MarcarLeido/{id}")]
        public async Task<ActionResult<string>> MarcarLeido(int id)
        {
            var notif = await db.Notificacions.FindAsync(id);

            if (notif == null)
                return NotFound("Notificación no encontrada.");

            notif.EsLeido = true;
            await db.SaveChangesAsync();

            return "Notificación marcada como leída.";
        }

        // ============================================================
        // 6) ELIMINAR NOTIFICACIÓN
        // (Física, aunque podríamos hacer lógica si quieres)
        // ============================================================
        [HttpDelete("DeleteNotificacion/{id}")]
        public async Task<ActionResult<string>> DeleteNotificacion(int id)
        {
            var notif = await db.Notificacions.FindAsync(id);

            if (notif == null)
                return NotFound("Notificación no encontrada.");

            db.Notificacions.Remove(notif);
            await db.SaveChangesAsync();

            return "Notificación eliminada correctamente.";
        }

    }
}
