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
using System.ComponentModel;
using Microsoft.Msagl.Core.Layout;
using MSAGLPoint = Microsoft.Msagl.Core.Geometry.Point;
using MSAGLRectangle = Microsoft.Msagl.Core.Geometry.Rectangle;
using MSAGLNode = Microsoft.Msagl.Core.Layout.Node;
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Initial;
using System.Diagnostics;
using Microsoft.Msagl.Core.Geometry.Curves;

namespace Mindmappy.Shared
{
    public sealed partial class AddEdgeCursor : Page, INotifyPropertyChanged
    {
        public MSAGLNode Node { get; set; }
        public new MainPage Parent { get; set; }
        public GeometryGraph Graph { get; set; }
        public FastIncrementalLayoutSettings LayoutSettings { get; set; }

        public AddEdgeCursor(Node node, MainPage parent, GeometryGraph graph, FastIncrementalLayoutSettings settings)
        {
            Node = node;
            Parent = parent;
            Graph = graph;
            LayoutSettings = settings;

            InitializeComponent();

            Parent.Canvas.Children.Add(this);
        }

        public void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var cursorPos = e.GetCurrentPoint(Parent).Position;
            Node.Center = new MSAGLPoint(cursorPos.X, cursorPos.Y);
            LayoutHelpers.RouteAndLabelEdges(Graph, LayoutSettings, Graph.Edges);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
