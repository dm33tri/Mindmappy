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
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Diagnostics;
using System.ComponentModel;

using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.DebugHelpers;
using Microsoft.Msagl.DebugHelpers.Persistence;
using Microsoft.Msagl.Routing;
using Microsoft.Msagl.Routing.Rectilinear;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Initial;
using MSAGLNode = Microsoft.Msagl.Core.Layout.Node;
using XAMLRectange = Windows.UI.Xaml.Shapes.Rectangle;
using XAMLPath = Windows.UI.Xaml.Shapes.Path;
using MSAGLPoint = Microsoft.Msagl.Core.Geometry.Point;
using MSAGLRectangle = Microsoft.Msagl.Core.Geometry.Rectangle;
using LineSegment = Microsoft.Msagl.Core.Geometry.Curves.LineSegment;
using Ellipse = Microsoft.Msagl.Core.Geometry.Curves.Ellipse;
using System.Threading;
using System.Threading.Tasks;
using Size = Microsoft.Msagl.Core.DataStructures.Size;
using Microsoft.Msagl.Layout.MDS;
using Microsoft.Msagl.Miscellaneous;

namespace Mindmappy.Shared
{
    public sealed partial class UIEdge : Page, INotifyPropertyChanged
    {
        private string GetPathData(ICurve curve)
        {
            if (curve is Curve)
            {
                List<string> result = new List<string>();

                for (int i = 0; i < (curve as Curve).Segments.Count; ++i)
                {
                    ICurve segment = (curve as Curve).Segments[i];
                    if (segment is LineSegment)
                    {
                        var s = segment as LineSegment;
                        string data = string.Format(
                            "M{0},{1}L{2},{3}",
                            Math.Round(s[0].X),
                            Math.Round(s[0].Y),
                            Math.Round(s[1].X),
                            Math.Round(s[1].Y)
                        );
                        result.Add(data);
                    }
                    else if (segment is CubicBezierSegment)
                    {
                        var s = segment as CubicBezierSegment;
                        string data = string.Format(
                            "M{0},{1}C{2},{3} {4},{5} {6},{7}",
                            Math.Round(s.B(0).X),
                            Math.Round(s.B(0).Y),
                            Math.Round(s.B(1).X),
                            Math.Round(s.B(1).Y),
                            Math.Round(s.B(2).X),
                            Math.Round(s.B(2).Y),
                            Math.Round(s.B(3).X),
                            Math.Round(s.B(3).Y)
                        );
                        result.Add(data);
                    }
                }

                return string.Join("", result);
            }
            else if (curve is LineSegment)
            {
                var s = curve as LineSegment;
                return string.Format(
                    "M{0},{1}L{2},{3}",
                    Math.Round(s[0].X),
                    Math.Round(s[0].Y),
                    Math.Round(s[1].X),
                    Math.Round(s[1].Y)
                );
            }

            return "";
        }
        
        private string pathData;
        public string PathData
        {
            get => pathData;
            set
            {
                pathData = value;
                OnPropertyChanged("PathData");
            }
        }
        public Edge Edge { get; set; }
        public new Canvas Parent { get; set; }

        private bool selected;
        public bool Selected {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged("StrokeColor");
                OnPropertyChanged("StrokeThickness");
            }
        }
        
        public string StrokeColor { get => Selected ? "Blue" : "Black"; }
        public int StrokeThickness { get => Selected ? 3 : 1; }

        public UIEdge(Edge edge, Canvas parent)
        {
            Parent = parent;
            Edge = edge;
            PathData = GetPathData(edge.Curve);

            InitializeComponent();

            Edge.BeforeLayoutChangeEvent += (sender, e) =>
            {
                if (e.DataAfterChange is ICurve)
                {
                    PathData = GetPathData(e.DataAfterChange as ICurve);
                }
            };

            PointerPressed += (sender, e) =>
            {
                Selected = !Selected;
            };

            Parent.Children.Add(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
