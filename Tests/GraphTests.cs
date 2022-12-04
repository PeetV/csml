using Xunit;
using CsML.Graph;

namespace CsML.Tests.Graph;

public class Graph
{

    public class GraphTNode
    {

        // Assume direction is in increasing alphabetical order:
        //      a
        //    /  \
        //   b    c - d
        //    \  /     \
        //     e - f -  g
        public static Graph<string> GraphStringDAG()
        {
            var graph = new Graph<string>();
            graph.AddNodes(new string[] { "a", "b", "c", "d", "e", "f", "g" });
            var edges = new (string, string)[] {
                ("a", "b"), ("a", "c"), ("b", "e"), ("c", "e"), ("c", "d"),
                ("d", "g"), ("e", "f"), ("f", "g")
             };
            graph.UpdateEdges(edges);
            return graph;
        }

        //      a
        //    /  \
        //   b    c - d
        //    \  /     \
        //     e - f -  g
        public static Graph<string> GraphStringUndirectedEqualWeights()
        {
            var graph = new Graph<string>();
            graph.AddNodes(new string[] { "a", "b", "c", "d", "e", "f", "g" });
            var edges = new (string, string)[] {
                ("a", "b"), ("a", "c"), ("b", "e"), ("c", "e"), ("c", "d"),
                ("d", "g"), ("e", "f"), ("f", "g")
             };
            graph.UpdateEdges(edges, undirected: true);
            return graph;
        }

        public static Graph<string> GraphStringUndirectedWeighted()
        {
            var graph = new Graph<string>();
            graph.AddNodes(new string[] { "A", "r11", "r12", "r13", "r21",
                                          "r22", "r31", "r32", "B" });
            graph.UpdateEdge("A", "r11", weight: 3, undirected: true);
            graph.UpdateEdge("A", "r21", weight: 7, undirected: true);
            graph.UpdateEdge("A", "r31", weight: 5, undirected: true);
            graph.UpdateEdge("r11", "r12", weight: 7, undirected: true);
            graph.UpdateEdge("r11", "r21", weight: 1, undirected: true);
            graph.UpdateEdge("r12", "r21", weight: 2, undirected: true);
            graph.UpdateEdge("r12", "r22", weight: 2, undirected: true);
            graph.UpdateEdge("r12", "r13", weight: 1, undirected: true);
            graph.UpdateEdge("r13", "B", weight: 5, undirected: true);
            graph.UpdateEdge("r13", "r22", weight: 3, undirected: true);
            graph.UpdateEdge("r21", "r31", weight: 3, undirected: true);
            graph.UpdateEdge("r21", "r32", weight: 3, undirected: true);
            graph.UpdateEdge("r21", "r22", weight: 1, undirected: true);
            graph.UpdateEdge("r22", "B", weight: 2, undirected: true);
            graph.UpdateEdge("r22", "r32", weight: 3, undirected: true);
            graph.UpdateEdge("r31", "r32", weight: 2, undirected: true);
            graph.UpdateEdge("r32", "B", weight: 4, undirected: true);
            return graph;
        }

        // Assume direction is in increasing numerical order:
        //      1  -  4
        //      |  \
        //      2 - 3
        //           \
        //            5
        public static Graph<string> GraphIntDAG()
        {
            var graph = new Graph<string>();
            graph.AddNodes(new string[] { "1", "2", "3", "4", "5" });
            var edges = new (string, string)[] {
                ("1", "2"), ("1", "3"), ("1", "4"),  ("2", "3"), ("3", "5")
            };
            graph.UpdateEdges(edges);
            return graph;
        }

        //      1  -  4
        //      |  \
        //      2 - 3
        //           \
        //            5
        public static Graph<string> GraphIntUndirected()
        {
            var graph = new Graph<string>();
            graph.AddNodes(new string[] { "1", "2", "3", "4", "5" });
            var edges = new (string, string)[] {
                ("1", "2"), ("1", "3"), ("1", "4"),  ("2", "3"), ("3", "5")
             };
            graph.UpdateEdges(edges, undirected: true);
            return graph;
        }

