﻿using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Layout.Incremental;
using MSAGLPoint = Microsoft.Msagl.Core.Geometry.Point;
using MSAGLNode = Microsoft.Msagl.Core.Layout.Node;

namespace Mindmappy.Shared
{
    public sealed partial class UINode : Page, INotifyPropertyChanged
    {
        public string Label { get; set; }
        public MSAGLNode Node { get; set; }
        public GraphViewer ParentPage { get; set; }
        public GeometryGraph Graph { get; set; }
        public FastIncrementalLayoutSettings LayoutSettings { get; set; }

        public double Top { get => Node?.BoundingBox.Bottom ?? 0; }
        public double Left { get => Node?.BoundingBox.Left ?? 0; }
        public double NodeWidth { get => Node?.BoundingBox.Width ?? 100; }
        public double NodeHeight { get => Node?.BoundingBox.Height ?? 100; }

        private bool active;
        public bool Active
        {
            get => active;
            set
            {
                active = value;
                OnPropertyChanged("Active");
                OnPropertyChanged("Stroke");
            }
        }

        private SolidColorBrush defaultBrush = new SolidColorBrush { Color = Colors.Gray };
        private SolidColorBrush highlightBrush = new SolidColorBrush { Color = Color.FromArgb(255, 0, 120, 212) };

        public Brush Stroke { get => active ? highlightBrush : defaultBrush; }

        public UINode(MSAGLNode node, GraphViewer parent, GeometryGraph graph, FastIncrementalLayoutSettings settings)
        {
            Node = node;
            ParentPage = parent;
            Graph = graph;
            LayoutSettings = settings;

            InitializeComponent();

            Tapped += OnTapped;
            removeButton.Tapped += OnRemoveClick;
            addEdgeButton.Tapped += OnAddEdgeClick;
            ParentPage.Unfocus += Unfocus;
            ParentPage.Canvas.Children.Add(this);
            ManipulationDelta += UINode_ManipulationDelta;
            resizePoint.ManipulationDelta += ResizePoint_ManipulationDelta;
        }

        private void ResizePoint_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            var delta = new MSAGLPoint(e.Delta.Translation.X, e.Delta.Translation.Y);

            var Box = Node.BoundingBox;
            if (Box.Width + delta.X >= 150)
            {
                Box.Right += delta.X;
            }
            if (Box.Height + delta.Y >= 60)
            {
                Box.Top += delta.Y;
            }
            Node.BoundingBox = Box;

            if (Node.BoundingBox.Right >= ParentPage.CanvasWidth - 20)
            {
                ParentPage.CanvasWidth += 500;
            }
            if (Node.BoundingBox.Bottom >= ParentPage.CanvasHeight - 20)
            {
                ParentPage.CanvasHeight += 500;
            }
            OnPropertyChanged("NodeWidth");
            OnPropertyChanged("NodeHeight");
            LayoutHelpers.RouteAndLabelEdges(Graph, LayoutSettings, Graph.Edges);
        }

        private void UINode_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            var delta = new MSAGLPoint(e.Delta.Translation.X, e.Delta.Translation.Y);
            Node.Center += delta;

            if (Node.BoundingBox.Right >= ParentPage.CanvasWidth - 20)
            {
                ParentPage.CanvasWidth += 500;
            }
            if (Node.BoundingBox.Bottom >= ParentPage.CanvasHeight - 20)
            {
                ParentPage.CanvasHeight += 500;
            }
            OnPropertyChanged("Left");
            OnPropertyChanged("Top");
            LayoutHelpers.RouteAndLabelEdges(Graph, LayoutSettings, Graph.Edges);
        }

        private void Unfocus()
        {
            Active = false;
            overlay.Visibility = Visibility.Visible;
            textBox.IsReadOnly = true;
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            ParentPage.UnfocusAll();
            if (ParentPage.CursorEdge != null)
            {
                ParentPage.AttachEdge(this);
            }
            else
            {
                Focus();
            }
        }

        public void OnRemoveClick(object sender, RoutedEventArgs e)
        {
            var edges = Node.Edges.ToArray();
            foreach (var edge in edges)
            {
                (edge.UserData as UIEdge).Remove();
            }
            Graph.Nodes.Remove(Node);
            LayoutHelpers.RouteAndLabelEdges(Graph, LayoutSettings, Graph.Edges);
            ParentPage.Canvas.Children.Remove(this);
        }

        public void OnAddEdgeClick(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            ParentPage.AddCursorNode(this);
        }

        public void Focus()
        {
            textBox.IsReadOnly = false;
            Active = true;
            overlay.Visibility = Visibility.Collapsed;
#if !__ANDROID__
            textBox.IsTabStop = true;
            textBox.Focus(FocusState.Programmatic);
            textBox.IsTabStop = false;
#endif
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
