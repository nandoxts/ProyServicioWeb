using Microsoft.AspNetCore.Mvc;
using ProyMvcProyectoOnline205.Filters;
using ProyMvcProyectoOnline205.Models;
using System.Text.Json;
using Rotativa.AspNetCore;
using System.Net.Http;


namespace ProyMvcProyectoOnline205.Controllers
{
    [RoleAuthorize(Roles.Admin, Roles.Vendedor, Roles.Cliente)] // Cualquier usuario logueado
    public class PedidoController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly string apiUrl = "http://localhost:5064/api/PedidoApi/";

        public PedidoController(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory;
        }

        // ==============================
        // LISTAR PEDIDOS (CLIENTE / ADMIN)
        // ==============================
        public async Task<IActionResult> IndexPedido()
        {
            var rol = HttpContext.Session.GetInt32("IdRol");

            if (rol == null)
                return RedirectToAction("Login", "Autenticacion");

            using var http = _http.CreateClient();

            // ------------------------------
            // CLIENTE → SUS PEDIDOS
            // ------------------------------
            if (rol == 3)
            {
                var idCliente = HttpContext.Session.GetInt32("IdCliente");
                if (idCliente == null)
                    return RedirectToAction("Login", "Autenticacion");

                var resp = await http.GetAsync(
                    $"{apiUrl}GetPedidosPorCliente/{idCliente}"
                );

                if (!resp.IsSuccessStatusCode)
                {
                    return View("IndexCliente", new List<Ventum>());
                }

                var pedidos = await resp.Content
                    .ReadFromJsonAsync<List<Ventum>>()
                    ?? new List<Ventum>();

                return View("IndexCliente", pedidos);
            }

            // ------------------------------
            // ADMIN / EMPLEADO → TODOS
            // ------------------------------
            if (rol == 1 || rol == 2)
            {
                var resp = await http.GetAsync($"{apiUrl}GetPedidos");
                if (!resp.IsSuccessStatusCode)
                {
                    return View("IndexAdmin", new List<Ventum>());
                }

                var pedidos = await resp.Content
                    .ReadFromJsonAsync<List<Ventum>>()
                    ?? new List<Ventum>();

                return View("IndexAdmin", pedidos);

            }

            return RedirectToAction("Login", "Autenticacion");
        }

        // ==============================
        // ALIAS: /Pedido/IndexAdmin → IndexPedido
        // (referenciado desde el sidebar y URL directa)
        // ==============================
        public Task<IActionResult> IndexAdmin() => IndexPedido();

        // ==============================
        // DETALLE DEL PEDIDO
        // ==============================
        public async Task<IActionResult> DetailsPedido(int id)
        {
            var rol = HttpContext.Session.GetInt32("IdRol");
            if (rol == null)
                return RedirectToAction("Login", "Autenticacion");

            using var client = _http.CreateClient();
            var resp = await client.GetAsync($"{apiUrl}GetPedido/{id}");

            if (!resp.IsSuccessStatusCode)
                return RedirectToAction(nameof(IndexPedido));

            if (!resp.IsSuccessStatusCode)
                return RedirectToAction(nameof(IndexPedido));

            var pedido = await resp.Content
                .ReadFromJsonAsync<Ventum>();

            if (pedido == null)
                return RedirectToAction(nameof(IndexPedido));

            return View(pedido);

        }
        // ==============================
        // DESCARGAR PEDIDO EN PDF
        // ==============================
        public async Task<IActionResult> DescargarPedido(int id)
        {
            var rol = HttpContext.Session.GetInt32("IdRol");
            if (rol == null)
                return RedirectToAction("Login", "Autenticacion");

            using var client = _http.CreateClient();
            var resp = await client.GetAsync($"{apiUrl}GetPedido/{id}");

            if (!resp.IsSuccessStatusCode)
                return RedirectToAction(nameof(IndexPedido));

            var pedido = await resp.Content.ReadFromJsonAsync<Ventum>();

            if (pedido == null)
                return RedirectToAction(nameof(IndexPedido));

            // 🔐 Seguridad cliente
            if (rol == 3)
            {
                var idCliente = HttpContext.Session.GetInt32("IdCliente");
                if (idCliente == null || pedido.IdCliente != idCliente)
                    return Unauthorized();
            }

            return new ViewAsPdf("PdfPedido", pedido)
            {
                FileName = $"Pedido_{pedido.IdVenta}.pdf"
            };
        }

        public async Task<IActionResult> DescargarProductosPedido(int id)
        {
            using var client = _http.CreateClient();
            var resp = await client.GetAsync($"{apiUrl}GetPedido/{id}");

            if (!resp.IsSuccessStatusCode)
                return RedirectToAction(nameof(IndexPedido));

            var pedido = await resp.Content.ReadFromJsonAsync<Ventum>();

            if (pedido == null)
                return RedirectToAction(nameof(IndexPedido));

            return new ViewAsPdf("PdfProductosPedido", pedido)
            {
                FileName = $"Productos_Pedido_{id}.pdf"
            };
        }

        // ==============================
        // DESCARGAR TODOS LOS PEDIDOS (PDF)
        // ==============================
        public async Task<IActionResult> DescargarTodosPedidos()
        {
            var rol = HttpContext.Session.GetInt32("IdRol");

            if (rol == null)
                return RedirectToAction("Login", "Autenticacion");

            using var client = _http.CreateClient();

            // -----------------------------
            // CLIENTE → solo sus pedidos
            // -----------------------------
            if (rol == 3)
            {
                var idCliente = HttpContext.Session.GetInt32("IdCliente");

                var resp = await client.GetAsync($"{apiUrl}GetPedidosPorCliente/{idCliente}");

                if (!resp.IsSuccessStatusCode)
                    return RedirectToAction(nameof(IndexPedido));

                var pedidos = await resp.Content
                    .ReadFromJsonAsync<List<Ventum>>()
                    ?? new List<Ventum>();

                return new ViewAsPdf("PdfPedidos", pedidos)
                {
                    FileName = "Mis_Pedidos.pdf"
                };

            }

            // -----------------------------
            // ADMIN / EMPLEADO → todos
            // -----------------------------
            var respAdmin = await client.GetAsync($"{apiUrl}GetPedidos");

            if (!respAdmin.IsSuccessStatusCode)
                return RedirectToAction(nameof(IndexPedido));

            var pedidosAdmin = await respAdmin.Content
                .ReadFromJsonAsync<List<Ventum>>()
                ?? new List<Ventum>();

            return new ViewAsPdf("PdfPedidos", pedidosAdmin)
            {
                FileName = "Todos_los_Pedidos.pdf"
            };

        }
        [HttpPost]
        [RoleAuthorize(Roles.Admin, Roles.Vendedor)] // Solo staff cambia estados
        public async Task<IActionResult> CambiarEstadoPedido(int idVenta, int idEstado)
        {
            var client = _http.CreateClient();

            // URL del API
            var apiUrl = $"http://localhost:5064/api/PedidoApi/CambiarEstado?idVenta={idVenta}&idEstado={idEstado}";

            var response = await client.PutAsync(apiUrl, null);

            if (!response.IsSuccessStatusCode)
            {
                TempData["mensaje"] = "Error al cambiar el estado del pedido";
            }

            return RedirectToAction("DetailsPedido", new { id = idVenta });
        }



    }
}
