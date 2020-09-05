using System;
using System.Collections.Generic;
using CollabLib;
using CollabLib.Struct;
using MSAGLNode = Microsoft.Msagl.Core.Layout.Node;
using Microsoft.Msagl.Core.Layout;
using MSAGLPoint = Microsoft.Msagl.Core.Geometry.Point;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

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
            //document = new Document();
            //document.clientId = 0;
            //nodes = new List<MSAGLNode>();
            //var map = document.AddMap("nodes");
            //InitConnection();
        }

        public async Task InitConnection()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:44562/mindmappy")
                .WithAutomaticReconnect()
                .Build();

            connection.On<string, byte[]>("ReceiveMessage", (user, message) => {
                Console.WriteLine(message);
            });
        }

        public void AddNode(MSAGLNode node)
        {
            //int index = nodes.Count;
            //node.UserData = index; // save node index
            //nodes.Add(node);
            //var map = document.GetMap("nodes");
            //map.Set(index.ToString(), new ContentBinary(PointToData(node.Center)));
            //map.Update += Map_Update;
        }

        private void Map_Update(AbstractStruct sender, string[] changedKeys)
        {
            //Debug.WriteLine(string.Join(",", changedKeys));
        }

        private void Node_BeforeLayoutChangeEvent(object sender, LayoutChangeEventArgs e)
        {
            //if (e.DataAfterChange is MSAGLPoint) // change center
            //{
            //    var map = document.GetMap("nodes");
            //    map.Set(
            //        ((int)(sender as MSAGLNode).UserData).ToString(), 
            //        new ContentBinary(PointToData((MSAGLPoint)e.DataAfterChange))
            //    );
            //}
        }

        private static CollabBinding globalBinding = null;
        public static CollabBinding Binding
        {
            get => globalBinding ?? (globalBinding = new CollabBinding()); 
        }
    }
}
