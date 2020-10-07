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
using Edge = Microsoft.Msagl.Drawing.Edge;
using Node = Microsoft.Msagl.Drawing.Node;
using Microsoft.Msagl.Drawing;

namespace Mindmappy.Shared
{
    public class CollabBinding
    {
        Document document;
        Array nodes;
        Array edges;
        public Controller Controller { get; set; }

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
            edges = document.AddArray("edges");
            InitConnection();
        }

        public async Task InitConnection()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("https://mindmappyservice20201007154114.azurewebsites.net/mindmappy")
                .WithAutomaticReconnect()
                .Build();

            connection.On("First", () => AddNodes());

            connection.On<string>("NewUser", (id) => {      
                connection.SendAsync("NewUser", id, document.EncodeState());
            });

            connection.On<byte[]>("Update", (message) => {
                document.ApplyUpdate(message);
            });

            nodes.Update += Nodes_Update;
            edges.Update += Edges_Update;
            document.Update += (doc, data) =>
            {
                connection.SendAsync("Update", data);
            };

            try
            {
                await connection.StartAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void Edges_Update(object sender, string[] changedKeys)
        {
            while (edges.length > Controller.Graph.EdgeCount)
            {
                var diff = edges.length - Controller.Graph.EdgeCount;
                Map newEdge = edges[edges.length - diff] as Map;
                AddEdge(newEdge);
            }
        }

        public void Nodes_Update(object sender, string[] changedKeys)
        {
            while (nodes.length > Controller.UINodes.Count)
            {
                var diff = nodes.length - Controller.UINodes.Count;
                Map newNode = nodes[nodes.length - diff] as Map;
                AddNode(newNode);
            }
        }

        public void SetNodeUpdateHandler(UINode node, Map map)
        {
            Text text = map.Get("text") as Text;
            if (text == null)
            {
                text = new Text();
                map.Set("text", text);
            }
            node.PropertyChanged += (sender, e) =>
            {
                var t = map.Get("text") as Text;
                switch (e.PropertyName)
                {
                    case "Label":
                        text.Diff(node.Label);
                        break;
                    case "Pos":
                        map.Set("x", new ContentBinary(BitConverter.GetBytes(node.Left)));
                        map.Set("y", new ContentBinary(BitConverter.GetBytes(node.Top)));
                        break;
                }
            };
            map.Update += (sender, changedKeys) =>
            {
                foreach (var key in changedKeys)
                {
                    switch (key)
                    {
                        case "x":
                            double x = BitConverter.ToDouble((map.Get("x") as ContentBinary).data);
                            double diffX = x - node.Left;
                            if (diffX != 0)
                            {
                                node.Node.Center += new MSAGLPoint(diffX, 0);
                                node.OnPropertyChanged("Node");
                                node.Relayout();
                            }
                            break;
                        case "y":
                            double y = BitConverter.ToDouble((map.Get("y") as ContentBinary).data);
                            double diffY = y - node.Top;
                            if (diffY != 0)
                            {
                                node.Node.Center += new MSAGLPoint(0, diffY);
                                node.OnPropertyChanged("Node");
                                node.Relayout();
                            }
                            break;
                    }
                }
            };
            text.Update += (sender, _) =>
            {
                if (text.ToString() != node.Label)
                {
                    node.Label = text.ToString();
                }
            };
        }

        public void AddNode(Map map)
        {
            var node = Controller.AddNode();
            var text = map.Get("text") as Text;
            var x = BitConverter.ToDouble((map.Get("x") as ContentBinary).data);
            var y = BitConverter.ToDouble((map.Get("y") as ContentBinary).data);
            node.Label = text?.ToString() ?? "";
            node.Left = x;
            node.Top = y;
            SetNodeUpdateHandler(node, map);
        }

        public void AddNode(UINode node)
        {
            var map = new Map();
            nodes.Push(map);
            map.Set("x", new ContentBinary(BitConverter.GetBytes(node.Left)));
            map.Set("y", new ContentBinary(BitConverter.GetBytes(node.Top)));
            var text = new Text();
            map.Set("text", text);
            SetNodeUpdateHandler(node, map);
        }

        public void AddEdge(Map map)
        {
            var from = (map.Get("from") as ContentString).str;
            var to = (map.Get("to") as ContentString).str;
            Controller.AddEdge(from, to);
        }

        public void AddEdge(Edge edge)
        {
            var map = new Map();
            edges.Push(map);
            map.Set("from", new ContentString(edge.Source));
            map.Set("to", new ContentString(edge.Target));
        }

        public void AddNodes()
        {
            Controller.CreateNodes();
            foreach (var node in Controller.UINodes)
            {
                AddNode(node);
            }
            foreach (var edge in Controller.Graph.Edges)
            {
                AddEdge(edge);
            }
        }
    }
}
