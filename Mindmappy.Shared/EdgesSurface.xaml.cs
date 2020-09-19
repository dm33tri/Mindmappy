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

        public EdgesSurface()
        {
            InitializeComponent();
            AnimationLoop();
        }

        SKPoint P(Point p)
        {
            return new SKPoint((float)p.X, (float)p.Y);
        }

        void PaintEdges(SKCanvas canvas)
        {
            var graph = Controller.Graph;

            foreach (Edge edge in graph.Edges)
            {
                var curve = edge.Curve;

                if (curve is Curve)
                {
                    for (int i = 0; i < (curve as Curve).Segments.Count; ++i)
                    {
                        ICurve segment = (curve as Curve).Segments[i];
                        if (segment is LineSegment)
                        {
                            var s = segment as LineSegment;
                            canvas.DrawLine(P(s[0]), P(s[1]), linePaint);
                        }
                        else if (segment is CubicBezierSegment)
                        {
                            var s = segment as CubicBezierSegment;
                            using (SKPath path = new SKPath())
                            {
                                path.MoveTo(P(s.B(0)));
                                path.CubicTo(P(s.B(1)), P(s.B(2)), P(s.B(3)));
                                canvas.DrawPath(path, linePaint);
                            }
                        }
                    }
                }
                else if (curve is LineSegment)
                {
                    var s = curve as LineSegment;
                    canvas.DrawLine(P(s[0]), P(s[1]), linePaint);
                }
            }
        }

        void PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Scale(2);
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
