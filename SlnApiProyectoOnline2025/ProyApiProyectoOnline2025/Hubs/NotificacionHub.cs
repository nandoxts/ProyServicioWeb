using Microsoft.AspNetCore.SignalR;

public class NotificacionHub : Hub
{
    public async Task UnirseCliente(int idCliente)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"cliente_{idCliente}"
        );
    }
}
