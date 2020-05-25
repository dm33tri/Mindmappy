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
        public Websockets(Document doc, string url) {
            client = new ClientWebSocket();
            this.doc = doc;
            doc.Update += (sender, changes) => Doc_Update(sender, changes);
            cancel = new CancellationTokenSource();
            Connect(url);
        }

        private async Task Doc_Update(Document sender, byte[] changes)
        {
            await client.SendAsync(new ArraySegment<byte>(changes), WebSocketMessageType.Binary, true, cancel.Token);
        }

        public async Task Connect(string url)
        {
            await client.ConnectAsync(new Uri(url), cancel.Token);
            Listen();
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
