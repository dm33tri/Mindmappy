using Windows.UI.Xaml.Controls;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using SkiaSharp;
using System.Threading.Tasks;
using SkiaSharp.Views.UWP;

namespace Mindmappy.Shared
{
    public sealed partial class EdgesSurface : Page
    {
        public Controller Controller { get; set; }
        SKPaint linePaint = new SKPaint
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3
        };

        SKPaint arrowPaint = new SKPaint
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Fill
        };


        SKPaint selectedLinePaint = new SKPaint
        {
            Color = SKColors.Blue,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3
        };

        SKPaint selectedArrowPaint = new SKPaint
        {
            Color = SKColors.Blue,
            Style = SKPaintStyle.Fill
        };

        public EdgesSurface()
        {
            InitializeComponent();
            AnimationLoop();
            this.Tapped += EdgesSurface_Tapped;
        }

        private bool HitDetect(Point point, Edge edge)
        {
            ICurve curve = edge.Curve;
            Point closest = Curve.ClosestPoint(curve, point);
            double length = (point - closest).Length;
            return length < 8;
        }

        private void EdgesSurface_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var p = e.GetPosition(this);
            Point point = new Point(p.X, p.Y);
            var graph = Controller?.Graph;
            if (graph == null)
            {
                return;
            }
            foreach (var edge in graph.Edges)
            {
                if (HitDetect(point, edge.GeometryEdge))
                {
                    Controller.SelectedEdge = edge;
                }
            }
        }

        SKPoint P(Point p)
        {
            return new SKPoint((float)p.X, (float)p.Y);
        }

        void DrawArrowhead(SKCanvas canvas, Point from, Point to, SKPaint paint)
        {
            Point dir = to - from;
            Point h = new Point(-dir.Y, dir.X);
            h /= h.Length;
            Point p1 = from + h * 5;
            Point p2 = from - h * 5;

            using (SKPath path = new SKPath { FillType = SKPathFillType.EvenOdd })
            {

                path.MoveTo(P(to));
                path.LineTo(P(p1));
                path.LineTo(P(p2));
                path.LineTo(P(to));
                path.Close();
                canvas.DrawPath(path, paint);
            }
        }

        void PaintEdges(SKCanvas canvas)
        {
            var graph = Controller?.GeometryGraph;
            if (graph == null)
            {
                return;
            }
            foreach (Edge edge in graph.Edges)
            {
                bool selected = Controller.SelectedEdge?.GeometryEdge == edge;
                var curve = edge.Curve;

                if (curve is Curve)
                {
                    for (int i = 0; i < (curve as Curve).Segments.Count; ++i)
                    {
                        ICurve segment = (curve as Curve).Segments[i];
                        if (segment is LineSegment)
                        {
                            var s = segment as LineSegment;
                            canvas.DrawLine(P(s[0]), P(s[1]), selected ? selectedLinePaint : linePaint);
                        }
                        else if (segment is CubicBezierSegment)
                        {
                            var s = segment as CubicBezierSegment;
                            using (SKPath path = new SKPath())
                            {
                                path.MoveTo(P(s.B(0)));
                                path.CubicTo(P(s.B(1)), P(s.B(2)), P(s.B(3)));
                                canvas.DrawPath(path, selected ? selectedLinePaint : linePaint);
                            }
                        }
                    }
                }
                else if (curve is LineSegment)
                {
                    var s = curve as LineSegment;
                    canvas.DrawLine(P(s[0]), P(s[1]), selected ? selectedLinePaint : linePaint);
                }
                if (edge.ArrowheadAtSource)
                {
                    DrawArrowhead(canvas, edge.Curve.Start, edge.EdgeGeometry.SourceArrowhead.TipPosition, selected ? selectedArrowPaint : arrowPaint);
                }
                if (edge.ArrowheadAtTarget)
                {
                    DrawArrowhead(canvas, edge.Curve.End, edge.EdgeGeometry.TargetArrowhead.TipPosition, selected ? selectedArrowPaint : arrowPaint);
                }
            }
        }

        void PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            if (e.Info.Width != this.ActualWidth && this.ActualWidth > 0)
            {
                canvas.Scale((float)(e.Info.Width / this.ActualWidth));
            }
            canvas.Clear(SKColors.White);
            PaintEdges(canvas);
        }

        async Task AnimationLoop()
        {
            while (true)
            {
                skiaView.Invalidate();
                await Task.Delay(System.TimeSpan.FromSeconds(1.0 / 60));
            }
        }
    }
}
