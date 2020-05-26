using System;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using CollabLib;
using CollabLib.Content;
using CollabLib.Struct;
using MSAGLNode = Microsoft.Msagl.Core.Layout.Node;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Initial;
using Microsoft.Msagl.Miscellaneous;
using MSAGLPoint = Microsoft.Msagl.Core.Geometry.Point;
using Microsoft.Msagl.Drawing;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mindmappy.Shared
{
    public class NodeUserData
    {
        public int Id { get; set; }
        public string Label { get; set; }
    }

    public class CollabBinding
    {
        Document document;
        Websockets ws;
        List<MSAGLNode> nodes;

        public static byte[] PointToData(MSAGLPoint point)
        {
            return BitConverter.GetBytes(point.X).Concat(BitConverter.GetBytes(point.Y)).ToArray();
        }

        public static MSAGLPoint DataToPoint(byte[] data)
        {
            return new MSAGLPoint
            {
                X = BitConverter.ToDouble(data, 0),
                Y = BitConverter.ToDouble(data, 8)
            };
        }

        public CollabBinding()
        {
            document = new Document();
            document.clientId = 0;
            nodes = new List<MSAGLNode>();
            ws = new Websockets(document, "ws://localhost:32771/doc");
            var map = document.AddMap("nodes");
            InitConnection();
        }

        public async Task InitConnection()
        {
            await ws.Connect();
            foreach (var node in nodes)
            {
                node.BeforeLayoutChangeEvent += Node_BeforeLayoutChangeEvent;
            }
            ws.Listen();
        }

        public void AddNode(MSAGLNode node)
        {
            int index = nodes.Count;
            node.UserData = index; // save node index
            nodes.Add(node);
            var map = document.GetMap("nodes");
            map.Set(index.ToString(), new ContentBinary(PointToData(node.Center)));
            map.Update += Map_Update;
        }

        private void Map_Update(AbstractStruct sender, string[] changedKeys)
        {
            Debug.WriteLine(string.Join(",", changedKeys));
        }

        private void Node_BeforeLayoutChangeEvent(object sender, LayoutChangeEventArgs e)
        {
            if (e.DataAfterChange is MSAGLPoint) // change center
            {
                var map = document.GetMap("nodes");
                map.Set(
                    ((int)(sender as MSAGLNode).UserData).ToString(), 
                    new ContentBinary(PointToData((MSAGLPoint)e.DataAfterChange))
                );
            }
        }

        private static CollabBinding globalBinding = null;
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
