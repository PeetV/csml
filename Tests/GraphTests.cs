using Xunit;

namespace CsML.Tests.Graph;

public class DirectedWeightedGraph
{
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
    public void DirectedWeightedGraph_AddEdge()
    {
        var graph = new CsML.Graph.DirectedWeightedGraph<string>();
        Assert.Empty(graph.nodes);
        graph.AddNode(new string[] { "a", "b", "c" });
        graph.UpdateEdge(0, 1, 1.0);
        Assert.Equal(1.0, graph.matrix[0][1]);
    }
}