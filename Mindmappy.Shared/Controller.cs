using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Initial;
using Microsoft.Msagl.Miscellaneous;

namespace Mindmappy.Shared
{
    public class Controller
    {
        public GeometryGraph Graph { get; set; }
        public FastIncrementalLayoutSettings LayoutSettings { get; set; }

        public Edge SelectedEdge { get; set; } = null;
        public Node SelectedNode { get; set; } = null;

        public Controller()
        {
            CreateGraph();
        }

        private void CreateGraph()
        {
            Graph = new GeometryGraph();
            Node[] allNodes = new Node[5];

            for (int i = 0; i < 5; i++)
            {
                Node node = new Node(CurveFactory.CreateRectangle(150, 60, new Point(0, 0)));
                allNodes[i] = node;
                Graph.Nodes.Add(node);
                for (int j = 0; j < i; ++j)
                {
                    Graph.Edges.Add(new Edge(allNodes[j], allNodes[i]));
                }
            }

            LayoutSettings = new FastIncrementalLayoutSettings
            {
                NodeSeparation = 64,
                AvoidOverlaps = true,
                MinConstraintLevel = 1,
            };

            var layout = new InitialLayout(Graph, LayoutSettings);
            layout.SingleComponent = true;
            layout.Run();
            LayoutHelpers.RouteAndLabelEdges(Graph, LayoutSettings, Graph.Edges);
        }
    }
}
