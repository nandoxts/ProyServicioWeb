using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyMvcProyectoOnline205.Filters;
using ProyMvcProyectoOnline205.Models;
using System.Text;

namespace ProyMvcProyectoOnline205.Controllers
{
    [RoleAuthorize(Roles.Admin)] // Solo el Administrador gestiona staff
    public class UsuarioController : Controller
    {
        // Usamos IHttpClientFactory para manejar la creación y ciclo de vida de HttpClient.
        // Esto es esencial para prevenir el agotamiento de sockets en el servidor.
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseApiUrl = "http://localhost:5064/api/UsuarioApi/";

        // Inyección de dependencia de IHttpClientFactory
        public UsuarioController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // =========================
        // MÉTODO PRIVADO: Obtiene la instancia de HttpClient
        // =========================
        private HttpClient GetHttpClient() => _httpClientFactory.CreateClient();

        // =========================
        // TRAER USUARIOS
        // =========================
        private async Task<List<Usuario>> TraerUsuarios()
        {
            using var http = GetHttpClient();
            var resp = await http.GetAsync(_baseApiUrl + "GetUsuarios");

            // MEJORA: Validar el estado de la respuesta
            if (resp.IsSuccessStatusCode)
            {
                var data = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Usuario>>(data) ?? new List<Usuario>();
            }
            // Si falla, devuelve una lista vacía para evitar errores en la vista.
            return new List<Usuario>();
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> IndexUsuario()
        {
            return View(await TraerUsuarios());
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> DetailsUsuario(int id)
        {
            using var http = GetHttpClient();
            var resp = await http.GetAsync(_baseApiUrl + $"GetUsuario/{id}");

            if (resp.IsSuccessStatusCode)
            {
                var data = await resp.Content.ReadAsStringAsync();
                return View(JsonConvert.DeserializeObject<Usuario>(data));
            }

            // Si falla (ej. 404), redirige a la lista o muestra un error.
            TempData["mensaje"] = "Error: No se pudo encontrar el usuario solicitado.";
            return RedirectToAction(nameof(IndexUsuario));
        }

        // =========================
        // CREATE - GET
        // =========================
        public IActionResult CreateUsuario()
        {
            return View(new Usuario());
        }

        // =========================
        // CREATE - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken] // Buena práctica de seguridad
        public async Task<IActionResult> CreateUsuario(Usuario obj)
        {
            if (!ModelState.IsValid)
            {
                return View(obj);
            }

            using var http = GetHttpClient();

            // Configuración automática de campos
            obj.Activo = true;
            obj.Reestablecer = false;

            var json = JsonConvert.SerializeObject(obj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await http.PostAsync(_baseApiUrl + "PostUsuario", content);

            // MEJORA: Manejo explícito de la respuesta de la API
            if (resp.IsSuccessStatusCode)
            {
                TempData["mensaje"] = "Usuario registrado exitosamente.";
                return RedirectToAction(nameof(IndexUsuario));
            }
            else
            {
                // Capturar el mensaje de error de la API (si lo proporciona)
                var errorContent = await resp.Content.ReadAsStringAsync();
                TempData["mensaje"] = $"Error al registrar el usuario: {errorContent}";
                // Regresa a la vista con los datos para que el usuario pueda corregir.
                return View(obj);
            }
        }

        // =========================
        // EDIT - GET
        // =========================
        public async Task<IActionResult> EditUsuario(int id)
        {
            using var http = GetHttpClient();
            var resp = await http.GetAsync(_baseApiUrl + $"GetUsuario/{id}");

            if (resp.IsSuccessStatusCode)
            {
                var data = await resp.Content.ReadAsStringAsync();
                return View(JsonConvert.DeserializeObject<Usuario>(data));
            }

            TempData["mensaje"] = "Error: No se pudo cargar el usuario para edición.";
            return RedirectToAction(nameof(IndexUsuario));
        }

        // =========================
        // EDIT - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUsuario(Usuario obj)
        {
            if (!ModelState.IsValid)
                return View(obj);

            var idUsuarioActual = HttpContext.Session.GetInt32("IdUsuario");
            using var http = GetHttpClient();

            // 1) PROTECCIÓN: Si te estás editando A TI MISMO, no puedes
            //    desactivarte ni quitarte el rol de admin (te quedarías sin acceso).
            if (idUsuarioActual == obj.IdUsuario)
            {
                if (obj.Activo == false)
                {
                    TempData["error"] = "No puedes desactivar tu propia cuenta.";
                    return View(obj);
                }
                if (obj.IdRol != Roles.Admin)
                {
                    TempData["error"] = "No puedes quitarte a ti mismo el rol de administrador.";
                    return View(obj);
                }
            }

            // 2) PROTECCIÓN: Si la edición deja al sistema sin ningún admin activo, bloquear.
            //    (admin → vendedor, o admin → inactivo, siendo el último admin activo)
            var todos = await TraerUsuarios();
            var original = todos.FirstOrDefault(u => u.IdUsuario == obj.IdUsuario);
            bool eraAdminActivo = original?.IdRol == Roles.Admin && original?.Activo == true;
            bool dejaDeSerAdminActivo =
                eraAdminActivo && (obj.IdRol != Roles.Admin || obj.Activo != true);

            if (dejaDeSerAdminActivo)
            {
                int otrosAdminsActivos = todos.Count(u =>
                    u.IdUsuario != obj.IdUsuario &&
                    u.IdRol == Roles.Admin &&
                    u.Activo == true);

                if (otrosAdminsActivos == 0)
                {
                    TempData["error"] = "No puedes dejar al sistema sin un administrador activo.";
                    return View(obj);
                }
            }

            // 3) Enviar al API (que ya valida correo duplicado y preserva campos sensibles)
            var json = JsonConvert.SerializeObject(obj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await http.PutAsync(_baseApiUrl + "PutUsuario", content);

            if (resp.IsSuccessStatusCode)
            {
                TempData["success"] = "Usuario actualizado correctamente.";
                return RedirectToAction(nameof(IndexUsuario));
            }

            var errorContent = await resp.Content.ReadAsStringAsync();
            TempData["error"] = $"Error al actualizar el usuario: {errorContent}";
            return View(obj);
        }

        // =========================
        // DELETE (lógico) - VISTA DE CONFIRMACIÓN (GET)
        // =========================
        // Muestra la vista de confirmación (si tienes una, sino se puede saltar este paso)
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            using var http = GetHttpClient();
            var resp = await http.GetAsync(_baseApiUrl + $"GetUsuario/{id}");

            if (resp.IsSuccessStatusCode)
            {
                var data = await resp.Content.ReadAsStringAsync();
                return View(JsonConvert.DeserializeObject<Usuario>(data)); // Asume que tienes una vista DeleteUsuario.cshtml
            }

            TempData["mensaje"] = "Error: No se pudo encontrar el usuario para eliminar.";
            return RedirectToAction(nameof(IndexUsuario));
        }


        // =========================
        // DELETE (lógico) - EJECUCIÓN (POST)
        // =========================
        [HttpPost, ActionName("DeleteUsuario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var idUsuarioActual = HttpContext.Session.GetInt32("IdUsuario");

            // PROTECCIÓN 1: no autoeliminarse
            if (idUsuarioActual == id)
            {
                TempData["error"] = "No puedes eliminar tu propia cuenta.";
                return RedirectToAction(nameof(IndexUsuario));
            }

            // PROTECCIÓN 2: no dejar al sistema sin admins activos
            var todos = await TraerUsuarios();
            var objetivo = todos.FirstOrDefault(u => u.IdUsuario == id);
            if (objetivo?.IdRol == Roles.Admin && objetivo?.Activo == true)
            {
                int otrosAdminsActivos = todos.Count(u =>
                    u.IdUsuario != id &&
                    u.IdRol == Roles.Admin &&
                    u.Activo == true);

                if (otrosAdminsActivos == 0)
                {
                    TempData["error"] = "No puedes eliminar al último administrador activo.";
                    return RedirectToAction(nameof(IndexUsuario));
                }
            }

            using var http = GetHttpClient();
            var resp = await http.DeleteAsync(_baseApiUrl + $"DeleteUsuario/{id}");

            if (resp.IsSuccessStatusCode)
            {
                TempData["success"] = "Usuario desactivado correctamente.";
            }
            else
            {
                var errorContent = await resp.Content.ReadAsStringAsync();
                TempData["error"] = $"Error al eliminar el usuario: {errorContent}";
            }

            return RedirectToAction(nameof(IndexUsuario));
        }
    }
}