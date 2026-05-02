using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyMvcProyectoOnline205.Filters;
using ProyMvcProyectoOnline205.Models;
using System.Text;

namespace ProyMvcProyectoOnline205.Controllers
{

    public class AutenticacionController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiClienteUrl = "http://localhost:5064/api/ClienteApi/";
        private readonly string _apiUsuarioUrl = "http://localhost:5064/api/UsuarioApi/";

        public AutenticacionController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // =========================
        // LOGIN (GET)
        // =========================
        [RequireAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // =========================
        // LOGIN (POST)
        // =========================
        [HttpPost]
        [RequireAnonymous]
        public async Task<IActionResult> Login(string correo, string password, string? returnUrl = null)
        {
            using var http = _httpClientFactory.CreateClient();

            var loginPayload = new
            {
                Correo = correo,
                PasswordHash = password
            };
            var json = JsonConvert.SerializeObject(loginPayload);

            // 1) Intentar primero como USUARIO (ADMIN / staff). Tiene prioridad
            //    porque un mismo correo puede existir en ambas tablas.
            var contentUsuario = new StringContent(json, Encoding.UTF8, "application/json");
            var respUsuario = await http.PostAsync(_apiUsuarioUrl + "Login", contentUsuario);

            if (respUsuario.IsSuccessStatusCode)
            {
                var data = await respUsuario.Content.ReadAsStringAsync();
                var usuario = JsonConvert.DeserializeObject<Usuario>(data);

                // Salvaguarda: si por algún motivo el API devuelve un rol no soportado,
                // no permitimos crear una sesión inconsistente.
                if (usuario!.IdRol != Roles.Admin && usuario.IdRol != Roles.Vendedor)
                {
                    ViewBag.Error = "Tu cuenta no tiene un rol válido. Contacta al administrador.";
                    ViewBag.ReturnUrl = returnUrl;
                    return View();
                }

                HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);
                HttpContext.Session.SetInt32("IdRol", usuario.IdRol);
                HttpContext.Session.SetString("Nombre", usuario.Nombres ?? "");
                HttpContext.Session.SetString("Correo", usuario.Correo ?? correo);

                // Staff siempre va al panel — ignora returnUrl si apunta a tienda.
                return RedirectToAction("Index", "Dashboard");
            }

            // 2) Fallback: intentar como CLIENTE
            var contentCliente = new StringContent(json, Encoding.UTF8, "application/json");
            var respCliente = await http.PostAsync(_apiClienteUrl + "Login", contentCliente);

            if (respCliente.IsSuccessStatusCode)
            {
                var data = await respCliente.Content.ReadAsStringAsync();
                var cliente = JsonConvert.DeserializeObject<Cliente>(data);

                HttpContext.Session.SetInt32("IdCliente", cliente!.IdCliente);
                HttpContext.Session.SetInt32("IdRol", Roles.Cliente);
                HttpContext.Session.SetString("Nombre", cliente.Nombre ?? "");
                HttpContext.Session.SetString("Correo", cliente.Correo ?? correo);
                HttpContext.Session.SetInt32("NotiTickets", 0);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Correo o contraseña incorrectos";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // =========================
        // REGISTRO CLIENTE (GET)
        // =========================
        [RequireAnonymous]
        public IActionResult Registro()
        {
            return View(new Cliente());
        }

        // =========================
        // REGISTRO CLIENTE (POST)
        // =========================
        [HttpPost]
        [RequireAnonymous]
        public async Task<IActionResult> Registro(Cliente obj)
        {
            // 1) Validaciones basicas server-side (no confiar solo en HTML required)
            if (string.IsNullOrWhiteSpace(obj.Nombre))
            {
                ViewBag.Error = "El nombre es obligatorio.";
                return View(obj);
            }
            if (string.IsNullOrWhiteSpace(obj.Apellidos))
            {
                ViewBag.Error = "Los apellidos son obligatorios.";
                return View(obj);
            }
            if (string.IsNullOrWhiteSpace(obj.Correo))
            {
                ViewBag.Error = "El correo es obligatorio.";
                return View(obj);
            }
            if (string.IsNullOrWhiteSpace(obj.PasswordHash))
            {
                ViewBag.Error = "La contrasena es obligatoria.";
                return View(obj);
            }
            if (obj.PasswordHash.Length < 6)
            {
                ViewBag.Error = "La contrasena debe tener al menos 6 caracteres.";
                return View(obj);
            }

            // Normalizar correo antes de enviarlo
            obj.Correo = obj.Correo.Trim().ToLowerInvariant();

            obj.Activo = true;
            obj.Reestablecer = false;

            using var http = _httpClientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(obj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage resp;
            try
            {
                resp = await http.PostAsync(_apiClienteUrl + "PostCliente", content);
            }
            catch (Exception)
            {
                ViewBag.Error = "No se pudo conectar con el servidor. Intentalo de nuevo.";
                return View(obj);
            }

            var body = (await resp.Content.ReadAsStringAsync())?.Trim() ?? "";
            // El API a veces devuelve "ERROR: ..." con HTTP 200 cuando atrapa excepciones,
            // por eso no basta con mirar IsSuccessStatusCode: tambien hay que leer el body.
            bool falloLogico = body.StartsWith("ERROR", StringComparison.OrdinalIgnoreCase);

            if (!resp.IsSuccessStatusCode || falloLogico)
            {
                // Limpia comillas dobles que añade JSON cuando el endpoint devuelve string
                var limpio = body.Trim('"');
                ViewBag.Error = string.IsNullOrEmpty(limpio)
                    ? "No se pudo registrar el cliente."
                    : limpio;
                return View(obj);
            }

            // Exito: mensaje visible en el Login
            TempData["RegistroOk"] = "Cuenta creada correctamente. Inicia sesion con tu correo y contrasena.";
            return RedirectToAction("Login");
        }

        // =========================
        // LOGOUT
        // =========================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Autenticacion");
        }
    }
}
