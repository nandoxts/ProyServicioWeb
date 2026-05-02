using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyApiProyectoOnline2025.Models;

namespace ProyApiProyectoOnline2025.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioApiController : ControllerBase
    {
        private readonly Proyectodiciembre2025Context db;

        public UsuarioApiController(Proyectodiciembre2025Context _db)
        {
            db = _db;
        }

        // ============================================================
        // 1) LISTAR USUARIOS
        // GET: api/UsuarioApi/GetUsuarios
        // ============================================================
        [HttpGet("GetUsuarios")]
        public async Task<ActionResult<List<Usuario>>> GetUsuarios()
        {
            if (db.Usuarios == null)
                return NotFound();

            var lista = await db.Usuarios
                .Include(r => r.IdRolNavigation)
                .Where(u => u.Activo == true)
                .ToListAsync();

            return lista;
        }

        // ============================================================
        // 2) OBTENER USUARIO POR ID
        // GET: api/UsuarioApi/GetUsuario/5
        // ============================================================
        [HttpGet("GetUsuario/{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var buscado = await db.Usuarios.FindAsync(id);

            if (buscado == null)
                return NotFound($"No existe el usuario con ID {id}");

            return buscado;
        }

        // ============================================================
        // 3) REGISTRAR USUARIO
        // POST: api/UsuarioApi/PostUsuario
        //
        // Validaciones:
        //  • Correo no vacío y formato válido.
        //  • Correo NO existe en CLIENTE (cross-tabla).
        //  • Correo NO existe en otro USUARIO activo.
        //  • Si existe en USUARIO pero inactivo → reactiva.
        // ============================================================
        [HttpPost("PostUsuario")]
        public async Task<ActionResult<string>> PostUsuario([FromBody] Usuario value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value.Correo))
                    return BadRequest("El correo es obligatorio.");

                value.Correo = value.Correo.Trim().ToLowerInvariant();

                if (!EsCorreoValido(value.Correo))
                    return BadRequest("El formato del correo no es válido.");

                // 1) CROSS-TABLA: no puede coexistir en CLIENTE
                bool enCliente = await db.Clientes
                    .AnyAsync(c => c.Correo == value.Correo);

                if (enCliente)
                    return BadRequest("Ese correo ya está registrado como cliente.");

                // 2) Mismo correo en USUARIO
                var existente = await db.Usuarios
                    .FirstOrDefaultAsync(u => u.Correo == value.Correo);

                if (existente != null)
                {
                    if (existente.Activo == false)
                    {
                        existente.Activo        = true;
                        existente.Reestablecer  = true;
                        existente.Nombres       = value.Nombres;
                        existente.Apellidos     = value.Apellidos;
                        existente.IdRol         = value.IdRol;
                        existente.PasswordHash  = value.PasswordHash;

                        await db.SaveChangesAsync();
                        return "Usuario reactivado correctamente.";
                    }

                    return BadRequest("Ese correo ya está registrado.");
                }

                // 3) Nuevo usuario
                value.Activo        = true;
                value.Reestablecer  = false;
                value.FechaRegistro = DateTime.Now;

                await db.Usuarios.AddAsync(value);
                await db.SaveChangesAsync();

                return "Usuario registrado exitosamente.";
            }
            catch (Exception ex)
            {
                return "ERROR: " + (ex.InnerException?.Message ?? ex.Message);
            }
        }

        private static bool EsCorreoValido(string correo)
        {
            try { var addr = new System.Net.Mail.MailAddress(correo); return addr.Address == correo; }
            catch { return false; }
        }


        // ============================================================
        // 4) ACTUALIZAR DATOS DE USUARIO
        // PUT: api/UsuarioApi/PutUsuario
        //
        // Validaciones al cambiar correo:
        //  • No vacío y formato válido.
        //  • No usado por OTRO usuario.
        //  • No usado por NINGÚN cliente (cross-tabla).
        // ============================================================
        [HttpPut("PutUsuario")]
        public async Task<ActionResult<string>> PutUsuario([FromBody] Usuario value)
        {
            try
            {
                var buscado = await db.Usuarios.FindAsync(value.IdUsuario);

                if (buscado == null)
                    return NotFound("Usuario no encontrado");

                if (string.IsNullOrWhiteSpace(value.Correo))
                    return BadRequest("El correo es obligatorio.");

                value.Correo = value.Correo.Trim().ToLowerInvariant();

                if (!EsCorreoValido(value.Correo))
                    return BadRequest("El formato del correo no es válido.");

                // ¿Cambió el correo? Solo entonces valida choques
                if (!string.Equals(buscado.Correo, value.Correo, StringComparison.OrdinalIgnoreCase))
                {
                    bool chocaConOtroUsuario = await db.Usuarios
                        .AnyAsync(u => u.Correo == value.Correo && u.IdUsuario != value.IdUsuario);
                    if (chocaConOtroUsuario)
                        return BadRequest("Ese correo ya está usado por otro usuario.");

                    bool chocaConCliente = await db.Clientes
                        .AnyAsync(c => c.Correo == value.Correo);
                    if (chocaConCliente)
                        return BadRequest("Ese correo ya está usado por un cliente.");
                }

                // Preservar campos sensibles desde BD (no confiar en el cliente).
                // Esto permite que el form NO envíe PasswordHash ni FechaRegistro.
                value.PasswordHash  = buscado.PasswordHash;
                value.FechaRegistro = buscado.FechaRegistro;

                db.Entry(buscado).State = EntityState.Detached;
                db.Entry(value).State = EntityState.Modified;

                await db.SaveChangesAsync();

                return $"Datos del usuario {value.Nombres} actualizados.";
            }
            catch (Exception ex)
            {
                return "ERROR: " + (ex.InnerException?.Message ?? ex.Message);
            }
        }

        // ============================================================
        // 5) ELIMINAR LÓGICO
        // DELETE: api/UsuarioApi/DeleteUsuario/5
        // ============================================================
        [HttpDelete("DeleteUsuario/{id}")]
        public async Task<ActionResult<string>> DeleteUsuario(int id)
        {
            var buscado = await db.Usuarios.FindAsync(id);

            if (buscado == null)
                return NotFound("Usuario no encontrado");

            buscado.Activo = false;
            await db.SaveChangesAsync();

            return $"Usuario {id} desactivado correctamente.";
        }

        // ============================================================
        // 6) LOGIN USUARIO (SEGURIDAD EMPLEADO/ADMIN)
        // POST: api/UsuarioApi/Login
        // ============================================================
        [HttpPost("Login")]
        public async Task<ActionResult<Usuario>> Login([FromBody] LoginRequest req)
        {
            var encontrado = await db.Usuarios
                .Where(u => u.Correo == req.Correo &&
                            u.PasswordHash == req.PasswordHash &&
                            u.Activo == true)
                .FirstOrDefaultAsync();

            if (encontrado == null)
                return Unauthorized("Usuario o contraseña incorrecta.");

            return encontrado;
        }

        // ============================================================
        // 7) RESTABLECER CONTRASEÑA
        // POST: api/UsuarioApi/ReestablecerClave
        // ============================================================
        [HttpPost("ResetClaveUsuarioRequest")]
        public async Task<ActionResult<string>> ResetClaveUsuarioRequest([FromBody] ResetClaveUsuarioRequest req)
        {
            var user = await db.Usuarios.FindAsync(req.IdUsuario);

            if (user == null)
                return NotFound("Usuario no encontrado.");

            user.PasswordHash = req.NuevaClave;
            user.Reestablecer = false;

            await db.SaveChangesAsync();

            return "Clave actualizada correctamente.";
        }
    }

    // Modelos auxiliares
    public class LoginUsuarioRequest
    {
        public string Correo { get; set; } = "";
        public string PasswordHash { get; set; } = "";
    }

    public class ResetClaveUsuarioRequest
    {
        public int IdUsuario { get; set; }
        public string NuevaClave { get; set; } = "";
    }
}
