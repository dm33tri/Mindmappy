using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using CollabLib;

namespace Mindmappy.Service.Hubs
{
    public static class Shared
    {
        public static Document doc = new Document();

       static Shared()
        {
            doc.AddArray("nodes");
            doc.AddArray("edges");
        }
    }
    public class MindmappyHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Clients.Client(Context.ConnectionId).SendAsync("Update", Shared.doc.EncodeState());
            return base.OnConnectedAsync();
        }

        public void Update(byte[] message)
        {
            Clients.Others.SendAsync("Update", message);
            Shared.doc.ApplyUpdate(message);
        }

        public void Reset()
        {
            Shared.doc = new Document();
            Shared.doc.AddArray("nodes");
            Shared.doc.AddArray("edges");
            Clients.All.SendAsync("Reset");
        }
    }
}
