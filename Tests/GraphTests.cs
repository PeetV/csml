using Xunit;
using CsML.Graph;

namespace CsML.Tests.Graph;

public class Graph
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
    public static Graph<string> GraphStringUndirected()
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
    public void Graph_AddNode_single()
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
    public void Graph_AddNode_multiple()
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
    public void Graph_Neighbours()
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
    public void Graph_PathCost_GraphIntUndirected_SameCost()
    {
        var graph = GraphIntUndirected();
        int[] path = { 0, 1, 2, 4, 2, 1, 0, 3 };
        double cost = graph.PathCost(path);
        Assert.Equal(7, cost);
    }

    [Fact]
    public void Graph_PathCost_GraphIntUndirected_DifferentCost()
    {
        var graph = GraphIntUndirected();
        graph.UpdateEdge(2, 4, 10);
        int[] path = { 0, 1, 2, 4, 2, 1, 0, 3 };
        double cost = graph.PathCost(path);
        Assert.Equal(16, cost);
    }

    [Fact]
    public void Graph_PathCost_GraphIntDAG_SameCost()
    {
        var graph = GraphIntDAG();
        var path = new int[] { 0, 1, 2, 4 };
        double cost = graph.PathCost(path);
        Assert.Equal(3, cost);
    }

    [Fact]
    public void Graph_UpdateEdge_ByIndex()
    {
        var graph = new Graph<string>();
        Assert.Empty(graph.nodes);
        graph.AddNodes(new string[] { "a", "b", "c" });
        graph.UpdateEdge(0, 1, 1.0);
        Assert.Equal(1.0, graph.matrix[0][1]);
    }

    [Fact]
    public void Graph_WalkDepthFirst_GraphStringDAG()
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
    public void Graph_WalkDepthFirst_GraphStringUndirected()
    {
        var graph = GraphStringUndirected();
        string[] walk = graph.WalkDepthFirst("a");
        string[] expected = { "a", "b", "e", "f", "g", "d", "c" };
        Assert.True(expected.SequenceEqual(walk));
    }

    [Fact]
    public void Graph_WalkDepthFirst_GraphIntDAG()
    {
        var graph = GraphIntDAG();
        string[] walk = graph.WalkDepthFirst("1");
        string[] expected = { "1", "2", "3", "5", "3", "2", "1", "4" };
        Assert.True(expected.SequenceEqual(walk));
        walk = graph.WalkDepthFirst("1", false);
        expected = new string[] { "1", "2", "3", "5", "4" };
    }

    [Fact]
    public void Graph_WalkDepthFirst_GraphIntUndirected()
    {
        var graph = GraphIntUndirected();
        string[] walk = graph.WalkDepthFirst("1");
        string[] expected = { "1", "2", "3", "5", "3", "2", "1", "4" };
        Assert.True(expected.SequenceEqual(walk));
        walk = graph.WalkDepthFirst("1", false);
        expected = new string[] { "1", "2", "3", "5", "4" };
    }

}