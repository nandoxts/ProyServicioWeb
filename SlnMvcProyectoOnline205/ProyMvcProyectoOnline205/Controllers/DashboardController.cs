using Microsoft.AspNetCore.Mvc;
using ProyMvcProyectoOnline205.Filters;
using ProyMvcProyectoOnline205.Models;
using System.Text.Json;

namespace ProyMvcProyectoOnline205.Controllers
{
    [RoleAuthorize(Roles.Admin, Roles.Vendedor)] // Panel administrativo
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _config;

        public DashboardController(IHttpClientFactory http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            using var client = _http.CreateClient();

            async Task<int> Count<T>(string url)
            {
                try
                {
                    var resp = await client.GetAsync(url);
                    if (!resp.IsSuccessStatusCode) return 0;
                    var json = await resp.Content.ReadAsStringAsync();
                    var list = JsonSerializer.Deserialize<List<T>>(json, opts);
                    return list?.Count ?? 0;
                }
                catch { return 0; }
            }

            var base64 = _config["ApiSettings:ClienteApiBaseUrl"]
                         ?? "http://localhost:5064/api/ClienteApi/";
            var apiBase = "http://localhost:5064/api/";

            var tasks = new[]
            {
                Count<Producto>($"{apiBase}ProductoApi/GetProductos"),
                Count<Usuario>($"{apiBase}UsuarioApi/GetUsuarios"),
                Count<Cliente>($"{apiBase}ClienteApi/GetClientes"),
                Count<Pedido>($"{apiBase}VentaApi/GetVentas"),
                Count<Ticket>($"{apiBase}TicketApi/GetTickets"),
            };

            await Task.WhenAll(tasks);

            ViewBag.TotalProductos = tasks[0].Result;
            ViewBag.TotalUsuarios  = tasks[1].Result;
            ViewBag.TotalClientes  = tasks[2].Result;
            ViewBag.TotalPedidos   = tasks[3].Result;
            ViewBag.TotalTickets   = tasks[4].Result;

            return View();
        }
    }
}
