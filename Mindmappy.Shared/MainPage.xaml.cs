using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Diagnostics;
using System.ComponentModel;

using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.DebugHelpers;
using Microsoft.Msagl.DebugHelpers.Persistence;
using Microsoft.Msagl.Routing;
using Microsoft.Msagl.Routing.Rectilinear;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Initial;
using MSAGLNode = Microsoft.Msagl.Core.Layout.Node;
using XAMLRectange = Windows.UI.Xaml.Shapes.Rectangle;
using XAMLPath = Windows.UI.Xaml.Shapes.Path;
using MSAGLPoint = Microsoft.Msagl.Core.Geometry.Point;
using MSAGLRectangle = Microsoft.Msagl.Core.Geometry.Rectangle;
using LineSegment = Microsoft.Msagl.Core.Geometry.Curves.LineSegment;
using Ellipse = Microsoft.Msagl.Core.Geometry.Curves.Ellipse;
using System.Threading;
using System.Threading.Tasks;
using Size = Microsoft.Msagl.Core.DataStructures.Size;
using Microsoft.Msagl.Layout.MDS;
using Microsoft.Msagl.Miscellaneous;
using Mindmappy.Shared;

namespace Mindmappy {

    public sealed partial class MainPage : INotifyPropertyChanged
    {
        public string LineData { get; set; } = "";

        public static Edge CreateEdge(Node source, Node target)
        {
            return new Edge(source, target);
        }
        public static MSAGLNode CreateNode(int id)
        {
            return new MSAGLNode(CurveFactory.CreateRectangle(150, 50, new MSAGLPoint(0, 0)), id);
        }

        public GeometryGraph GetGraph()
        {
            GeometryGraph graph = new GeometryGraph();

            MSAGLNode[] allNodes = new MSAGLNode[5];

            for (int i = 0; i < 5; i++)
            {
                Node node = CreateNode(i);
                allNodes[i] = node;
                graph.Nodes.Add(node);
                for (int j = 0; j < i; ++j)
                {
                    graph.Edges.Add(CreateEdge(allNodes[j], allNodes[i]));
                }
            }

            return graph;
        }

        public GeometryGraph graph;

        public InitialLayout layout;
        public FastIncrementalLayoutSettings layoutSettings;

        public void DrawNodes()
        {
            foreach (MSAGLNode node in graph.Nodes)
            {
                new UINode(node, canvas, graph, layoutSettings);
            }
        }

        public void DrawEdges()
        {
            foreach (Edge edge in graph.Edges)
            {
                new UIEdge(edge, canvas);
            }
        }

        public void NormalizeGraph()
        {
            MSAGLPoint center = new MSAGLPoint(canvas.Width / 2, canvas.Height / 2);
            graph.Translate(center - graph.BoundingBox.Center);
            graph.UpdateBoundingBox();
        }

        public MainPage()
        {
            graph = GetGraph();
            layoutSettings = new FastIncrementalLayoutSettings
            {
                RouteEdges = true,
                NodeSeparation = 100,
                AvoidOverlaps = true,
            };
            layout = new InitialLayout(graph, layoutSettings);
            graph.AlgorithmData = layout;
            layout.Run();

            LayoutHelpers.RouteAndLabelEdges(graph, layoutSettings, graph.Edges);

            InitializeComponent();

            grid.SizeChanged += (sender, e) =>
            {
                if (grid.ActualHeight > 0 && grid.ActualWidth > 0)
                {
                    canvas.Width = grid.ActualWidth;
                    canvas.Height = grid.ActualHeight;
                    NormalizeGraph();
                    DrawEdges();
                    DrawNodes();
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
