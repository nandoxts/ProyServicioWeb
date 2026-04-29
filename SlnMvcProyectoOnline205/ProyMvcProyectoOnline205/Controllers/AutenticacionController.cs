using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        public IActionResult Login()
        {
            return View();
        }

        // =========================
        // LOGIN (POST)
        // =========================
        [HttpPost]
        public async Task<IActionResult> Login(string correo, string password)
        {
            using var http = _httpClientFactory.CreateClient();

            // Intentar login como CLIENTE
            var clienteLogin = new
            {
                Correo = correo,
                PasswordHash = password
            };

            var jsonCliente = JsonConvert.SerializeObject(clienteLogin);
            var contentCliente = new StringContent(jsonCliente, Encoding.UTF8, "application/json");

            var respCliente = await http.PostAsync(_apiClienteUrl + "Login", contentCliente);

            if (respCliente.IsSuccessStatusCode)
            {
                var data = await respCliente.Content.ReadAsStringAsync();
                var cliente = JsonConvert.DeserializeObject<Cliente>(data);

                HttpContext.Session.SetInt32("IdCliente", cliente!.IdCliente);
                HttpContext.Session.SetInt32("IdRol", 3);
                HttpContext.Session.SetString("Nombre", cliente.Nombre!);

                // 🟢 PASO 1: inicializar contadores
                HttpContext.Session.SetInt32("NotiTickets", 0);

                return RedirectToAction("Index", "Home");
            }

            // Intentar login como USUARIO (ADMIN)
            var usuarioLogin = new
            {
                Correo = correo,
                PasswordHash = password
            };

            var jsonUsuario = JsonConvert.SerializeObject(usuarioLogin);
            var contentUsuario = new StringContent(jsonUsuario, Encoding.UTF8, "application/json");

            var respUsuario = await http.PostAsync(_apiUsuarioUrl + "Login", contentUsuario);

            if (respUsuario.IsSuccessStatusCode)
            {
                var data = await respUsuario.Content.ReadAsStringAsync();
                var usuario = JsonConvert.DeserializeObject<Usuario>(data);

                HttpContext.Session.SetInt32("IdUsuario", usuario!.IdUsuario);
                HttpContext.Session.SetInt32("IdRol", usuario.IdRol);
                HttpContext.Session.SetString("Nombre", usuario.Nombres!);

                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Correo o contraseña incorrectos";
            return View();
        }

        // =========================
        // REGISTRO CLIENTE (GET)
        // =========================
        public IActionResult Registro()
        {
            return View(new Cliente());
        }

        // =========================
        // REGISTRO CLIENTE (POST)
        // =========================
        [HttpPost]
        public async Task<IActionResult> Registro(Cliente obj)
        {
            using var http = _httpClientFactory.CreateClient();

            obj.Activo = true;
            obj.Reestablecer = false;

            var json = JsonConvert.SerializeObject(obj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await http.PostAsync(_apiClienteUrl + "PostCliente", content);

            if (resp.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Error = await resp.Content.ReadAsStringAsync();
            return View(obj);
        }

        // =========================
        // LOGOUT
        // =========================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
