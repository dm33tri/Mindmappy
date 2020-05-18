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
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Initial;
using System.Diagnostics;

namespace Mindmappy.Shared
{
    public sealed partial class UINode : Page, INotifyPropertyChanged
    {
        public string Label { get; set; } = "Node label";
        public Node Node { get; set; }
        public new Canvas Parent { get; set; }
        public GeometryGraph Graph { get; set; }
        public FastIncrementalLayoutSettings LayoutSettings { get; set; }

        private Point grabPoint;

        private bool moved = false;

        public double Top
        {
            get => Node?.BoundingBox.Bottom ?? 0; // 🤷‍
        }

        public double Left
        {
            get => Node?.BoundingBox.Left ?? 0;
        }

        public double RectWidth
        {
            get => Node?.BoundingBox.Width ?? 100;
        }

        public double RectHeight
        {
            get => Node?.BoundingBox.Height ?? 100;
        }

        public UINode(Node node, Canvas parent, GeometryGraph graph, FastIncrementalLayoutSettings settings)
        {
            Node = node;
            Parent = parent;
            Graph = graph;
            LayoutSettings = settings;

            InitializeComponent();

            Node.BeforeLayoutChangeEvent += (target, e) =>
            {
                OnPropertyChanged("Left");
                OnPropertyChanged("Top");
                LayoutHelpers.RouteAndLabelEdges(Graph, LayoutSettings, Node.Edges);
            };

            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;
            PointerMoved += OnPointerMoved;

            Parent.Children.Add(this);
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (PointerCaptures?.Count > 0)
            {
                moved = true;
                var cursorPos = e.GetCurrentPoint(Parent).Position;
                var p1 = new MSAGLPoint(cursorPos.X, cursorPos.Y);
                var p2 = new MSAGLPoint(grabPoint.X, grabPoint.Y);
                var p3 = new MSAGLPoint(Node.Width / 2, Node.Height / 2);
                Node.Center = p1 + p3 - p2;
            }
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleasePointerCapture(e.Pointer);
            if (moved)
            {
                LayoutHelpers.RouteAndLabelEdges(Graph, LayoutSettings, Graph.Edges);
            }
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            CapturePointer(e.Pointer);
            grabPoint = e.GetCurrentPoint(this).Position;
            moved = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
