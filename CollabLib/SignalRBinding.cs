using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace CollabLib
{
    public class SignalRBinding
    {
        HubConnection connection;
        Document doc;
        string url;

        public SignalRBinding(Document doc, string url) {
            this.doc = doc;
            this.url = url;

            connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();

            connection.On<int, byte[]>("SendMessage", OnMessage);
            connection.On<int>("Connected", OnConnected);

            doc.Update += Doc_Update;
        }

        public async Task<int> GetId()
        {
            return 0;
        }

        private void Doc_Update(Document sender, byte[] changes)
        {
            connection.InvokeAsync("SendMessage", sender.clientId, changes);
        }

        public async Task Connect()
        {
            await connection.StartAsync();
        }

        public void OnMessage(int user, byte[] data)
        {
            doc.ApplyUpdate(data);
        }

        public async Task OnConnected(int clientId)
        {
            doc.clientId = clientId;
            // TODO send state
        }
    }
}
