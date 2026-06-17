using Microsoft.AspNetCore.SignalR;

namespace FeastOfTheNarts.Web.Hubs
{
    public class GameHub : Hub
    {
        public async Task JoinMatch(string playerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "waiting_room");
            await Clients.Caller.SendAsync("ReceiveMessage", "Система", "Вы присоединились к комнате ожидания.");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}