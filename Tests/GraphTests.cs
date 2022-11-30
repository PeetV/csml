using Xunit;

namespace CsML.Tests.Graph;

public class DirectedWeightedGraph
{
    [Fact]
    public void DirectedWeightedGraph_Add()
    {
        var graph = new CsML.Graph.DirectedWeightedGraph<string>();
        Assert.Equal(0, graph.nodes.Count);
        graph.AddNode("a");
        Assert.Equal(1, graph.nodes.Count);
        Assert.Equal(1, graph.adjacencyMatrix.Count);
        Assert.Equal(99, graph.adjacencyMatrix[0].Count);
    }
}