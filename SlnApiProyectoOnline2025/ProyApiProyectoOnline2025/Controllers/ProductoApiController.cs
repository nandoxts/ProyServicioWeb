using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyApiProyectoOnline2025.Models;

namespace ProyApiProyectoOnline2025.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoApiController : ControllerBase
    {
        // Contexto privado y readonly
        private readonly Proyectodiciembre2025Context db;

        public ProductoApiController(Proyectodiciembre2025Context _db)
        {
            db = _db;
        }

        // ===============================
        // GET: api/ProductoApi/GetProductos
        // ===============================
        [HttpGet("GetProductos")]
        public async Task<ActionResult<List<Producto>>> GetProductos()
        {
            if (db.Productos == null)
                return NotFound();

            var listado = await db.Productos
                                  .Where(p => p.Activo == true)
                                  .ToListAsync();

            return listado;
        }

        // ===============================
        // GET: api/ProductoApi/GetProducto/5
        // ===============================
        [HttpGet("GetProducto/{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            var buscado = await db.Productos.FindAsync(id);

            if (buscado == null)
                return BadRequest("No se encontró el producto con ID: " + id);

            return buscado;
        }

        // ===============================
        // POST: api/ProductoApi/PostProducto
        // ===============================
        [HttpPost("PostProducto")]
        public async Task<ActionResult<string>> PostProducto([FromBody] Producto value)
        {
            try
            {
                // 1. Buscar producto por nombre
                var existente = await db.Productos
                                        .FirstOrDefaultAsync(p => p.Nombre == value.Nombre);

                // 2. Si ya existe activo → no permitir duplicados
                if (existente != null && existente.Activo == true)
                    return BadRequest("Ya existe un producto con ese nombre.");

                // 3. Si existe pero está inactivo → reactivar
                if (existente != null && existente.Activo == false)
                {
                    existente.Activo = true;
                    existente.Precio = value.Precio;
                    existente.Stock = value.Stock;
                    existente.Descripcion = value.Descripcion;
                    existente.RutaImagen = value.RutaImagen;
                    existente.NombreImagen = value.NombreImagen;

                    await db.SaveChangesAsync();
                    return "Producto reactivado exitosamente.";
                }

                // 4. Si no existe, insertar nuevo
                await db.Productos.AddAsync(value);
                await db.SaveChangesAsync();

                return $"Producto '{value.Nombre}' registrado correctamente";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.InnerException?.Message;
            }
        }

        // ===============================
        // PUT: api/ProductoApi/PutProducto
        // ===============================
        [HttpPut("PutProducto")]
        public async Task<ActionResult<string>> PutProducto([FromBody] Producto value)
        {
            try
            {
                var buscado = await db.Productos.FindAsync(value.IdProducto);

                if (buscado == null)
                    return NotFound("Producto no encontrado");

                // Desacoplar para permitir modificar
                db.Entry(buscado).State = EntityState.Detached;
                db.Entry(value).State = EntityState.Modified;

                await db.SaveChangesAsync();

                return "Se actualizaron los datos del producto: " + value.Nombre;
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.InnerException?.Message;
            }
        }

        // ===============================
        // DELETE: api/ProductoApi/DeleteProducto/5
        // ===============================
        [HttpDelete("DeleteProducto/{id}")]
        public async Task<ActionResult<string>> DeleteProducto(int id)
        {
            var buscado = await db.Productos.FindAsync(id);

            if (buscado == null)
                return NotFound("ERROR: No se encontró el producto: " + id);

            // Eliminación lógica
            buscado.Activo = false;
            await db.SaveChangesAsync();

            return "Se eliminó el producto: " + id;
        }
        [HttpGet("Filtrar")]
        //producto filtro 
        public async Task<ActionResult<List<Producto>>> Filtrar(
    int? idMarca,
    int? idCategoria)
        {
            var query = db.Productos
    .Include(p => p.IdMarcaNavigation)
    .Include(p => p.IdCategoriaNavigation)
    .Where(p => p.Activo == true)
    .AsQueryable();

            if (idMarca != null)
                query = query.Where(p => p.IdMarca == idMarca);

            if (idCategoria != null)
                query = query.Where(p => p.IdCategoria == idCategoria);

            return await query.ToListAsync();
        }
        // ===================================================
        // BUSCAR PRODUCTOS (CATÁLOGO PÚBLICO)
        // ===================================================
[HttpGet("Buscar")]
public async Task<ActionResult<List<Producto>>> Buscar(string texto)
{
    var query = db.Productos
        .Include(p => p.IdCategoriaNavigation)
        .Include(p => p.IdMarcaNavigation)
        .Where(p => p.Activo == true)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(texto))
    {
        texto = texto.ToLower();
        query = query.Where(p =>
            p.Nombre.ToLower().Contains(texto) ||
            p.Descripcion.ToLower().Contains(texto));
    }

    return await query.ToListAsync();
}


    }
}
