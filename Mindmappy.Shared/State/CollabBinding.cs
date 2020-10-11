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

        HubConnection connection;
        public async Task InitConnection()
        {
            connection = new HubConnectionBuilder()
                .WithUrl("https://mindmappyservice20201007154114.azurewebsites.net/mindmappy")
                .WithAutomaticReconnect()
                .Build();

            connection.On("Reset", () => Controller.Reset());
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

        private void Nodes_Update(object sender, string[] changedKeys)
        {
            while (nodes.length > Controller.UINodes.Count)
            {
                var diff = nodes.length - Controller.UINodes.Count;
                Map newNode = nodes[nodes.length - diff] as Map;
                AddNode(newNode);
            }
        }

        public void Reset()
        {
            connection.SendAsync("Reset");
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
                            if (x != node.Left)
                            {
                                node.Left = x;
                                node.Relayout();
                            }
                            break;
                        case "y":
                            double y = BitConverter.ToDouble((map.Get("y") as ContentBinary).data);
                            double diffY = y - node.Top;
                            if (y != node.Top)
                            {
                                node.Top = y;
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
            var x = map.Get("x") is ContentBinary ? BitConverter.ToDouble((map.Get("x") as ContentBinary).data) : 0;
            var y = map.Get("y") is ContentBinary ? BitConverter.ToDouble((map.Get("y") as ContentBinary).data) : 0;
            node.Label = text?.ToString() ?? "";
            node.Left = x;
            node.Top = y;
            SetNodeUpdateHandler(node, map);
        }

        public void AddNode(UINode node)
        {
            var map = new Map();
            document.Transact((transaction) =>
            {
                nodes.InsertFunc(nodes.length, map, transaction);
                map.SetFunc("x", new ContentBinary(BitConverter.GetBytes(node.Left)), transaction);
                map.SetFunc("y", new ContentBinary(BitConverter.GetBytes(node.Top)), transaction);
                var text = new Text();
                map.SetFunc("text", text, transaction);
            });
            SetNodeUpdateHandler(node, map);
        }

        public void AddEdge(Map map)
        {
            var from = (map.Get("from") as ContentString).str;
            var to = (map.Get("to") as ContentString).str;
            if (from != null && to != null)
            {
                Controller.AddEdge(from, to);
            }
        }

        public void AddEdge(Edge edge)
        {
            var map = new Map();
            document.Transact((transaction) =>
            {
                edges.InsertFunc(edges.length, map, transaction);
                map.SetFunc("from", new ContentString(edge.Source), transaction);
                map.SetFunc("to", new ContentString(edge.Target), transaction);
            });
        }
    }
}
