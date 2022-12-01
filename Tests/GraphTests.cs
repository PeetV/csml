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
        Assert.Single(graph.adjacencyMatrix);
        Assert.Single(graph.adjacencyMatrix[0]);
        graph.AddNode("b");
        Assert.Equal(2, graph.nodes.Count);
        Assert.Equal(2, graph.adjacencyMatrix.Count);
        Assert.Equal(2, graph.adjacencyMatrix[0].Count);
        Assert.True(new double[] { 0, 0 }.SequenceEqual(
                graph.adjacencyMatrix[0]));
    }

    [Fact]
    public void DirectedWeightedGraph_Add_nodes()
    {
        var graph = new CsML.Graph.DirectedWeightedGraph<string>();
        Assert.Empty(graph.nodes);
        graph.AddNode(new string[] { "a", "b" });
        Assert.Equal(2, graph.nodes.Count);
        Assert.Equal(2, graph.adjacencyMatrix.Count);
        Assert.Equal(2, graph.adjacencyMatrix[0].Count);
        Assert.True(new double[] { 0, 0 }.SequenceEqual(
                graph.adjacencyMatrix[0]));
    }
}