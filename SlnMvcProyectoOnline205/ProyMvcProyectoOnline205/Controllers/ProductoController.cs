using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyMvcProyectoOnline205.Models;
using System.Text;

namespace ProyMvcProyectoOnline205.Controllers
{
    public class ProductoController : Controller
    {
        // Se recomienda inyectar HttpClientFactory en lugar de crear HttpClient en cada método
        private readonly string apiUrl = "http://localhost:5064/api/ProductoApi/";

        // ============================
        // TRAER PRODUCTOS
        // ============================
        private async Task<List<Producto>> TraerProductos()
        {
            using var http = new HttpClient();
            var resp = await http.GetAsync(apiUrl + "GetProductos");
            var data = await resp.Content.ReadAsStringAsync();

            // Usar '?' o manejar el caso de error de deserialización
            return JsonConvert.DeserializeObject<List<Producto>>(data) ?? new List<Producto>();
        }

        // ============================
        // INDEX
        // ============================
        public async Task<IActionResult> IndexProducto()
        {
            return View(await TraerProductos());
        }

        // ============================
        // DETAILS
        // ============================
        public async Task<IActionResult> DetailsProducto(int id)
        {
            using var http = new HttpClient();
            var resp = await http.GetAsync(apiUrl + $"GetProducto/{id}");
            var data = await resp.Content.ReadAsStringAsync();
            var producto = JsonConvert.DeserializeObject<Producto>(data);
            return View(producto);
        }

        // ============================
        // CREATE
        // ============================
        public IActionResult CreateProducto()
        {
            return View(new Producto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateProducto(Producto obj)
        {
            using var http = new HttpClient();
            // Lógica de negocio: todo producto nuevo está activo
            obj.Activo = true;

            var json = JsonConvert.SerializeObject(obj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await http.PostAsync(apiUrl + "PostProducto", content);
            TempData["mensaje"] = await resp.Content.ReadAsStringAsync();

            return RedirectToAction(nameof(IndexProducto));
        }

        // ============================
        // EDIT - GET
        // ============================
        public async Task<IActionResult> EditProducto(int id)
        {
            using var http = new HttpClient();
            var resp = await http.GetAsync(apiUrl + $"GetProducto/{id}");
            var data = await resp.Content.ReadAsStringAsync();
            return View(JsonConvert.DeserializeObject<Producto>(data));
        }

        // ============================
        // EDIT - POST (Corregido para evitar la desactivación)
        // ============================
        [HttpPost]
        public async Task<IActionResult> EditProducto(Producto obj)
        {
            // 1. OBTENER EL PRODUCTO ORIGINAL DESDE EL API (Para mantener Activo, Rutas, etc.)
            Producto? original;
            using (var http = new HttpClient())
            {
                var response = await http.GetAsync(apiUrl + $"GetProducto/{obj.IdProducto}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["mensaje"] = "Error al obtener el producto original para edición.";
                    return RedirectToAction(nameof(IndexProducto));
                }

                string json = await response.Content.ReadAsStringAsync();
                original = JsonConvert.DeserializeObject<Producto>(json);
            }

            if (original == null)
            {
                TempData["mensaje"] = "Producto no encontrado.";
                return RedirectToAction(nameof(IndexProducto));
            }

            // 2. PRESERVAR CAMPOS: Transferimos los valores de gestión (Activo, imágenes) 
            //    del objeto original al objeto que vamos a enviar (obj).
            obj.Activo = original.Activo;
            obj.RutaImagen = original.RutaImagen;
            obj.NombreImagen = original.NombreImagen;

            // 3. ENVIAR EL OBJETO COMPLETO Y CORREGIDO AL PUT
            using (var http = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(obj);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await http.PutAsync(apiUrl + "PutProducto", content);
                TempData["mensaje"] = await resp.Content.ReadAsStringAsync();
            }

            return RedirectToAction(nameof(IndexProducto));
        }

        // ============================
        // DELETE (lógico)
        // ============================
        public async Task<IActionResult> DeleteProducto(int id)
        {
            using var http = new HttpClient();
            var resp = await http.DeleteAsync(apiUrl + $"DeleteProducto/{id}");

            TempData["mensaje"] = await resp.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(IndexProducto));
        }
    }
}