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
        // ===========================================
        [HttpPost("PostCliente")]
        public async Task<ActionResult<string>> PostCliente([FromBody] Cliente value)
        {
            try
            {
                // Buscar cliente por correo (dato único natural)
                var existente = await db.Clientes
                    .FirstOrDefaultAsync(c => c.Correo == value.Correo);

                // Si NO existe → Registrar normal
                if (existente == null)
                {
                    value.Activo = true;
                    value.FechaRegistro = DateTime.Now;

                    await db.Clientes.AddAsync(value);
                    await db.SaveChangesAsync();

                    return $"Cliente {value.Nombre} registrado correctamente.";
                }

                // Si existe pero está desactivado → REACTIVAR
                if (existente.Activo == false)
                {
                    existente.Nombre = value.Nombre;
                    existente.Apellidos = value.Apellidos;
                    existente.Telefono = value.Telefono;
                    existente.PasswordHash = value.PasswordHash;
                    existente.Reestablecer = false;
                    existente.Activo = true;

                    await db.SaveChangesAsync();

                    return $"Cliente reactivado: {existente.Nombre}";
                }

                // Si ya existe y está activo → Error
                return BadRequest("El correo ya está registrado en un cliente activo.");
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.InnerException?.Message;
            }
        }   

        // ===========================================
        // 4) ACTUALIZAR DATOS DE CLIENTE
        // PUT: api/ClienteApi/PutCliente
        // ===========================================
        [HttpPut("PutCliente")]
        public async Task<ActionResult<string>> PutCliente([FromBody] Cliente value)
        {
            try
            {
                var buscado = await db.Clientes.FindAsync(value.IdCliente);

                if (buscado == null)
                    return NotFound("Cliente no encontrado");

                db.Entry(buscado).State = EntityState.Detached;
                db.Entry(value).State = EntityState.Modified;

                await db.SaveChangesAsync();

                return $"Se actualizaron los datos del cliente: {value.Nombre}";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.InnerException?.Message;
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
