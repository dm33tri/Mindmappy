using System;
using System.Collections.Generic;
using CollabLib;
using CollabLib.Struct;
using CollabLib.Content;
using MSAGLNode = Microsoft.Msagl.Core.Layout.Node;
using Microsoft.Msagl.Core.Layout;
using MSAGLPoint = Microsoft.Msagl.Core.Geometry.Point;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using Array = CollabLib.Struct.Array;
using Node = Microsoft.Msagl.Drawing.Node;
using Microsoft.Msagl.Drawing;

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
        Array nodes;
        public Controller Controller { get; set; }
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

        public CollabBinding(Controller controller)
        {
            Controller = controller;
            document = new Document();
            nodes = document.AddArray("nodes");
            InitConnection();
        }

        public async Task InitConnection()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:6608/mindmappy")
                .WithAutomaticReconnect()
                .Build();

            connection.On<byte[]>("ReceiveMessage", (message) => {      
                document.ApplyUpdate(message);
            });
            document.Update += (doc, data) =>
            {
                connection.SendAsync("ReceiveMessage", data);
            };
            try
            {
                await connection.StartAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            AddNodes();
        }

        public void AddNodes()
        {
            Action<MSAGLNode> AddNode = (MSAGLNode node) =>
            {
                var map = new Map();
                nodes.Push(map);
                map.Set("x", new ContentBinary(BitConverter.GetBytes(node.BoundingBox.Center.X)));
                map.Set("y", new ContentBinary(BitConverter.GetBytes(node.BoundingBox.Center.Y)));
                map.Set("text", new Text());
                map.Update += (sender, changedKeys) =>
                {
                    
                };
                node.BeforeLayoutChangeEvent += (sender, e) =>
                {
                    if (e.DataAfterChange is MSAGLPoint)
                    {
                        var p = (MSAGLPoint) e.DataAfterChange;
                        map.Set("x", new ContentBinary(BitConverter.GetBytes(p.X)));
                        map.Set("y", new ContentBinary(BitConverter.GetBytes(p.Y)));
                    }
                };
            };

            foreach (var node in Controller.Graph.Nodes)
            {
                AddNode(node);
            }
        }
    }
}
