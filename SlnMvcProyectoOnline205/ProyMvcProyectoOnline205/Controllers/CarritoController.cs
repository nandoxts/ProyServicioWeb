using Microsoft.AspNetCore.Mvc;
using ProyMvcProyectoOnline205.Models;
using System.Text.Json;

namespace ProyMvcProyectoOnline205.Controllers
{
    public class CarritoController : Controller
    {
        private const string CartSessionKey = "Carrito";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string apiProductoUrl = "http://localhost:5064/api/ProductoApi/";

        public CarritoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // ============================
        // HELPERS DE SESION
        // ============================
        private List<CarritoItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json)) return new List<CarritoItem>();
            return JsonSerializer.Deserialize<List<CarritoItem>>(json) ?? new List<CarritoItem>();
        }

        private void SaveCart(List<CarritoItem> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
            HttpContext.Session.SetInt32("CarritoCount", cart.Sum(x => x.Cantidad));
        }

        // ============================
        // AGREGAR PRODUCTO (anonimo o cliente)
        // ============================
        public async Task<IActionResult> Agregar(int id, int cantidad = 1)
        {
            using var client = _httpClientFactory.CreateClient();
            var resp = await client.GetAsync($"{apiProductoUrl}GetProducto/{id}");

            if (!resp.IsSuccessStatusCode)
            {
                TempData["mensaje"] = "El producto solicitado no esta disponible.";
                return RedirectToAction("Index", "Home");
            }

            var json = await resp.Content.ReadAsStringAsync();
            var producto = JsonSerializer.Deserialize<Producto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (producto == null || producto.Activo != true)
            {
                TempData["mensaje"] = "El producto no esta disponible.";
                return RedirectToAction("Index", "Home");
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.IdProducto == id);
            int nuevaCantidad = (item?.Cantidad ?? 0) + cantidad;

            // Validacion de stock
            if (producto.Stock.HasValue && nuevaCantidad > producto.Stock.Value)
            {
                TempData["mensaje"] = $"Solo quedan {producto.Stock.Value} unidades de '{producto.Nombre}'.";
                return RedirectToAction("Index");
            }

            if (item == null)
            {
                cart.Add(new CarritoItem
                {
                    IdProducto   = producto.IdProducto,
                    Nombre       = producto.Nombre ?? "",
                    Precio       = producto.Precio ?? 0,
                    Cantidad     = cantidad,
                    StockMaximo  = producto.Stock,
                    RutaImagen   = producto.RutaImagen,
                    NombreImagen = producto.NombreImagen
                });
            }
            else
            {
                item.Cantidad = nuevaCantidad;
            }

            SaveCart(cart);

            // Si vino de catalogo, volver al catalogo
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && referer.Contains("/Home", StringComparison.OrdinalIgnoreCase))
                return Redirect(referer);

            return RedirectToAction("Index");
        }

        // ============================
        // VER CARRITO
        // ============================
        public IActionResult Index()
        {
            return View(GetCart());
        }

        // ============================
        // ACTUALIZAR CANTIDAD (+/-)
        // ============================
        public async Task<IActionResult> Actualizar(int id, int cantidad)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.IdProducto == id);
            if (item == null) return RedirectToAction("Index");

            if (cantidad <= 0)
            {
                cart.Remove(item);
                SaveCart(cart);
                return RedirectToAction("Index");
            }

            // Re-validar stock contra la API por si cambio
            using var client = _httpClientFactory.CreateClient();
            var resp = await client.GetAsync($"{apiProductoUrl}GetProducto/{id}");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var producto = JsonSerializer.Deserialize<Producto>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (producto?.Stock.HasValue == true && cantidad > producto.Stock.Value)
                {
                    TempData["mensaje"] = $"Solo quedan {producto.Stock.Value} unidades de '{item.Nombre}'.";
                    cantidad = producto.Stock.Value;
                }
            }

            item.Cantidad = cantidad;
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // ============================
        // QUITAR PRODUCTO
        // ============================
        public IActionResult Quitar(int id)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.IdProducto == id);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // ============================
        // LIMPIAR CARRITO
        // ============================
        public IActionResult Limpiar()
        {
            HttpContext.Session.Remove(CartSessionKey);
            HttpContext.Session.SetInt32("CarritoCount", 0);
            return RedirectToAction("Index");
        }

        // ============================
        // CHECKOUT (requiere login de cliente)
        // ============================
        public IActionResult Checkout()
        {
            var idCliente = HttpContext.Session.GetInt32("IdCliente");
            var rol       = HttpContext.Session.GetInt32("IdRol");

            if (idCliente == null || rol != 3)
            {
                TempData["mensaje"] = "Inicia sesion como cliente para finalizar tu compra.";
                return RedirectToAction("Login", "Autenticacion");
            }

            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["mensaje"] = "Tu carrito esta vacio.";
                return RedirectToAction("Index");
            }

            // TODO: integracion con PedidoApi para registrar la venta
            TempData["mensaje"] = "Pago en construccion. (PayPal sandbox)";
            return RedirectToAction("Index");
        }
    }

    // ============================
    // MODELO DE CARRITO
    // ============================
    public class CarritoItem
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public int? StockMaximo { get; set; }
        public string? RutaImagen { get; set; }
        public string? NombreImagen { get; set; }

        public decimal Subtotal => Precio * Cantidad;
    }
}
