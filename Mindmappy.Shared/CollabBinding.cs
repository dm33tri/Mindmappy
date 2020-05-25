using System;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using System.ServiceModel.Channels;
using CollabLib;
namespace Mindmappy.Shared
{
    class CollabBinding
    {
        Document document;
        Websockets ws;
        public CollabBinding()
        {
            document = new Document();
            ws = new Websockets(document, "ws://localhost:32770/doc");

        }

        private static CollabBinding globalBinding;
        public static CollabBinding Binding
        {
            get
            {
                if (globalBinding == null)
                {
                    globalBinding = new CollabBinding();
                }

                return globalBinding; 
            }
        }
    }
}
