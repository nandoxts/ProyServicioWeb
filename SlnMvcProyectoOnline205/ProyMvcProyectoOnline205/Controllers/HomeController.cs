using Microsoft.AspNetCore.Mvc;
using ProyMvcProyectoOnline205.Filters;
using ProyMvcProyectoOnline205.Models;
using System.Net.Http;
using System.Text.Json;

namespace ProyMvcProyectoOnline205.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _config;
        // URLs del API
        private readonly string apiCategoria = "http://localhost:5064/api/CategoriaApi/GetCategorias";
        private readonly string apiMarca = "http://localhost:5064/api/MarcaApi/GetMarcas";
        private readonly string apiProductos = "http://localhost:5064/api/ProductoApi/GetProductos";
        private readonly string apiFiltrar = "http://localhost:5064/api/ProductoApi/Filtrar";


        // ==============================
        // HOME / CATÁLOGO
        // ==============================
        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _http = httpClientFactory;
            _config = config;
        }
        public async Task<IActionResult> Index(int? idCategoria, int? idMarca, string? q)
        {
            // Staff (admin/vendedor) NO tiene nada que hacer en el catálogo público.
            var rol = HttpContext.Session.GetInt32("IdRol");
            if (rol == Roles.Admin || rol == Roles.Vendedor)
                return RedirectToAction("Index", "Dashboard");

            // ==============================
            // 🔔 CARGAR NOTIFICACIONES (CLIENTE)
            // ==============================
            int? idCliente = HttpContext.Session.GetInt32("IdCliente");
/*
            if (idCliente != null)
            {
                // 👉 para el avance puedes dejar algo simple
            }
            else
            {
            }*/

            using var client = _http.CreateClient();

            // ===============================
            // CARGAR CATEGORÍAS Y MARCAS
            // ===============================
            ViewBag.Categorias = await client
                .GetFromJsonAsync<List<Categorium>>("http://localhost:5064/api/CategoriaApi/GetCategorias");

            ViewBag.Marcas = await client
                .GetFromJsonAsync<List<Marca>>("http://localhost:5064/api/MarcaApi/GetMarcas");

            List<Producto> productos;

            // ===============================
            // 🔍 BÚSQUEDA POR TEXTO
            // ===============================
            if (!string.IsNullOrWhiteSpace(q))
            {
                productos = await client.GetFromJsonAsync<List<Producto>>(
                    $"http://localhost:5064/api/ProductoApi/Buscar?texto={q}"
                ) ?? new();
            }
            else
            {
                // ===============================
                // 📂 FILTRO POR CATEGORÍA / MARCA
                // ===============================
                string url = "http://localhost:5064/api/ProductoApi/Filtrar?";

                if (idCategoria != null)
                    url += $"idCategoria={idCategoria}&";

                if (idMarca != null)
                    url += $"idMarca={idMarca}";

                productos = await client.GetFromJsonAsync<List<Producto>>(url) ?? new();
            }

            return View(productos);
        }

    }
}
