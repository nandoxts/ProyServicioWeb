using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyApiProyectoOnline2025.Models;

namespace ProyApiProyectoOnline2025.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketApiController : ControllerBase
    {
        private readonly Proyectodiciembre2025Context db;

        public TicketApiController(Proyectodiciembre2025Context _db)
        {
            db = _db;
        }

        // ===========================================
        // 1) LISTAR TICKETS
        // ===========================================
        [HttpGet("GetTickets")]
        public async Task<ActionResult<List<Ticket>>> GetTickets()
        {
            var lista = await db.Tickets
                                .OrderByDescending(t => t.FechaCreacion)
                                .ToListAsync();
            return lista;
        }

        // ===========================================
        // 2) OBTENER TICKET POR ID
        // ===========================================
        [HttpGet("GetTicket/{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            var ticket = await db.Tickets.FindAsync(id);

            if (ticket == null)
                return NotFound("No existe el ticket con ID: " + id);

            return ticket;
        }

        // ===========================================
        // 3) CLIENTE CREA UN TICKET
        // ===========================================
        [HttpPost("PostTicket")]
        public async Task<ActionResult<string>> PostTicket([FromBody] Ticket value)
        {
            try
            {
                value.Estado = "Abierto";
                value.FechaCreacion = DateTime.Now;

                await db.Tickets.AddAsync(value);
                await db.SaveChangesAsync();

                // Registrar LOG
                await RegistrarLog(value.IdTicket, value.IdClienteOrigen, "Creación", "Ticket creado por el cliente");

                return "Ticket registrado correctamente.";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.InnerException?.Message;
            }
        }

        // ===========================================
        // 4) ADMIN RESPONDE UN TICKET
        // ===========================================
        [HttpPut("ResponderTicket")]
        public async Task<ActionResult<string>> ResponderTicket([FromBody] TicketRespuestaRequest req)
        {
            var ticket = await db.Tickets.FindAsync(req.IdTicket);

            if (ticket == null)
                return NotFound("Ticket no encontrado.");

            // VALIDAR USUARIO (staff) QUE RESPONDE
            var usuarioExistente = await db.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == req.IdUsuarioGestion && u.Activo == true);

            if (usuarioExistente == null)
                return BadRequest("El usuario que responde no existe o está inactivo.");

            // RESPUESTA DEL ADMIN
            ticket.IdUsuarioGestion = req.IdUsuarioGestion;
            ticket.RespuestaAdmin = req.Respuesta;
            ticket.Estado = "Respondido";

            await db.SaveChangesAsync();

            await RegistrarLog(
                ticket.IdTicket,
                req.IdUsuarioGestion,
                "Respuesta",
                req.Respuesta
            );

            // Notificacion al CLIENTE (tabla NOTIFICACION_CLIENTE).
            // OJO: el SP ssp_Notificacion_Insertar es para tabla USUARIO (staff).
            // Aqui el destinatario es un Cliente, asi que insertamos directo via EF.
            // Se hace despues del SaveChanges del ticket y dentro de un try
            // para que un fallo de notificacion no rompa la respuesta del ticket.
            if (ticket.IdClienteOrigen != null)
            {
                try
                {
                    db.NotificacionClientes.Add(new NotificacionCliente
                    {
                        IdCliente     = ticket.IdClienteOrigen.Value,
                        Mensaje       = "Tu ticket ha sido respondido. Revisa tu bandeja de tickets.",
                        EsLeido       = false,
                        FechaCreacion = DateTime.Now
                    });
                    await db.SaveChangesAsync();
                }
                catch
                {
                    // Notificacion es side-effect: si falla no debe revertir la respuesta.
                }
            }

            return Ok("Respuesta registrada exitosamente.");
        }

        // ===========================================
        // 5) ADMIN CIERRA TICKET
        // ===========================================
        [HttpPut("CerrarTicket/{id}")]
        public async Task<ActionResult<string>> CerrarTicket(int id)
        {
            var ticket = await db.Tickets.FindAsync(id);

            if (ticket == null)
                return NotFound("Ticket no encontrado.");

            ticket.Estado = "Cerrado";
            ticket.FechaCierre = DateTime.Now;

            await db.SaveChangesAsync();

            await RegistrarLog(ticket.IdTicket, ticket.IdUsuarioGestion ?? 0, "Cierre", "Ticket cerrado");

            return "Ticket cerrado correctamente.";
        }
    

        // ===========================================
        // 6) REGISTRAR LOG EN TICKET_LOG
        // ===========================================
        private async Task RegistrarLog(int idTicket, int? idUsuario, string accion, string detalle)
        {
            var log = new TicketLog
            {
                IdTicket = idTicket,
                IdUsuario = idUsuario,
                Accion = accion,
                Comentario = detalle,
                Fecha = DateTime.Now
            };

            await db.TicketLogs.AddAsync(log);
            await db.SaveChangesAsync();
        }

    }

    // ===========================================
    // MODELOS AUXILIARES
    // ===========================================
    public class TicketRespuestaRequest
    {
        public int IdTicket { get; set; }
        public int IdUsuarioGestion { get; set; }
        public string Respuesta { get; set; } = "";
    }

    public class TicketCerrarRequest
    {
        public int IdTicket { get; set; }
        public int IdUsuarioGestion { get; set; }
    }
}
