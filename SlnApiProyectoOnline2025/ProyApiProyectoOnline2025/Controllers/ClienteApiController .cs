using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyApiProyectoOnline2025.Models;

namespace ProyApiProyectoOnline2025.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteApiController : ControllerBase
    {
        private readonly Proyectodiciembre2025Context db;

        public ClienteApiController(Proyectodiciembre2025Context _db)
        {
            db = _db;
        }

        // ===========================================
        // 1) LISTAR CLIENTES
        // GET: api/ClienteApi/GetClientes
        // ===========================================
        [HttpGet("GetClientes")]
        public async Task<ActionResult<List<Cliente>>> GetClientes()
        {
            if (db.Clientes == null)
                return NotFound();

            var lista = await db.Clientes
                                .Where(c => c.Activo == true)
                                .ToListAsync();

            return lista;
        }

        // ===========================================
        // 2) OBTENER CLIENTE POR ID
        // GET: api/ClienteApi/GetCliente/5
        // ===========================================
        [HttpGet("GetCliente/{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var buscado = await db.Clientes.FindAsync(id);

            if (buscado == null)
                return NotFound($"No existe el cliente con ID: {id}");

            return buscado;
        }

        // ===========================================
        // 3) REGISTRAR CLIENTE
        // POST: api/ClienteApi/PostCliente
        //
        // Validaciones:
        //  • Correo no vacío y formato válido.
        //  • Correo NO existe en USUARIO (cross-tabla).
        //  • Correo NO existe en otro CLIENTE activo.
        //  • Si existe en CLIENTE pero inactivo → reactiva.
        // ===========================================
        [HttpPost("PostCliente")]
        public async Task<ActionResult<string>> PostCliente([FromBody] Cliente value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value.Correo))
                    return BadRequest("El correo es obligatorio.");

                value.Correo = value.Correo.Trim().ToLowerInvariant();

                if (!EsCorreoValido(value.Correo))
                    return BadRequest("El formato del correo no es válido.");

                // 1) CROSS-TABLA: no puede coexistir en USUARIO (admin/empleado)
                bool enUsuario = await db.Usuarios
                    .AnyAsync(u => u.Correo == value.Correo);

                if (enUsuario)
                    return BadRequest("Ese correo ya está registrado como personal interno.");

                // 2) Mismo correo en CLIENTE
                var existente = await db.Clientes
                    .FirstOrDefaultAsync(c => c.Correo == value.Correo);

                if (existente == null)
                {
                    value.Activo = true;
                    value.FechaRegistro = DateTime.Now;

                    await db.Clientes.AddAsync(value);
                    await db.SaveChangesAsync();

                    return $"Cliente {value.Nombre} registrado correctamente.";
                }

                // 3) Existe pero inactivo → reactivar
                if (existente.Activo == false)
                {
                    existente.Nombre        = value.Nombre;
                    existente.Apellidos     = value.Apellidos;
                    existente.Telefono      = value.Telefono;
                    existente.PasswordHash  = value.PasswordHash;
                    existente.Reestablecer  = false;
                    existente.Activo        = true;

                    await db.SaveChangesAsync();
                    return $"Cliente reactivado: {existente.Nombre}";
                }

                // 4) Ya existe y está activo
                return BadRequest("Ese correo ya está registrado.");
            }
            catch (Exception ex)
            {
                return "ERROR: " + (ex.InnerException?.Message ?? ex.Message);
            }
        }

        // Helper local para validar formato. Evita dependencia de DataAnnotations.
        private static bool EsCorreoValido(string correo)
        {
            try { var addr = new System.Net.Mail.MailAddress(correo); return addr.Address == correo; }
            catch { return false; }
        }

        // ===========================================
        // 4) ACTUALIZAR DATOS DE CLIENTE
        // PUT: api/ClienteApi/PutCliente
        //
        // Validaciones al cambiar correo:
        //  • No vacío y formato válido.
        //  • No usado por OTRO cliente (mismo IdCliente puede mantener su correo).
        //  • No usado por NINGÚN usuario (cross-tabla).
        // ===========================================
        [HttpPut("PutCliente")]
        public async Task<ActionResult<string>> PutCliente([FromBody] Cliente value)
        {
            try
            {
                var buscado = await db.Clientes.FindAsync(value.IdCliente);

                if (buscado == null)
                    return NotFound("Cliente no encontrado");

                if (string.IsNullOrWhiteSpace(value.Correo))
                    return BadRequest("El correo es obligatorio.");

                value.Correo = value.Correo.Trim().ToLowerInvariant();

                if (!EsCorreoValido(value.Correo))
                    return BadRequest("El formato del correo no es válido.");

                // ¿Cambió de correo? Solo entonces valida choques
                if (!string.Equals(buscado.Correo, value.Correo, StringComparison.OrdinalIgnoreCase))
                {
                    bool chocaConOtroCliente = await db.Clientes
                        .AnyAsync(c => c.Correo == value.Correo && c.IdCliente != value.IdCliente);
                    if (chocaConOtroCliente)
                        return BadRequest("Ese correo ya está usado por otro cliente.");

                    bool chocaConUsuario = await db.Usuarios
                        .AnyAsync(u => u.Correo == value.Correo);
                    if (chocaConUsuario)
                        return BadRequest("Ese correo ya está usado por personal interno.");
                }

                db.Entry(buscado).State = EntityState.Detached;
                db.Entry(value).State = EntityState.Modified;

                await db.SaveChangesAsync();

                return $"Se actualizaron los datos del cliente: {value.Nombre}";
            }
            catch (Exception ex)
            {
                return "ERROR: " + (ex.InnerException?.Message ?? ex.Message);
            }
        }

        // ===========================================
        // 5) ELIMINAR (LÓGICO)
        // DELETE: api/ClienteApi/DeleteCliente/5
        // ===========================================
        [HttpDelete("DeleteCliente/{id}")]
        public async Task<ActionResult<string>> DeleteCliente(int id)
        {
            var buscado = await db.Clientes.FindAsync(id);

            if (buscado == null)
                return NotFound("Cliente no encontrado");

            buscado.Activo = false;
            await db.SaveChangesAsync();

            return $"Cliente {id} desactivado correctamente.";
        }

        // ===========================================
        // 6) LOGIN DE CLIENTE
        // POST: api/ClienteApi/Login
        // ===========================================
        [HttpPost("Login")]
        public async Task<ActionResult<Cliente>> Login([FromBody] LoginRequest req)
        {
            var encontrado = await db.Clientes
                .Where(c => c.Correo == req.Correo &&
                            c.PasswordHash == req.PasswordHash &&
                            c.Activo == true)
                .FirstOrDefaultAsync();

            if (encontrado == null)
                return Unauthorized("Correo o contraseña incorrecta.");

            return encontrado;
        }

        // ===========================================
        // 7) RESTABLECER CONTRASEÑA
        // POST: api/ClienteApi/ReestablecerClave
        // ===========================================
        [HttpPost("ReestablecerClave")]
        public async Task<ActionResult<string>> ReestablecerClave([FromBody] ResetClaveRequest req)
        {
            var cliente = await db.Clientes.FindAsync(req.IdCliente);

            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            cliente.PasswordHash = req.NuevaClave;
            cliente.Reestablecer = false;

            await db.SaveChangesAsync();

            return "Contraseña actualizada correctamente.";
        }
    }

    // ===========================================================
    // MODELOS EXTRAS PARA LOGIN Y RESET (NO VIENEN EN LA BD)
    // ===========================================================
    public class LoginRequest
    {
        public string Correo { get; set; }
        public string PasswordHash { get; set; }
    }

    public class ResetClaveRequest
    {
        public int IdCliente { get; set; }
        public string NuevaClave { get; set; }
    }
}
