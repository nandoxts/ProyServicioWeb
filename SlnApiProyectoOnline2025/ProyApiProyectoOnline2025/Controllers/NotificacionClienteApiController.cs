using Microsoft.AspNetCore.Mvc;
using ProyApiProyectoOnline2025.Models;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProyApiProyectoOnline2025.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionClienteApiController : ControllerBase
    {
        private readonly Proyectodiciembre2025Context db;

        public NotificacionClienteApiController(Proyectodiciembre2025Context context)
        {
            db = context;
        }

        // 🔔 Listar notificaciones del cliente
        [HttpGet("PorCliente/{idCliente}")]
        public async Task<IActionResult> GetNotificaciones(int idCliente)
        {
            var notificaciones = await db.NotificacionClientes
                .Where(n => n.IdCliente == idCliente)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();

            return Ok(notificaciones);
        }

        // ✅ Marcar como leída
        [HttpPut("MarcarLeida/{id}")]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            var noti = await db.NotificacionClientes.FindAsync(id);
            if (noti == null) return NotFound();

            noti.EsLeido = true;
            await db.SaveChangesAsync();

            return Ok();
        }

        // 🔢 CONTAR NOTIFICACIONES NO LEÍDAS
        [HttpGet("NoLeidas/{idCliente}")]
        public async Task<IActionResult> ContarNoLeidas(int idCliente)
        {
            var total = await db.NotificacionClientes
                .CountAsync(n => n.IdCliente == idCliente && n.EsLeido == false);

            return Ok(total);
        }
        [HttpPut("MarcarTodasLeidas/{idCliente}")]
        public async Task<IActionResult> MarcarTodasLeidas(int idCliente)
        {
            var notis = await db.NotificacionClientes
                .Where(n => n.IdCliente == idCliente && n.EsLeido == false)
                .ToListAsync();

            foreach (var n in notis)
                n.EsLeido = true;

            await db.SaveChangesAsync();
            return Ok();
        }


    }


}
