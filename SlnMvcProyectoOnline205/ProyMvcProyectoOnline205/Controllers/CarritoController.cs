using Microsoft.AspNetCore.Mvc;
using ProyMvcProyectoOnline205.Models;
using System.Text.Json;

namespace ProyMvcProyectoOnline205.Controllers
{
    public class CarritoController : Controller
    {
        private static List<CarritoItem> carrito = new();

        // URL de tu API de productos
        private readonly string apiProductoUrl = "http://localhost:5064/api/ProductoApi/";

        // ============================
        // AGREGAR PRODUCTO
        // ============================
        public async Task<IActionResult> Agregar(int id)
        {
            if (HttpContext.Session.GetInt32("IdRol") != 3)
                return RedirectToAction("Login", "Autenticacion");

            var item = carrito.FirstOrDefault(x => x.IdProducto == id);

            if (item == null)
            {
                // 🔥 Traer producto desde API
                using var client = new HttpClient();
                var resp = await client.GetAsync($"{apiProductoUrl}GetProducto/{id}");

                if (!resp.IsSuccessStatusCode)
                    return RedirectToAction("Index", "Home");

                var json = await resp.Content.ReadAsStringAsync();
                var producto = JsonSerializer.Deserialize<Producto>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                carrito.Add(new CarritoItem
                {
                    IdProducto = producto!.IdProducto,
                    Nombre = producto.Nombre!,
                    Precio = producto.Precio!.Value,
                    Cantidad = 1
                });
            }
            else
            {
                item.Cantidad++;
            }

            return RedirectToAction("Index");
        }

        // ============================
        // VER CARRITO
        // ============================
        public IActionResult Index()
        {
            return View(carrito);
        }

        // ============================
        // QUITAR PRODUCTO
        // ============================
        public IActionResult Quitar(int id)
        {
            carrito.RemoveAll(x => x.IdProducto == id);
            return RedirectToAction("Index");
        }

        // ============================
        // LIMPIAR CARRITO
        // ============================
        public IActionResult Limpiar()
        {
            carrito.Clear();
            return RedirectToAction("Index");
        }
    }

    // ============================
    // MODELO DE CARRITO (MEJORADO)
    // ============================
    public class CarritoItem
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }

        public decimal Subtotal => Precio * Cantidad;
    }
}