        [Fact]
        public void AddNode_single()
        {
            var graph = new Graph<string>();
            Assert.Empty(graph.nodes);
            graph.AddNode("a");
            Assert.Single(graph.nodes);
            Assert.Single(graph.matrix);
            Assert.Single(graph.matrix[0]);
            graph.AddNode("b");
            Assert.Equal(2, graph.nodes.Count);
            Assert.Equal(2, graph.matrix.Count);
            Assert.Equal(2, graph.matrix[0].Count);
            Assert.True(new double[] { 0, 0 }.SequenceEqual(
                    graph.matrix[0]));
        }

        [Fact]
        public void AddNode_multiple()
        {
            var graph = new Graph<string>();
            Assert.Empty(graph.nodes);
            graph.AddNodes(new string[] { "a", "b" });
            Assert.Equal(2, graph.nodes.Count);
            Assert.Equal(2, graph.matrix.Count);
            Assert.Equal(2, graph.matrix[0].Count);
            Assert.True(new double[] { 0, 0 }.SequenceEqual(graph.matrix[0]));
        }

        [Fact]
        public void Neighbours()
        {
            var graph = GraphStringDAG();
            Assert.Equal(7, graph.nodes.Count);
            Assert.Equal(7, graph.matrix.Count);
            Assert.Equal(7, graph.matrix[0].Count);
            var result = graph.Neighbours("a");
            var expected = new string[] { "b", "c" };
            Assert.True(expected.SequenceEqual(result));
            result = graph.Neighbours("c");
            expected = new string[] { "d", "e" };
            Assert.True(expected.SequenceEqual(result));
            result = graph.Neighbours("d");
            expected = new string[] { "g" };
            Assert.True(expected.SequenceEqual(result));
            result = graph.Neighbours("g");
            expected = new string[] { };
            Assert.True(expected.SequenceEqual(result));
        }

        [Fact]
        public void PathCost_Index_GraphIntUndirected_SameCost()
        {
            var graph = GraphIntUndirected();
            int[] path = { 0, 1, 2, 4, 2, 1, 0, 3 };
            double cost = graph.PathCost(path);
            Assert.Equal(7, cost);
        }

        [Fact]
        public void PathCost_Node_GraphIntUndirected_SameCost()
        {
            var graph = GraphIntUndirected();
            string[] path = { "1", "2", "3", "5", "3", "2", "1", "4" };
            double cost = graph.PathCost(path);
            Assert.Equal(7, cost);
        }

        [Fact]
        public void PathCost_Index_GraphIntUndirected_DifferentCost()
        {
            var graph = GraphIntUndirected();
            graph.UpdateEdge(2, 4, 10);
            int[] path = { 0, 1, 2, 4, 2, 1, 0, 3 };
            double cost = graph.PathCost(path);
            Assert.Equal(16, cost);
        }

        [Fact]
        public void PathCost_Node_GraphIntUndirected_DifferentCost()
        {
            var graph = GraphIntUndirected();
            graph.UpdateEdge(2, 4, 10);
            string[] path = { "1", "2", "3", "5", "3", "2", "1", "4" };
            double cost = graph.PathCost(path);
            Assert.Equal(16, cost);
        }

        [Fact]
        public void PathCost_Index_GraphIntDAG_SameCost()
        {
            var graph = GraphIntDAG();
            int[] path = { 0, 1, 2, 4 };
            double cost = graph.PathCost(path);
            Assert.Equal(3, cost);
        }

        [Fact]
        public void UpdateEdge_ByIndex()
        {
            var graph = new Graph<string>();
            Assert.Empty(graph.nodes);
            graph.AddNodes(new string[] { "a", "b", "c" });
            graph.UpdateEdge(0, 1, 1.0);
            Assert.Equal(1.0, graph.matrix[0][1]);
        }

