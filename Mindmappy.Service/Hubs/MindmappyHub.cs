using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Mindmappy.Service.Hubs
{
    public class MindmappyHub : Hub
    {
        int lastClientId = 0;
        public async Task OnConnected()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("Connected", lastClientId);
        }

        public async Task SendMessage(int user, byte[] message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
