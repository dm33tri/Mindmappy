using System;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using System.ServiceModel.Channels;
using CollabLib;
using CollabLib.Content;
using CollabLib.Struct;
using MSAGLNode = Microsoft.Msagl.Core.Layout.Node;

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
            document.AddArray("nodes");
        }

        public void AddNode(MSAGLNode node)
        {
            var array = document.GetArray("nodes");
            //array.Insert()
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
