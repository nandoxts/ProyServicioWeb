using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyMvcProyectoOnline205.Filters;
using ProyMvcProyectoOnline205.Models;
using System.Text;

namespace ProyMvcProyectoOnline205.Controllers
{
    [RoleAuthorize(Roles.Admin, Roles.Vendedor)] // CRUD de productos: solo staff
    public class ProductoController : Controller
    {
        // Se recomienda inyectar HttpClientFactory en lugar de crear HttpClient en cada método
        private readonly string apiUrl          = "http://localhost:5064/api/ProductoApi/";
        private readonly string apiCategoriaUrl = "http://localhost:5064/api/CategoriaApi/";
        private readonly string apiMarcaUrl     = "http://localhost:5064/api/MarcaApi/";

        private readonly IWebHostEnvironment _env;
        public ProductoController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ============================
        // GUARDAR IMAGEN SUBIDA EN wwwroot/uploads/productos
        // Devuelve (rutaWeb, nombreArchivo) o (null, null) si no hay archivo o hay error.
        // Lanza excepcion con mensaje amigable si el archivo es invalido.
        // ============================
        private async Task<(string? ruta, string? nombre)> GuardarImagenAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return (null, null);

            // Validacion: solo imagenes
            var permitidos = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!permitidos.Contains(ext))
                throw new InvalidOperationException("Formato no permitido. Usa JPG, PNG, GIF o WEBP.");

            // Validacion: maximo 5 MB
            const long max = 5 * 1024 * 1024;
            if (file.Length > max)
                throw new InvalidOperationException("La imagen supera los 5 MB.");

            // Carpeta destino: wwwroot/uploads/productos
            var carpeta = Path.Combine(_env.WebRootPath, "uploads", "productos");
            Directory.CreateDirectory(carpeta);

            // Nombre unico: guid + extension (evita colisiones y caracteres raros)
            var nombreArchivo = $"{Guid.NewGuid():N}{ext}";
            var rutaFisica = Path.Combine(carpeta, nombreArchivo);

            using (var fs = new FileStream(rutaFisica, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            // Ruta web (servida por StaticFiles)
            var rutaWeb = $"/uploads/productos/{nombreArchivo}";
            return (rutaWeb, nombreArchivo);
        }

        // Borra del disco una imagen previamente subida (solo si esta en /uploads/productos)
        private void BorrarImagenSiAplica(string? rutaWeb)
        {
            if (string.IsNullOrWhiteSpace(rutaWeb)) return;
            if (!rutaWeb.StartsWith("/uploads/productos/", StringComparison.OrdinalIgnoreCase)) return;
            try
            {
                var rel = rutaWeb.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var ruta = Path.Combine(_env.WebRootPath, rel);
                if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
            }
            catch { /* silencioso: no bloquea el guardado del producto */ }
        }

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
        // CARGAR DROPDOWNS (Marca / Categoria)
        // ============================
        private async Task CargarDropdownsAsync()
        {
            using var http = new HttpClient();

            try
            {
                var respCat = await http.GetAsync(apiCategoriaUrl + "GetCategorias");
                if (respCat.IsSuccessStatusCode)
                {
                    var cats = JsonConvert.DeserializeObject<List<Categorium>>(
                        await respCat.Content.ReadAsStringAsync()) ?? new List<Categorium>();
                    ViewBag.Categorias = cats;
                }
            }
            catch { ViewBag.Categorias = new List<Categorium>(); }

            try
            {
                var respMar = await http.GetAsync(apiMarcaUrl + "GetMarcas");
                if (respMar.IsSuccessStatusCode)
                {
                    var mars = JsonConvert.DeserializeObject<List<Marca>>(
                        await respMar.Content.ReadAsStringAsync()) ?? new List<Marca>();
                    ViewBag.Marcas = mars;
                }
            }
            catch { ViewBag.Marcas = new List<Marca>(); }
        }

        // ============================
        // INDEX (paginado)
        // ============================
        public async Task<IActionResult> IndexProducto(int page = 1, int pageSize = 10)
        {
            var todos = await TraerProductos();
            var paged = PagedList<Producto>.Create(todos, page, pageSize);
            return View(paged);
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
        public async Task<IActionResult> CreateProducto()
        {
            await CargarDropdownsAsync();
            return View(new Producto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateProducto(Producto obj, IFormFile? imagenArchivo)
        {
            // Subida de imagen (opcional)
            try
            {
                var (ruta, nombre) = await GuardarImagenAsync(imagenArchivo);
                if (ruta != null)
                {
                    obj.RutaImagen   = ruta;
                    obj.NombreImagen = nombre;
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["mensaje"] = ex.Message;
                await CargarDropdownsAsync();
                return View(obj);
            }

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
            await CargarDropdownsAsync();
            using var http = new HttpClient();
            var resp = await http.GetAsync(apiUrl + $"GetProducto/{id}");
            var data = await resp.Content.ReadAsStringAsync();
            return View(JsonConvert.DeserializeObject<Producto>(data));
        }

        // ============================
        // EDIT - POST (Corregido para evitar la desactivación)
        // ============================
        [HttpPost]
        public async Task<IActionResult> EditProducto(Producto obj, IFormFile? imagenArchivo)
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

            // 2. PRESERVAR CAMPOS: Activo siempre del original.
            //    Imagen: si subieron una nueva, reemplaza; si no, mantiene la actual.
            obj.Activo = original.Activo;

            try
            {
                var (rutaNueva, nombreNuevo) = await GuardarImagenAsync(imagenArchivo);
                if (rutaNueva != null)
                {
                    // Reemplaza la imagen anterior y borra la antigua si era subida local
                    BorrarImagenSiAplica(original.RutaImagen);
                    obj.RutaImagen   = rutaNueva;
                    obj.NombreImagen = nombreNuevo;
                }
                else
                {
                    obj.RutaImagen   = original.RutaImagen;
                    obj.NombreImagen = original.NombreImagen;
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["mensaje"] = ex.Message;
                await CargarDropdownsAsync();
                return View(obj);
            }

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
        // DELETE (lógico) - GET (vista confirmación)
        // ============================
        public async Task<IActionResult> DeleteProducto(int id)
        {
            using var http = new HttpClient();
            var resp = await http.GetAsync(apiUrl + $"GetProducto/{id}");

            if (!resp.IsSuccessStatusCode)
            {
                TempData["mensaje"] = "No se pudo cargar el producto.";
                return RedirectToAction(nameof(IndexProducto));
            }

            var data = await resp.Content.ReadAsStringAsync();
            return View(JsonConvert.DeserializeObject<Producto>(data));
        }

        // ============================
        // DELETE (lógico) - POST (ejecución)
        // ============================
        [HttpPost, ActionName("DeleteProducto")]
        public async Task<IActionResult> DeleteProductoConfirmed(int id)
        {
            using var http = new HttpClient();
            var resp = await http.DeleteAsync(apiUrl + $"DeleteProducto/{id}");

            TempData["mensaje"] = resp.IsSuccessStatusCode
                ? "Producto desactivado correctamente."
                : await resp.Content.ReadAsStringAsync();

            return RedirectToAction(nameof(IndexProducto));
        }
    }
}