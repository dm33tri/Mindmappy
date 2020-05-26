using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

#if __WASM__
using ClientWebSocket = Uno.Wasm.WebSockets.WasmWebSocket;
#else
using ClientWebSocket = System.Net.WebSockets.ClientWebSocket;
#endif

namespace CollabLib
{
    public class Websockets
    {

        ClientWebSocket client;
        CancellationTokenSource cancel;
        Document doc;
        string url;

        public Websockets(Document doc, string url) {
            client = new ClientWebSocket();
            this.doc = doc;
            this.url = url;
            doc.Update += (sender, changes) => Doc_Update(sender, changes);
            cancel = new CancellationTokenSource();
        }

        public async Task<int> GetId()
        {
            var buffer = new ArraySegment<byte>(new byte[4096]);

            if (client.State == WebSocketState.Open)
            {
                await client.ReceiveAsync(buffer, cancel.Token);
            }

            return BitConverter.ToInt32(buffer.Array, 0);
        }

        private async Task Doc_Update(Document sender, byte[] changes)
        {
            if (client.State == WebSocketState.Open)
            {
                client.SendAsync(new ArraySegment<byte>(changes), WebSocketMessageType.Binary, true, cancel.Token);
            }
        }

        public async Task Connect()
        {
            await client.ConnectAsync(new Uri(url), cancel.Token);
            if (client.State == WebSocketState.Open)
            {
                await client.SendAsync(new ArraySegment<byte>(doc.EncodeState()), WebSocketMessageType.Binary, true, cancel.Token);
            }
        }

        public async Task Listen()
        {
            var buffer = new ArraySegment<byte>(new byte[4096]);

            while (!cancel.IsCancellationRequested)
            {
                try
                {
                    var result = await client.ReceiveAsync(buffer, cancel.Token);
                    if (result.MessageType == WebSocketMessageType.Binary && result.EndOfMessage)
                    {
                        doc.ApplyUpdate(buffer.Array);
                    }
                }
                catch
                {

                }
            }
        }
    }
}
