using Microsoft.AspNetCore.Mvc;
using ProyApiProyectoOnline2025.Models;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProyApiProyectoOnline2025.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarcaApiController : ControllerBase
    {
        private readonly Proyectodiciembre2025Context db;

        public MarcaApiController(Proyectodiciembre2025Context context)
        {
            db = context;
        }

        [HttpGet("GetMarcas")]
        public async Task<ActionResult<List<Marca>>> GetMarcas()
        {
            return await db.Marcas
                           .Where(m => m.Activo == true)
                           .ToListAsync();
        }
    }

}

