using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ComponentModel;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Input;
using Windows.UI;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Initial;
using Microsoft.Msagl.Core.Geometry.Curves;
using MSAGLPoint = Microsoft.Msagl.Core.Geometry.Point;
using MSAGLRectangle = Microsoft.Msagl.Core.Geometry.Rectangle;
using MSAGLNode = Microsoft.Msagl.Core.Layout.Node;

namespace Mindmappy.Shared
{
    public sealed partial class UINode : Page, INotifyPropertyChanged
    {
        public string Label { get; set; }
        public MSAGLNode Node { get; set; }
        public MainPage ParentPage { get; set; }
        public GeometryGraph Graph { get; set; }
        public FastIncrementalLayoutSettings LayoutSettings { get; set; }

        private Point grabPoint;

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

        private bool moving;
        public bool Moving
        {
            get => moving;
            set
            {
                moving = value;
                OnPropertyChanged("Stroke");
            }
        }

        private SolidColorBrush defaultBrush = new SolidColorBrush { Color = Colors.Gray };
        private SolidColorBrush highlightBrush = new SolidColorBrush { Color = Color.FromArgb(255, 0, 120, 212) };

        public Brush Stroke { get => active || Moving ? highlightBrush : defaultBrush; }
        private bool resizing;

        public UINode(MSAGLNode node, MainPage parent, GeometryGraph graph, FastIncrementalLayoutSettings settings)
        {
            Node = node;
            ParentPage = parent;
            Graph = graph;
            LayoutSettings = settings;

            InitializeComponent();

            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;
            PointerMoved += OnPointerMoved;
            Tapped += OnTapped;
            Holding += OnHolding;
            removeButton.Tapped += OnRemoveClick;
            addEdgeButton.Tapped += OnAddEdgeClick;
            resizePoint.PointerPressed += ResizePoint_OnPointerPressed;
            resizePoint.PointerReleased += ResizePoint_OnPointerReleased;
            ParentPage.Unfocus += Unfocus;
            ParentPage.Canvas.Children.Add(this);
        }

        private void Unfocus()
        {
            Active = false;
            overlay.Visibility = Visibility.Visible;
            textBox.IsReadOnly = true;
        }

        private void OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Started)
            {
                Moving = true;
            }
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (ParentPage.CursorEdge != null)
            {
                ParentPage.AttachEdge(this);
                return;
            }
            else
            {
                Focus();
            }
        }

        private void ResizePoint_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            resizing = false;
            resizePoint.ReleasePointerCapture(e.Pointer);
        }

        private void ResizePoint_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            resizing = true;
            resizePoint.CapturePointer(e.Pointer);
            e.Handled = true;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!resizing && Moving)
            {
                var cursorPos = e.GetCurrentPoint(ParentPage.Canvas).Position;
                var p1 = new MSAGLPoint(cursorPos.X, cursorPos.Y);
                var p2 = new MSAGLPoint(grabPoint.X, grabPoint.Y);
                var p3 = new MSAGLPoint(Node.Width / 2, Node.Height / 2);
                Node.Center = p1 + p3 - p2;
                OnPropertyChanged("Left");
                OnPropertyChanged("Top");
            }
            else if (resizing)
            {
                var cursorPos = e.GetCurrentPoint(ParentPage.Canvas).Position;
                MSAGLPoint point = new MSAGLPoint(cursorPos.X, cursorPos.Y);
                var Box = Node.BoundingBox;
                if (Box.Left + 150 <= point.X)
                {
                    Box.Right = point.X;
                }
                if (Box.Bottom + 60 <= point.Y)
                {
                    Box.Top = point.Y;
                }
                Node.BoundingBox = Box;
                OnPropertyChanged("NodeWidth");
                OnPropertyChanged("NodeHeight");
            }
            LayoutHelpers.RouteAndLabelEdges(Graph, LayoutSettings, Graph.Edges);

        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Moving = false;
            resizing = false;
            ReleasePointerCapture(e.Pointer);
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
#if !__ANDROID__
            if (e.GetCurrentPoint(ParentPage.Canvas).Properties.IsLeftButtonPressed)
            {
                Moving = true;
            }
#endif
            grabPoint = e.GetCurrentPoint(this).Position;
            CapturePointer(e.Pointer);
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
