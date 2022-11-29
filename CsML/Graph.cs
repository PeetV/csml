namespace CsML.Graph;

/// <summary>Interface for weighted graph edges.</summary>
public interface IEdge
{
    /// <summary>The edge weight.</summary>
    double Weight { get; set; }
}

/// <summary>
/// A graph with edges that have direction and weight and T as node type.
/// </summary>
public class DirectedWeightedGraph<TNode, TEdge>
    where TNode : notnull
    where TEdge : IEdge
{
    private List<TNode> nodes;

    // Columns represent destinations (to) and rows sources (from). 0 in a cell
    // represents no link. A number other than 0 indicates a link and the
    // number represents the weight of the edge.
    private List<List<TEdge>> adjacencyMatrix;

    /// <summary>Create a new empty graph.</summary>
    public DirectedWeightedGraph()
    {
        nodes = new List<TNode>();
        adjacencyMatrix = new List<List<TEdge>>();
    }

}