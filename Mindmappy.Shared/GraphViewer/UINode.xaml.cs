using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Layout.Initial;
using MSAGLPoint = Microsoft.Msagl.Core.Geometry.Point;

namespace Mindmappy.Shared
{
    public sealed partial class UINode : Page, INotifyPropertyChanged
    {
        static SolidColorBrush defaultBrush = new SolidColorBrush { Color = Colors.Black };
        static SolidColorBrush highlightBrush = new SolidColorBrush { Color = Color.FromArgb(255, 0, 120, 212) };

        Node node;
        public Node Node
        {
            get => node;
            set
            {
                node = value;
                OnPropertyChanged("Node");
            }
        }

        Controller controller;
        public Controller Controller
        {
            get => controller;
            set
            {
                if (controller != null)
                {
                    controller.Unfocus -= Unfocus;
                }
                controller = value;
                controller.Unfocus += Unfocus;
            }
        }

        string label;
        public string Label {
            get => label;
            set
            {
                label = value;
                OnPropertyChanged("Label");
            }
        }

        bool active;
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

        public GraphViewer ParentPage { get; set; }
        public double Top { 
            get => Node?.BoundingBox.Bottom ?? 0; 
            set
            {
                if (Node != null)
                {
                    MSAGLPoint diff = new MSAGLPoint(0, value - Node.BoundingBox.Bottom);
                    Node.Center += diff;
                    OnPropertyChanged("Node");
                }
            }
        }
        public double Left
        {
            get => Node?.BoundingBox.Left ?? 0;
            set
            {
                if (Node != null)
                {
                    MSAGLPoint diff = new MSAGLPoint(value - Node.BoundingBox.Left, 0);
                    Node.Center += diff;
                    OnPropertyChanged("Node");
                }
            }
        }
        public double NodeWidth { get => Node?.BoundingBox.Width ?? 0; }
        public double NodeHeight { get => Node?.BoundingBox.Height ?? 0; }
        public Brush Stroke { get => active ? highlightBrush : defaultBrush; }

        public UINode()
        {
            InitializeComponent();
            ManipulationDelta += UINode_ManipulationDelta;
            Tapped += UINode_Tapped;
            resizePoint.ManipulationDelta += ResizePoint_ManipulationDelta;
            textBox.TextChanging += TextBox_TextChanging;
            removeButton.Click += RemoveButton_Click;
        }

        void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            Label = sender.Text;
        }

        void ResizePoint_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            var delta = new MSAGLPoint(e.Delta.Translation.X, e.Delta.Translation.Y);
            var box = Node.BoundingBox;
            if (box.Width + delta.X >= 150)
            {
                box.Right += delta.X;
            }
            if (box.Height + delta.Y >= 60)
            {
                box.Top += delta.Y;
            }
            Node.BoundingBox = box;
            OnPropertyChanged("Node");
            Relayout();
        }

        void UINode_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            var delta = new MSAGLPoint(e.Delta.Translation.X, e.Delta.Translation.Y);
            Node.Center += delta;
            OnPropertyChanged("Node");
            OnPropertyChanged("Pos");
            Relayout();
        }

        void UINode_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            Controller.UnfocusAll();
            if (ParentPage.CursorEdge != null)
            {
                ParentPage.AttachEdge(this);
            }
            else
            {
                Focus();
            }
        }

        void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            Controller.GeometryGraph.Nodes.Remove(Node);
            Relayout();
            ParentPage.Canvas.Children.Remove(this);
        }

        void AddEdgeButton_Click(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            ParentPage.AddCursorNode(this);
        }

        public void Relayout()
        {
            var layout = new Relayout(
                Controller.GeometryGraph,
                new Node[] { Node },
                new Node[] { },
                (cluster) => Controller.LayoutSettings
            );
            layout.Run();
        }

        void Focus()
        {
            textBox.IsReadOnly = false;
            Active = true;
            overlay.Visibility = Visibility.Collapsed;
            textBox.IsTabStop = true;
            textBox.Focus(FocusState.Programmatic);
            textBox.IsTabStop = false;
        }

        void Unfocus()
        {
            Active = false;
            overlay.Visibility = Visibility.Visible;
            textBox.IsReadOnly = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
