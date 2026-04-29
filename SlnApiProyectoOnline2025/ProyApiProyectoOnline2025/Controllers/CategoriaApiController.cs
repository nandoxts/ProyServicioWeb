using Microsoft.AspNetCore.Mvc;
using ProyApiProyectoOnline2025.Models;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProyApiProyectoOnline2025.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaApiController : ControllerBase
    {
        private readonly Proyectodiciembre2025Context db;

        public CategoriaApiController(Proyectodiciembre2025Context context)
        {
            db = context;
        }

        [HttpGet("GetCategorias")]
        public async Task<ActionResult<List<Categorium>>> GetCategorias()
        {
            return await db.Categoria
                           .Where(c => c.Activo == true)
                           .ToListAsync();
        }
    }
}
