using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Initial;
using Microsoft.Msagl.Miscellaneous;
using SvgGraphWriter = Microsoft.Msagl.Drawing.SvgGraphWriter;
using Graph = Microsoft.Msagl.Drawing.Graph;
using Node = Microsoft.Msagl.Drawing.Node;
using Edge = Microsoft.Msagl.Drawing.Edge;
using System.IO;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using Windows.Graphics.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.Msagl.Layout.Layered;
using System.Diagnostics;
using Microsoft.Msagl.Drawing;

namespace Mindmappy.Shared
{
    public delegate void UnfocusEventHandler();

    public class Controller
    {
        public GeometryGraph Graph { get => DrawingGraph.GeometryGraph; }
        public Graph DrawingGraph { get; set; }
        public LayoutAlgorithmSettings LayoutSettings { get => DrawingGraph.LayoutAlgorithmSettings; }
        public Edge SelectedEdge { get; set; }
        public Node SelectedNode { get; set; }
        public CollabBinding CollabBinding { get; set; }
        public event UnfocusEventHandler Unfocus;
        public string ImagePath { get; set; }
        public void UnfocusAll()
        {
            Unfocus();
        }
        public async Task WriteGraph()
        {
            Windows.Storage.StorageFolder storageFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile = await storageFolder.CreateFileAsync("sample.svg",
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);
            var stream = await sampleFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            using (var outputStream = stream.GetOutputStreamAt(0).AsStreamForWrite())
            {
                SvgGraphWriter writer = new SvgGraphWriter(outputStream, DrawingGraph);
                writer.Write();
            }
            ImagePath = sampleFile.Path;
            stream.Dispose();
        }
        public Controller()
        {
            CreateGraph();
            CollabBinding = new CollabBinding(this);
        }

        private void CreateGraph()
        {
            DrawingGraph = new Graph();
            DrawingGraph.LayoutAlgorithmSettings = new FastIncrementalLayoutSettings
            {
                NodeSeparation = 64,
                AvoidOverlaps = true,
                MinConstraintLevel = 1,
            };

            for (int i = 0; i < 5; i++)
            {
                DrawingGraph.AddNode(i.ToString());
                for (int j = 0; j < i; ++j)
                {
                    DrawingGraph.AddEdge(i.ToString(), j.ToString());
                }
            }
            DrawingGraph.CreateGeometryGraph();
            foreach (var node in DrawingGraph.Nodes)
            {
                node.GeometryNode.BoundaryCurve = NodeBoundaryCurves.GetNodeBoundaryCurve(node, 150, 60);
            }

            var layout = new InitialLayout(Graph, LayoutSettings as FastIncrementalLayoutSettings);
            layout.Run();
            LayoutHelpers.RouteAndLabelEdges(Graph, LayoutSettings, Graph.Edges);
        }
    }
}
