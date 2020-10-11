using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Initial;
using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.Msagl.Drawing;
using System.Collections.Generic;

using SvgGraphWriter = Microsoft.Msagl.Drawing.SvgGraphWriter;
using Graph = Microsoft.Msagl.Drawing.Graph;
using Node = Microsoft.Msagl.Drawing.Node;
using Edge = Microsoft.Msagl.Drawing.Edge;
using GeomNode = Microsoft.Msagl.Core.Layout.Node;
using GeomEdge = Microsoft.Msagl.Core.Layout.Edge;

using Microsoft.Msagl.Miscellaneous;

namespace Mindmappy.Shared
{
    public delegate void UnfocusEventHandler();

    public class Controller
    {
        public GraphViewer GraphViewer { get; private set; }
        public Graph Graph { get; private set; }
        public GeometryGraph GeometryGraph { get => Graph.GeometryGraph; }
        public LayoutAlgorithmSettings LayoutSettings { get => Graph.LayoutAlgorithmSettings; }
        public List<UINode> UINodes { get; } = new List<UINode>();
        public List<UIEdge> UIEdges { get; } = new List<UIEdge>();
        public UINode EdgeFromNode { get; set; }
        public Edge SelectedEdge { get; set; }
        public Node SelectedNode { get; set; }
        public CollabBinding CollabBinding { get; set; }
        public event UnfocusEventHandler Unfocus;
        public string ImagePath { get; set; }

        public void UnfocusAll() => Unfocus();

        public async Task WriteGraph()
        {
            Windows.Storage.StorageFile file = await Windows.Storage.DownloadsFolder.CreateFileAsync("mindmap.svg", Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            using (var outputStream = stream.GetOutputStreamAt(0).AsStreamForWrite())
            {
                SvgGraphWriter writer = new SvgGraphWriter(outputStream, Graph);
                writer.Write();
            }
            stream.Dispose();
        }

        public Controller(GraphViewer graphViewer)
        {
            GraphViewer = graphViewer;
            CreateGraph();
            CreateCollabBinding();
        }

        public UINode AddNode()
        {
            Node node = Graph.AddNode(Graph.NodeCount.ToString());
            node.LabelText = "";
            GeomNode geomNode = GeometryGraphCreator.CreateGeometryNode(Graph, GeometryGraph, node, ConnectionToGraph.Connected);
            geomNode.BoundaryCurve = NodeBoundaryCurves.GetNodeBoundaryCurve(node, 150, 60);
            geomNode.Center = new Microsoft.Msagl.Core.Geometry.Point(100, 100);
            UINode uiNode = new UINode { Node = node, Controller = this, ParentPage = GraphViewer };
            UINodes.Add(uiNode);
            GraphViewer.Canvas.Children.Add(uiNode);
            return uiNode;
        }

        public Edge AddEdge(string from, string to)
        {
            Edge edge = Graph.AddEdge(from, to);
            GeomEdge geomEdge = GeometryGraphCreator.CreateGeometryEdgeFromDrawingEdge(edge);
            GeometryGraph.Edges.Add(geomEdge);
            LayoutHelpers.RouteAndLabelEdges(GeometryGraph, LayoutSettings, GeometryGraph.Edges);
            return edge;
        }

        public void CreateNode()
        {
            CollabBinding.AddNode(AddNode());
        }

        public void CreateEdge(string from, string to)
        {
            CollabBinding.AddEdge(AddEdge(from, to));
        }

        private void CreateCollabBinding()
        {
            this.CollabBinding = new CollabBinding(this);
        }

        public void Reset()
        {

        }

        private void CreateGraph()
        {
            Graph = new Graph();

            Graph.LayoutAlgorithmSettings = new FastIncrementalLayoutSettings
            {
                NodeSeparation = 64,
                AvoidOverlaps = true,
                MinConstraintLevel = 1,
            };

            Graph.CreateGeometryGraph();
        }
    }
}