        [Fact]
        public void ShortestPathDijkstra_Indexed_StringUndirectedWeighted()
        {
            var graph = GraphStringUndirectedWeighted();
            (double result, int[] path) = graph.ShortestPathDijkstra(0, 8);
            int[] expected = new int[] { 0, 1, 4, 5, 8 };
            Assert.Equal(7, result);
            Assert.True(path.SequenceEqual(expected));
            Assert.Equal(7, graph.PathCost(expected));
            (result, path) = graph.ShortestPathDijkstra(2, 6);
            Assert.Equal(5, result);
            expected = new int[] { 2, 4, 6 };
            Assert.True(path.SequenceEqual(expected));
        }

        [Fact]
        public void ShortestPathDijkstra_Nodes_StringUndirectedWeighted()
        {
            var graph = GraphStringUndirectedWeighted();
            (double result, string[] path) = graph
                        .ShortestPathDijkstra("A", "B");
            string[] expected = new string[] { "A", "r11", "r21", "r22", "B" };
            Assert.Equal(7, result);
            Assert.True(path.SequenceEqual(expected));
            (result, path) = graph.ShortestPathDijkstra("r12", "r31");
            Assert.Equal(5, result);
            expected = new string[] { "r12", "r21", "r31" };
            Assert.True(path.SequenceEqual(expected));
        }

        [Fact]
        public void ShortestPathDijkstra_Indexed_StringDAG()
        {
            var graph = GraphStringDAG();
            (double result, int[] path) = graph.ShortestPathDijkstra(0, 6);
            int[] expected = { 0, 2, 3, 6 };
            Assert.Equal(3, result);
            Assert.True(path.SequenceEqual(expected));
        }

        [Fact]
        public void ShortestPathDijkstra_Nodes_StringDAG()
        {
            var graph = GraphStringDAG();
            (double result, string[] path) = graph
                        .ShortestPathDijkstra("a", "g");
            string[] expected = { "a", "c", "d", "g" };
            Assert.Equal(3, result);
            Assert.True(path.SequenceEqual(expected));
        }

        [Fact]
        public void ShortestPathDijkstra_Nodes_NotConnected()
        {
            var graph = GraphStringDAG();
            (double result, string[] path) = graph
                        .ShortestPathDijkstra("g", "a");
            string[] expected = { };
            Assert.Equal(0, result);
            Assert.True(path.SequenceEqual(expected));
        }

        [Fact]
        public void WalkDepthFirst_GraphStringDAG()
        {
            var graph = GraphStringDAG();
            string[] walk = graph.WalkDepthFirst("a");
            string[] expected = { "a", "b", "e", "f", "g", "f", "e", "b", "a",
                              "c", "d" };
            Assert.True(expected.SequenceEqual(walk));
            walk = graph.WalkDepthFirst("a", false);
            expected = new string[] { "a", "b", "e", "f", "g", "c", "d" };
            Assert.True(expected.SequenceEqual(walk));
        }

        [Fact]
        public void WalkDepthFirst_GraphStringUndirected()
        {
            var graph = GraphStringUndirectedEqualWeights();
            string[] walk = graph.WalkDepthFirst("a");
            string[] expected = { "a", "b", "e", "f", "g", "d", "c" };
            Assert.True(expected.SequenceEqual(walk));
        }

        [Fact]
        public void WalkDepthFirst_GraphIntDAG()
        {
            var graph = GraphIntDAG();
            string[] walk = graph.WalkDepthFirst("1");
            string[] expected = { "1", "2", "3", "5", "3", "2", "1", "4" };
            Assert.True(expected.SequenceEqual(walk));
            walk = graph.WalkDepthFirst("1", false);
            expected = new string[] { "1", "2", "3", "5", "4" };
        }

        [Fact]
        public void WalkDepthFirst_GraphIntUndirected()
        {
            var graph = GraphIntUndirected();
            string[] walk = graph.WalkDepthFirst("1");
            string[] expected = { "1", "2", "3", "5", "3", "2", "1", "4" };
            Assert.True(expected.SequenceEqual(walk));
            walk = graph.WalkDepthFirst("1", false);
            expected = new string[] { "1", "2", "3", "5", "4" };
        }

    }
}