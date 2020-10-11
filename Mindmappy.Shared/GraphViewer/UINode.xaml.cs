using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Layout.Initial;
using MSAGLPoint = Microsoft.Msagl.Core.Geometry.Point;
using MSAGLNode = Microsoft.Msagl.Core.Layout.Node;
using Node = Microsoft.Msagl.Drawing.Node;

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
                OnPropertyChanged("GeometryNode");
            }
        }

        public MSAGLNode GeometryNode
        {
            get => node?.GeometryNode;
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

        public string Label {
            get => node?.LabelText;
            set
            {
                node.LabelText = value;
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
        public Brush Stroke { get => active ? highlightBrush : defaultBrush; }
        public GraphViewer ParentPage { get; set; }
        public double Top { 
            get => Node?.BoundingBox.Bottom ?? 0; 
            set
            {
                if (Node != null)
                {
                    MSAGLPoint diff = new MSAGLPoint(0, value - Node.BoundingBox.Bottom);
                    GeometryNode.Center += diff;
                    OnPropertyChanged("GeometryNode");
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
                    GeometryNode.Center += diff;
                    OnPropertyChanged("GeometryNode");
                }
            }
        }
        public double NodeWidth { get => Node?.BoundingBox.Width ?? 0; }
        public double NodeHeight { get => Node?.BoundingBox.Height ?? 0; }

        public UINode()
        {
            InitializeComponent();
            ManipulationDelta += UINode_ManipulationDelta;
            Tapped += UINode_Tapped;
            resizePoint.ManipulationDelta += ResizePoint_ManipulationDelta;
            textBox.TextChanging += TextBox_TextChanging;
            removeButton.Click += RemoveButton_Click;
            addEdgeButton.Tapped += AddEdgeButton_Tapped;
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
            GeometryNode.BoundingBox = box;
            OnPropertyChanged("GeometryNode");
            Relayout();
        }

        void UINode_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            var delta = new MSAGLPoint(e.Delta.Translation.X, e.Delta.Translation.Y);
            GeometryNode.Center += delta;
            OnPropertyChanged("GeometryNode");
            OnPropertyChanged("Pos");
            Relayout();
        }

        void UINode_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            Controller.UnfocusAll();
            if (Controller.EdgeFromNode != null)
            {
                Controller.CreateEdge(Controller.EdgeFromNode.Node.Id, Node.Id);
                Controller.EdgeFromNode = null;
            }
            else
            {
                Focus();
            }
        }

        void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            Controller.GeometryGraph.Nodes.Remove(GeometryNode);
            Relayout();
            ParentPage.Canvas.Children.Remove(this);
        }

        void AddEdgeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            Controller.EdgeFromNode = this;
        }

        public void Relayout()
        {
            var layout = new Relayout(
                Controller.GeometryGraph,
                new MSAGLNode[] { GeometryNode },
                new MSAGLNode[] { },
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
