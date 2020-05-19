﻿using System;
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

using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Initial;
using MSAGLNode = Microsoft.Msagl.Core.Layout.Node;
using MSAGLPoint = Microsoft.Msagl.Core.Geometry.Point;
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
            return new MSAGLNode(CurveFactory.CreateRectangle(150, 60, new MSAGLPoint(0, 0)), id);
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
            for (int i = 0; i < graph.Nodes.Count; ++i)
            {
                new UINode(graph.Nodes[i], canvas, graph, layoutSettings);
            }
        }

        public void DrawEdges()
        {
            for (
                var enumerator = graph.Edges.GetEnumerator(); 
                enumerator.MoveNext(); )
            {
                new UIEdge(enumerator.Current, canvas);
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
                NodeSeparation = 64,
                AvoidOverlaps = true,
                MinConstraintLevel = 1,
            };
            layout = new InitialLayout(graph, layoutSettings);
            layout.SingleComponent = true;
            graph.AlgorithmData = layout;
            layout.Run();
            LayoutHelpers.RouteAndLabelEdges(graph, layoutSettings, graph.Edges);
            InitializeComponent();

            grid.SizeChanged += InitView;
        }

        private void InitView(object sender, SizeChangedEventArgs e)
        {
            if (grid.ActualHeight > 0 && grid.ActualWidth > 0)
            {
                canvas.Width = grid.ActualWidth;
                canvas.Height = grid.ActualHeight;
                NormalizeGraph();
                DrawEdges();
                DrawNodes();
            }

            grid.SizeChanged -= InitView;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
