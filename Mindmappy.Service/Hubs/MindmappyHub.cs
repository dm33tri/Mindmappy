using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using CollabLib;

namespace Mindmappy.Service.Hubs
{
    public static class Shared
    {
        public static HashSet<string> ConnectedIds = new HashSet<string>();
    }
    public class MindmappyHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            if (Shared.ConnectedIds.Count > 0)
            {
                Clients.Client(Shared.ConnectedIds.First()).SendAsync("NewUser", Context.ConnectionId);
            } 
            else
            {
                Clients.Client(Context.ConnectionId).SendAsync("First");
            }
            Shared.ConnectedIds.Add(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Shared.ConnectedIds.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public void NewUser(string id, byte[] message)
        {
            Clients.Client(id).SendAsync("Update", message);
        }

        public void Update(byte[] message)
        {
            Clients.Others.SendAsync("Update", message);
        }
    }
}
