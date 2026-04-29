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
        // ============================================================
        [HttpPost("PostUsuario")]
        public async Task<ActionResult<string>> PostUsuario([FromBody] Usuario value)
        {
            try
            {
                // Buscar por correo (dato único)
                var existente = await db.Usuarios
                    .FirstOrDefaultAsync(u => u.Correo == value.Correo);

                if (existente != null)
                {
                    if (existente.Activo == false)
                    {
                        // 🔥 REACTIVAR USUARIO
                        existente.Activo = true;
                        existente.Reestablecer = true;
                        existente.Nombres = value.Nombres;
                        existente.Apellidos = value.Apellidos;
                        existente.IdRol = value.IdRol;
                        existente.PasswordHash = value.PasswordHash;

                        await db.SaveChangesAsync();

                        return "Usuario reactivado correctamente.";
                    }

                    return BadRequest("El correo ya está registrado y activo.");
                }

                // 🆕 NUEVO USUARIO
                value.Activo = true;
                value.Reestablecer = false;
                value.FechaRegistro = DateTime.Now;

                await db.Usuarios.AddAsync(value);
                await db.SaveChangesAsync();

                return "Usuario registrado exitosamente.";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.InnerException?.Message;
            }
        }


        // ============================================================
        // 4) ACTUALIZAR DATOS DE USUARIO
        // PUT: api/UsuarioApi/PutUsuario
        // ============================================================
        [HttpPut("PutUsuario")]
        public async Task<ActionResult<string>> PutUsuario([FromBody] Usuario value)
        {
            try
            {
                var buscado = await db.Usuarios.FindAsync(value.IdUsuario);

                if (buscado == null)
                    return NotFound("Usuario no encontrado");

                db.Entry(buscado).State = EntityState.Detached;
                db.Entry(value).State = EntityState.Modified;

                await db.SaveChangesAsync();

                return $"Datos del usuario {value.Nombres} actualizados.";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.InnerException?.Message;
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
