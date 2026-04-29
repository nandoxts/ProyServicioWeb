using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore; // 👈 ESTA ES LA CLAVE
using ProyApiProyectoOnline2025.Models;
using Microsoft.AspNetCore.SignalR;


[Route("api/[controller]")]
[ApiController]
public class PedidoApiController : ControllerBase
{

    private readonly Proyectodiciembre2025Context db;
    private readonly IHubContext<NotificacionHub> _hub;

    public PedidoApiController(Proyectodiciembre2025Context _db, IHubContext<NotificacionHub> hub)
    {
        db = _db;
        _hub = hub;
    }

    [HttpGet("GetPedidosPorCliente/{idCliente}")]
    public async Task<ActionResult<IEnumerable<Ventum>>> GetPedidosPorCliente(int idCliente)
    {
        var pedidos = await db.Venta
            .Where(v => v.IdCliente == idCliente)
            .OrderByDescending(v => v.FechaVenta)
            .ToListAsync();

        // 👇 SI NO HAY PEDIDOS → LISTA VACÍA, NO ERROR
        return Ok(pedidos);
    }


    [HttpGet("GetPedido/{idVenta}")]
    public async Task<ActionResult<Ventum>> GetPedido(int idVenta)
    {
        var pedido = await db.Venta
            .Include(v => v.DetalleVenta) // 👈 colección
                .ThenInclude(d => d.IdProductoNavigation) // 👈 producto
            .Include(v => v.IdEstadoPedidoNavigation)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);

        if (pedido == null)
            return NotFound();

        return Ok(pedido);
    }

    [HttpGet("GetPedidos")]
    public async Task<ActionResult<IEnumerable<Ventum>>> GetPedidos()
    {
        var pedidos = await db.Venta
            .Include(v => v.IdClienteNavigation)
            .Include(v => v.IdEstadoPedidoNavigation)
            .OrderByDescending(v => v.FechaVenta)
            .ToListAsync();

        return Ok(pedidos);
    }

    [HttpPut("CambiarEstado")]
    public async Task<IActionResult> CambiarEstado(int idVenta, int idEstado)
    {
        var venta = await db.Venta
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);

        if (venta == null)
            return NotFound("Pedido no encontrado");

        // 1️⃣ Cambiar estado
        venta.IdEstadoPedido = idEstado;

        // 2️⃣ Mensaje automático
        string mensaje = idEstado switch
        {
            2 => "💳 Tu pedido ha sido pagado",
            3 => "🧑‍🍳 Tu pedido está en preparación",
            4 => "🚚 Tu pedido fue enviado",
            5 => "✅ Tu pedido fue entregado",
            6 => "❌ Tu pedido fue cancelado",
            _ => "📦 Estado del pedido actualizado"
        };

        // 3️⃣ Guardar notificación en BD
        var noti = new NotificacionCliente
        {
            IdCliente = venta.IdCliente,
            Mensaje = mensaje,
            EsLeido = false,
            FechaCreacion = DateTime.Now
        };

        db.NotificacionClientes.Add(noti);
        await db.SaveChangesAsync();

        // 4️⃣ Enviar notificación en tiempo real
        await _hub.Clients
            .Group($"cliente_{venta.IdCliente}")
            .SendAsync("RecibirNotificacion", mensaje);

        return Ok("Estado actualizado y notificación enviada");
    }


}
