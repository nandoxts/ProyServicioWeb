using Microsoft.AspNetCore.Mvc;
using ProyMvcProyectoOnline205.Filters;
using ProyMvcProyectoOnline205.Models;
using System.Text;

namespace ProyMvcProyectoOnline205.Controllers
{
    [RoleAuthorize(Roles.Admin, Roles.Vendedor, Roles.Cliente)] // Mixto: cliente o staff
    public class NotificacionController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _http;

        public NotificacionController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _http = httpClientFactory;
            _configuration = configuration;
        }
        private HttpClient GetHttpClient(string baseUrl)
        {
            var client = _http.CreateClient();
            client.BaseAddress = new Uri(baseUrl);
            return client;
        }
        // ==============================
        // LISTAR NOTIFICACIONES

        public async Task<IActionResult> Index()
        {
          
            var rol = HttpContext.Session.GetInt32("IdRol");

            if (rol == null)
                return RedirectToAction("Login", "Autenticacion");

            using var client = _http.CreateClient();

            // ======================
            // CLIENTE
            // ======================
            if (rol == 3)
            {
                var idCliente = HttpContext.Session.GetInt32("IdCliente");
                var url = _configuration["ApiSettings:NotificacionClienteApi"];

                var resp = await client.GetAsync($"{url}PorCliente/{idCliente}");

                var lista = resp.IsSuccessStatusCode
                    ? await resp.Content.ReadFromJsonAsync<List<NotificacionCliente>>() ?? new()
                    : new List<NotificacionCliente>();


                ViewBag.LimpiarBadge = true;


                // 🔥 CLAVE
                return View("IndexCliente", lista);
            }

            // ======================
            // ADMIN / EMPLEADO
            // ======================
            var adminUrl = _configuration["ApiSettings:NotificacionAdminApi"];
            var respAdmin = await client.GetAsync($"{adminUrl}GetNotificaciones");

            var listaAdmin = respAdmin.IsSuccessStatusCode
                ? await respAdmin.Content.ReadFromJsonAsync<List<Notificacion>>() ?? new()
                : new List<Notificacion>();

            // 🔥 CLAVE
            return View("IndexAdmin", listaAdmin);
        }
        public async Task<IActionResult> DetailsNotificacion(int id)
        {
            var baseUrl = _configuration["ApiSettings:NotificacionAdminApi"];
            using var http = GetHttpClient(baseUrl);

            var resp = await http.GetAsync($"GetNotificacion/{id}");

            if (!resp.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var noti = await resp.Content.ReadFromJsonAsync<Notificacion>();
            return View(noti);
        }

        public async Task<IActionResult> MarcarLeida(int id)
        {
            var baseUrl = _configuration["ApiSettings:NotificacionAdminApi"];
            using var http = GetHttpClient(baseUrl);

            await http.PutAsync(
                $"MarcarLeido/{id}",
                new StringContent("{}", Encoding.UTF8, "application/json")
            );

            return RedirectToAction(nameof(Index));
        }


    }
}
