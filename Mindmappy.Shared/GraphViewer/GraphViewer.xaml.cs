using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.ComponentModel;
using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Initial;
using Microsoft.Msagl.Miscellaneous;
using MSAGLNode = Microsoft.Msagl.Core.Layout.Node;
using MSAGLPoint = Microsoft.Msagl.Core.Geometry.Point;
using Mindmappy.Shared;
using System.Threading.Tasks;

namespace Mindmappy
{
    public sealed partial class GraphViewer : INotifyPropertyChanged
    {
        string imagePath;
        public string ImagePath { 
            get => imagePath;
            set {
                imagePath = value;
                OnPropertyChanged("ImagePath");
            }
        }
        public CancelToken cancelToken = new CancelToken();
        private UIEdge selectedEdge;
        public UIEdge SelectedEdge
        {
            get => selectedEdge;
            set
            {
                selectedEdge = value;
                OnPropertyChanged("IsEdgeSelected");
            }
        }
        public Visibility IsEdgeSelected { get => SelectedEdge != null ? Visibility.Visible : Visibility.Collapsed; }
        private AddEdgeCursor cursor;
        public UIEdge CursorEdge { get; set; }
        public void AddCursorNode(UINode origin)
        {
            var graph = Controller.GeometryGraph;
            var tempNode = new MSAGLNode(CurveFactory.CreateRectangle(1, 1, new MSAGLPoint(0, 0)));
            var tempEdge = new Edge(origin.Node, tempNode);
            graph.Nodes.Add(tempNode);
            graph.Edges.Add(tempEdge);
            CursorEdge = new UIEdge(tempEdge, this, graph, true);
            cursor = new AddEdgeCursor(tempNode, this, graph, layoutSettings);
            canvas.PointerMoved += cursor.OnPointerMoved;
        }

        private void OnResetFocus(object sender, RoutedEventArgs e)
        {
            Controller.UnfocusAll();
        }

        private void OnTapped(object sender, RoutedEventArgs e)
        {
            if (cursor != null)
            {
                AttachEdge(null);
            }
        }

        public void AttachEdge(UINode node)
        {
            var graph = Controller.GeometryGraph;
            var origin = CursorEdge.Edge.Source;

            canvas.PointerMoved -= cursor.OnPointerMoved;
            CursorEdge.Remove();
            graph.Nodes.Remove(cursor.Node);
            CursorEdge = null;
            cursor = null;

            if (node != null && origin != node.Node)
            {
                var newEdge = new Edge(origin, node.Node);
                graph.Edges.Add(newEdge);
                new UIEdge(newEdge, this, graph);
                LayoutHelpers.RouteAndLabelEdges(graph, layoutSettings, new[] { newEdge });
            }
        }

        public Canvas Canvas { get => canvas; }


        public Controller Controller { get; set; }
        public InitialLayout layout;
        public FastIncrementalLayoutSettings layoutSettings;

        public void DrawNodes()
        {
            foreach (var node in Controller.GeometryGraph.Nodes)
            {
                var uiNode = new UINode { Node = node, Controller = Controller, ParentPage = this };
                Controller.UINodes.Add(uiNode);
                canvas.Children.Add(uiNode);
            }
        }

        public GraphViewer()
        {
            InitializeComponent();
            Controller = new Controller(this);
            edgesSurface.Controller = Controller;
            canvas.Tapped += OnResetFocus;
            canvas.Tapped += OnTapped;
            mainGrid.SizeChanged += MainGrid_SizeChanged;
            bottomMenu.Controller = Controller;
        }

        async Task GetImagePath()
        {
            await Controller.WriteGraph();
            ImagePath = Controller.ImagePath;
        }

        private double offsetX;
        public double OffsetX
        {
            get => offsetX;
            set
            {
                offsetX = value;
                OnPropertyChanged("OffsetX");
            }
        }

        private double offsetY;
        public double OffsetY
        {
            get => offsetY;
            set
            {
                offsetY = value;
                OnPropertyChanged("OffsetY");
            }
        }

        private double canvasWidth = 500;
        public double CanvasWidth
        {
            get => canvasWidth;
            set
            {
                canvasWidth = value;
                OnPropertyChanged("CanvasWidth");
            }
        }
        private double canvasHeight = 500;
        public double CanvasHeight
        {
            get => canvasHeight;
            set
            {
                canvasHeight = value;
                OnPropertyChanged("CanvasHeight");
            }
        }
        public void CanvasManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            OffsetX += e.Delta.Translation.X;
            OffsetY += e.Delta.Translation.Y;
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (mainGrid.ActualHeight > 0 && mainGrid.ActualWidth > 0)
            {
                CanvasWidth = mainGrid.ActualWidth;
                CanvasHeight = mainGrid.ActualHeight;
            }
        }

        public void RemoveEdge(object sender, RoutedEventArgs e)
        {
            SelectedEdge.Remove();
            SelectedEdge = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
