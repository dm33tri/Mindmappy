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

        private void OnResetFocus(object sender, RoutedEventArgs e)
        {
            Controller.UnfocusAll();
        }

        private void OnTapped(object sender, RoutedEventArgs e)
        {
            if (Controller.EdgeFromNode != null)
            {
                Controller.EdgeFromNode = null;
            }
        }

        public Canvas Canvas { get => canvas; }


        public Controller Controller { get; set; }
        public InitialLayout layout;
        public FastIncrementalLayoutSettings layoutSettings;

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
