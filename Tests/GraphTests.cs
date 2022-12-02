using Xunit;

namespace CsML.Tests.Graph;

public class DirectedWeightedGraph
{

    // Assume direction is in increasing alphabetical order:
    //      a
    //    /  \
    //   b    c - d
    //    \  /     \
    //     e - f -  g
    public static CsML.Graph.DirectedWeightedGraph<string> TestGraph1()
    {
        var graph = new CsML.Graph.DirectedWeightedGraph<string>();
        graph.AddNode(new string[] { "a", "b", "c", "d", "e", "f", "g" });
        graph.UpdateEdge("a", "b");
        graph.UpdateEdge("a", "c");
        graph.UpdateEdge("b", "e");
        graph.UpdateEdge("c", "e");
        graph.UpdateEdge("c", "d");
        graph.UpdateEdge("d", "g");
        graph.UpdateEdge("e", "f");
        graph.UpdateEdge("f", "g");
        return graph;
    }

    // Assume direction is in increasing numerical order:
    //      1  -  4
    //      |  \
    //      2 - 3
    //           \
    //            5
    public static CsML.Graph.DirectedWeightedGraph<string> TestGraph2()
    {
        var graph = new CsML.Graph.DirectedWeightedGraph<string>();
        graph.AddNode(new string[] { "1", "2", "3", "4", "5" });
        graph.UpdateEdge("1", "2");
        graph.UpdateEdge("1", "3");
        graph.UpdateEdge("1", "4");
        graph.UpdateEdge("2", "3");
        graph.UpdateEdge("3", "5");
        return graph;
    }

    [Fact]
    public void DirectedWeightedGraph_Add_single_node()
    {
        var graph = new CsML.Graph.DirectedWeightedGraph<string>();
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
    public void DirectedWeightedGraph_Add_nodes()
    {
        var graph = new CsML.Graph.DirectedWeightedGraph<string>();
        Assert.Empty(graph.nodes);
        graph.AddNode(new string[] { "a", "b" });
        Assert.Equal(2, graph.nodes.Count);
        Assert.Equal(2, graph.matrix.Count);
        Assert.Equal(2, graph.matrix[0].Count);
        Assert.True(new double[] { 0, 0 }.SequenceEqual(
                graph.matrix[0]));
    }

    [Fact]
    public void DirectedWeightedGraph_Neighbours()
    {
        var graph = TestGraph1();
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
    public void DirectedWeightedGraph_UpdateEdge()
    {
        var graph = new CsML.Graph.DirectedWeightedGraph<string>();
        Assert.Empty(graph.nodes);
        graph.AddNode(new string[] { "a", "b", "c" });
        graph.UpdateEdge(0, 1, 1.0);
        Assert.Equal(1.0, graph.matrix[0][1]);
    }

    [Fact]
    public void DirectedWeightedGraph_WalkDepthFirst_testgraph1()
    {
        var graph = TestGraph1();
        string[] walk = graph.WalkDepthFirst("a");
        string[] expected = { "a", "b", "e", "f", "g", "f", "e", "b", "a",
                              "c", "d" };
        Assert.True(expected.SequenceEqual(walk));
        walk = graph.WalkDepthFirst("a", false);
        expected = new string[] { "a", "b", "e", "f", "g", "c", "d" };
        Assert.True(expected.SequenceEqual(walk));
    }

    [Fact]
    public void DirectedWeightedGraph_WalkDepthFirst_testgraph2()
    {
        var graph = TestGraph2();
        string[] walk = graph.WalkDepthFirst("1");
        string[] expected = { "1", "2", "3", "5", "3", "2", "1", "4" };
        Assert.True(expected.SequenceEqual(walk));
        walk = graph.WalkDepthFirst("1", false);
        expected = new string[] { "1", "2", "3", "5", "4" };
    }

}