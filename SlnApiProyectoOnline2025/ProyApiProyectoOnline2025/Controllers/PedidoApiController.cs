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

    // ============================================================
    // CREAR PEDIDO (CHECKOUT)
    // ============================================================
    public class CrearPedidoItemDto
    {
        public int IdProducto { get; set; }
        public int Cantidad   { get; set; }
    }

    public class CrearPedidoRequest
    {
        public int IdCliente               { get; set; }
        public string? MetodoPago          { get; set; } = "PayPal";
        public string? ReferenciaPago      { get; set; }
        public string? Contacto            { get; set; }
        public string? Telefono            { get; set; }
        public string? Direccion           { get; set; }
        public string? IdDistrito          { get; set; }
        public List<CrearPedidoItemDto> Items { get; set; } = new();
    }

    public class CrearPedidoResponse
    {
        public int IdVenta        { get; set; }
        public decimal MontoTotal { get; set; }
        public int TotalProducto  { get; set; }
        public string Mensaje     { get; set; } = "";
    }

    [HttpPost("CrearPedido")]
    public async Task<ActionResult<CrearPedidoResponse>> CrearPedido([FromBody] CrearPedidoRequest req)
    {
        if (req == null || req.Items == null || !req.Items.Any())
            return BadRequest("El pedido no tiene productos.");

        // Validar cliente
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.IdCliente == req.IdCliente);
        if (cliente == null) return BadRequest("Cliente no encontrado.");

        // Cargar productos del pedido en una sola consulta
        var idsProductos = req.Items.Select(i => i.IdProducto).Distinct().ToList();
        var productos = await db.Productos
            .Where(p => idsProductos.Contains(p.IdProducto) && p.Activo == true)
            .ToListAsync();

        if (productos.Count != idsProductos.Count)
            return BadRequest("Alguno de los productos ya no esta disponible.");

        // Validar stock antes de tocar nada
        foreach (var item in req.Items)
        {
            var p = productos.First(x => x.IdProducto == item.IdProducto);
            if ((p.Stock ?? 0) < item.Cantidad)
                return BadRequest($"Stock insuficiente para '{p.Nombre}'. Disponible: {p.Stock ?? 0}, solicitado: {item.Cantidad}.");
        }

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            decimal monto = 0;
            int totalUnidades = 0;
            var detalles = new List<DetalleVentum>();

            foreach (var item in req.Items)
            {
                var p = productos.First(x => x.IdProducto == item.IdProducto);
                var subtotal = (p.Precio ?? 0) * item.Cantidad;

                detalles.Add(new DetalleVentum
                {
                    IdProducto = p.IdProducto,
                    Cantidad   = item.Cantidad,
                    Total      = subtotal
                });

                p.Stock = (p.Stock ?? 0) - item.Cantidad; // descuento de stock
                monto         += subtotal;
                totalUnidades += item.Cantidad;
            }

            var venta = new Ventum
            {
                IdCliente        = req.IdCliente,
                MontoTotal       = monto,
                TotalProducto    = totalUnidades,
                FechaVenta       = DateTime.Now,
                IdEstadoPedido   = 1, // Registrado
                Contacto         = req.Contacto,
                Telefono         = req.Telefono,
                Direccion        = req.Direccion,
                IdDistrito       = req.IdDistrito,
                IdTransaccion    = req.ReferenciaPago,
                DetalleVenta     = detalles
            };

            db.Venta.Add(venta);
            await db.SaveChangesAsync();

            // Registrar el pago. Para simulacion PayPal sandbox lo damos por completado.
            bool pagoCompletado = !string.IsNullOrEmpty(req.ReferenciaPago);
            var pago = new Pago
            {
                IdVenta        = venta.IdVenta,
                MetodoPago     = req.MetodoPago ?? "PayPal",
                Monto          = monto,
                Moneda         = "PEN",
                Estado         = pagoCompletado ? "Completado" : "Pendiente",
                ReferenciaPago = req.ReferenciaPago,
                FechaPago      = DateTime.Now
            };
            db.Pagos.Add(pago);

            // Si el pago se completo, marcar el pedido como Pagado (estado 2)
            if (pagoCompletado)
                venta.IdEstadoPedido = 2;

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new CrearPedidoResponse
            {
                IdVenta       = venta.IdVenta,
                MontoTotal    = monto,
                TotalProducto = totalUnidades,
                Mensaje       = pagoCompletado
                    ? "Pedido registrado y pago completado."
                    : "Pedido registrado. Pago pendiente."
            });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, $"Error al registrar el pedido: {ex.Message}");
        }
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
