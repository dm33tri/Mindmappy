using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using MSAGLPoint = Microsoft.Msagl.Core.Geometry.Point;
using MSAGLLineSegment = Microsoft.Msagl.Core.Geometry.Curves.LineSegment;

namespace Mindmappy.Shared
{
    public sealed partial class UIEdge : Page, INotifyPropertyChanged
    {
        private bool HitDetect(MSAGLPoint point)
        {
            ICurve curve = Edge.Curve;
            MSAGLPoint closest = Curve.ClosestPoint(curve, point);
            double length = (point - closest).Length;
            return length < 8;
        }

        private string GetPathData(ICurve curve)
        {
            if (curve is Curve)
            {
                List<string> result = new List<string>();

                for (int i = 0; i < (curve as Curve).Segments.Count; ++i)
                {
                    ICurve segment = (curve as Curve).Segments[i];
                    if (segment is MSAGLLineSegment)
                    {
                        var s = segment as MSAGLLineSegment;
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
            else if (curve is MSAGLLineSegment)
            {
                var s = curve as MSAGLLineSegment;
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

        private GeometryGraph Graph { get; set; }
        public Edge Edge { get; set; }
        public GraphViewer ParentPage { get; set; }
        public bool Selected { 
            get => ParentPage.SelectedEdge == this; 
            set {
                if (value && ParentPage.SelectedEdge == null)
                {
                    ParentPage.SelectedEdge = this;
                }
                else if (ParentPage.SelectedEdge == this)
                {
                    ParentPage.SelectedEdge = null;
                }
                OnPropertyChanged("StrokeColor");
                OnPropertyChanged("StrokeThickness");
            }
        }        
        public string StrokeColor { get => Selected ? "Blue" : "Black"; }
        public int StrokeThickness { get => Selected ? 3 : 2; }

        public UIEdge(Edge edge, GraphViewer parent, GeometryGraph graph, bool temp = false)
        {
            ParentPage = parent;
            Edge = edge;
            PathData = GetPathData(edge.Curve);
            Graph = graph;
            Edge.UserData = this;

            InitializeComponent();

            Edge.BeforeLayoutChangeEvent += (sender, e) =>
            {
                if (e.DataAfterChange is ICurve)
                {
                    PathData = GetPathData(e.DataAfterChange as ICurve);
                }
            };

            if (!temp)
            {
                ParentPage.Unfocus += ParentPage_Unfocus;
                ParentPage.Canvas.Tapped += OnTapped;
            }
            ParentPage.Canvas.Children.Add(this);
        }

        private void ParentPage_Unfocus()
        {
            Selected = false;
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var p = e.GetPosition(ParentPage.Canvas);
            MSAGLPoint point = new MSAGLPoint(p.X, p.Y);
            if (HitDetect(point))
            {
                Selected = true;
            }
            else
            {
                Selected = false;
            }
        }

        public void Remove()
        {
            ParentPage.Canvas.Tapped -= OnTapped;
            var e = Edge;
            Edge = null;
            ParentPage.Canvas.Children.Remove(this);
            Graph.Edges.Remove(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
