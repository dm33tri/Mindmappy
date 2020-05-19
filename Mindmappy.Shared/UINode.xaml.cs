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
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Initial;
using System.Diagnostics;
using Microsoft.Msagl.Core.Geometry.Curves;

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

        private Point pointPos;
        private bool active;
        public bool Active
        {
            get => active;
            set
            {
                active = value;
                OnPropertyChanged("Active");
            }
        }

        /// <summary>
        ///  Хаки для WASM и Android, потому что события фокуса там работают иначе
        /// </summary>
        bool tapped;
        bool resizing;
        bool resetState;
        
        public UINode(Node node, Canvas parent, GeometryGraph graph, FastIncrementalLayoutSettings settings)
        {
            Node = node;
            Parent = parent;
            Graph = graph;
            LayoutSettings = settings;

            InitializeComponent();

            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;
            PointerMoved += OnPointerMoved;
            Tapped += OnTapped;
            LostFocus += OnLostFocus;
            point.PointerPressed += Point_OnPointerPressed;
            point.PointerReleased += Point_OnPointerReleased;

            GotFocus += UINode_GotFocus;

            Parent.Children.Add(this);
        }

        private void UINode_GotFocus(object sender, RoutedEventArgs e)
        {
            if (resizing)
            {
                Active = true;
                rect.Visibility = Visibility.Collapsed;
            }
            if (tapped)
            {
                Active = true;
                rect.Visibility = Visibility.Collapsed;
                textBox.IsTabStop = true;
                textBox.Focus(FocusState.Programmatic);
                textBox.IsTabStop = false;
            }
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            tapped = true;
            resetState = false;
            Active = true;
            rect.Visibility = Visibility.Collapsed;
            textBox.IsTabStop = true;
            textBox.Focus(FocusState.Programmatic);
            textBox.IsTabStop = false;
        }

        private void Point_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            resizing = false;
            point.ReleasePointerCapture(e.Pointer);
        }

        private void Point_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            resizing = true;
            point.CapturePointer(e.Pointer);
            pointPos = e.GetCurrentPoint(point).Position;
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (resetState && tapped)
            {
                resetState = false;
                tapped = false;
            }
            else
            {
                resetState = true;
            }

                
            Active = false;
            rect.Visibility = Visibility.Visible;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!resizing && PointerCaptures?.Count > 0)
            {
                var cursorPos = e.GetCurrentPoint(Parent).Position;
                var p1 = new MSAGLPoint(cursorPos.X, cursorPos.Y);
                var p2 = new MSAGLPoint(grabPoint.X, grabPoint.Y);
                var p3 = new MSAGLPoint(Node.Width / 2, Node.Height / 2);
                Node.Center = p1 + p3 - p2;
                OnPropertyChanged("Left");
                OnPropertyChanged("Top");
                LayoutHelpers.RouteAndLabelEdges(Graph, LayoutSettings, Graph.Edges);
            }
            else if (resizing)
            {
                var cursorPos = e.GetCurrentPoint(point).Position;
                MSAGLPoint p1 = new MSAGLPoint(cursorPos.X, cursorPos.Y);
                MSAGLPoint p2 = new MSAGLPoint(pointPos.X, pointPos.Y);
                var Box = Node.BoundingBox;
                if (Node.Width + p1.X - p2.X >= 150)
                {
                    Box.Right += p1.X - p2.X;
                }
                if (Node.Height + p1.Y - p2.Y >= 60)
                {
                    Box.Top += p1.Y - p2.Y;
                }
                Node.BoundingBox = Box;
                OnPropertyChanged("RectWidth");
                OnPropertyChanged("RectHeight");
                LayoutHelpers.RouteAndLabelEdges(Graph, LayoutSettings, Graph.Edges);
            }
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            resizing = false;
            ReleasePointerCapture(e.Pointer);
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            CapturePointer(e.Pointer);
            grabPoint = e.GetCurrentPoint(this).Position;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
