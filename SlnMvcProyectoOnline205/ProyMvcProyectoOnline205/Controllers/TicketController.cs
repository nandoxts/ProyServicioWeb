using Microsoft.AspNetCore.Mvc;
using ProyMvcProyectoOnline205.Filters;
using System.Text;
using System.Text.Json;
using ProyMvcProyectoOnline205.Models; // Asumiendo que Ticket, TicketResponseRequest están aquí

namespace ProyMvcProyectoOnline205.Controllers
{
    [RoleAuthorize(Roles.Admin, Roles.Vendedor, Roles.Cliente)] // Cualquier usuario logueado
    public class TicketController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrlBase;
        private readonly string _apiEndpoint = "api/TicketApi/";

        // 1. MEJORA: Inyección de IConfiguration para leer la URL base
        public TicketController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            // Leer la URL base desde appsettings.json
            _apiUrlBase = configuration["ApiSettings:BaseUrl"] ?? throw new InvalidOperationException("ApiSettings:BaseUrl no configurada.");
        }

        // Método auxiliar para crear HttpClient y construir la URL completa
        private HttpClient GetHttpClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_apiUrlBase);
            return client;
        }

        // ==================================================
        // LISTAR TICKETS
        // ==================================================
        private async Task<List<Ticket>> TraerTickets()
        {
            using var http = GetHttpClient();
            var resp = await http.GetAsync("GetTickets");

            if (!resp.IsSuccessStatusCode)
                return new List<Ticket>();

            var data = await resp.Content.ReadAsStringAsync();
            // 2. MEJORA: Usar System.Text.Json
            return JsonSerializer.Deserialize<List<Ticket>>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }

        public async Task<IActionResult> IndexTicket()
        {
            var lista = await TraerTickets();

            int? rol = HttpContext.Session.GetInt32("IdRol");
            int? idCliente = HttpContext.Session.GetInt32("IdCliente");

            if (rol == 3)
            {
                lista = lista
                    .Where(t => t.IdClienteOrigen == idCliente)
                    .ToList();

                // 🟢 PASO 2: si hay tickets respondidos → subir campanita
                if (lista.Any(t => t.Estado == "Respondido"))
                {
                    HttpContext.Session.SetInt32("NotiTickets", 1);
                }
            }

            return View(lista);
        }

        // ==================================================
        // DETALLE DEL TICKET
        // ==================================================
        public async Task<IActionResult> DetailsTicket(int id)
        {

            using var http = GetHttpClient();
            var resp = await http.GetAsync($"GetTicket/{id}");

            if (!resp.IsSuccessStatusCode)
            {
                TempData["mensaje"] = "Ticket no encontrado.";
                return RedirectToAction(nameof(IndexTicket));
            }

            var data = await resp.Content.ReadAsStringAsync();
            // 2. MEJORA: Usar System.Text.Json
            var ticket = JsonSerializer.Deserialize<Ticket>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(ticket);
        }

        // ==================================================
        // CREAR TICKET (CLIENTE)
        // ==================================================
        [RoleAuthorize(Roles.Cliente)]
        public IActionResult CreateTicket()
        {
            return View(new Ticket());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles.Cliente)]
        public async Task<IActionResult> CreateTicket(Ticket obj)
        {
            // 🔵 NUEVO: asignar cliente desde sesión
            obj.IdClienteOrigen = HttpContext.Session.GetInt32("IdCliente");

            using var http = GetHttpClient();

            var json = JsonSerializer.Serialize(obj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await http.PostAsync("PostTicket", content);
            var message = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                TempData["mensaje"] = $"Éxito: {message}";
            }
            else if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                TempData["mensaje"] = $"Error de validación: {message}";
            }
            else
            {
                TempData["mensaje"] = $"Error al crear el ticket ({resp.StatusCode}): {message}";
            }

            return RedirectToAction(nameof(IndexTicket));
        }


        // ==================================================
        // RESPONDER TICKET (ADMIN)
        // ==================================================
        [RoleAuthorize(Roles.Admin, Roles.Vendedor)]
        public async Task<IActionResult> ResponderTicket(int id)
        {
            // Lógica de carga del ticket original (igual que Details)
            using var http = GetHttpClient();
            var resp = await http.GetAsync($"GetTicket/{id}");

            if (!resp.IsSuccessStatusCode)
                return RedirectToAction(nameof(IndexTicket));

            var data = await resp.Content.ReadAsStringAsync();
            // 2. MEJORA: Usar System.Text.Json
            var ticket = JsonSerializer.Deserialize<Ticket>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(ticket);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize(Roles.Admin, Roles.Vendedor)]
        public async Task<IActionResult> ResponderTicket(TicketResponseRequest req)
        {
            // 🔐 NUEVO: asignar el admin desde la sesión
            req.IdUsuarioGestion = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            using var http = GetHttpClient();

            var json = JsonSerializer.Serialize(req);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await http.PutAsync("ResponderTicket", content);
            var message = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                TempData["mensaje"] = $"Éxito al responder: {message}";
            }
            else
            {
                TempData["mensaje"] = $"Error al responder el ticket ({resp.StatusCode}): {message}";
            }

            return RedirectToAction(nameof(IndexTicket));
        }


        // ==================================================
        // CERRAR TICKET (ADMIN)
        // ==================================================
        [RoleAuthorize(Roles.Admin, Roles.Vendedor)]
        public async Task<IActionResult> CerrarTicket(int id)
        {
            using var http = GetHttpClient();
            // Nota: PutAsync requiere un segundo parámetro Content, usar StringContent.Empty
            var resp = await http.PutAsync($"CerrarTicket/{id}", new StringContent("{}", Encoding.UTF8, "application/json"));

            var message = await resp.Content.ReadAsStringAsync();

            // 3. MEJORA: Manejo de errores más explícito
            if (resp.IsSuccessStatusCode)
            {
                TempData["mensaje"] = $"Éxito al cerrar: {message}";
            }
            else
            {
                TempData["mensaje"] = $"Error al cerrar el ticket ({resp.StatusCode}): {message}";
            }

            return RedirectToAction(nameof(IndexTicket));
        }
    }

    // Nota: Este DTO debería estar en un archivo Models/TicketResponseRequest.cs
    public class TicketResponseRequest
    {
        public int IdTicket { get; set; }
        public string Respuesta { get; set; } = string.Empty;
        public int IdUsuarioGestion { get; set; }
    }
}