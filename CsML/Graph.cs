namespace CsML.Graph;

/// <summary>
/// A graph with edges that have direction and weight and T as node type.
/// </summary>
public class DirectedWeightedGraph<T>
    where T : notnull
{
    private List<T> nodes;

    // Columns represent destinations (to) and rows sources (from). 0 in a cell
    // represents no link. A number other than 0 indicates a link and the
    // number represents the weight of the edge.
    private List<List<int>> adjacencyMatrix;

    /// <summary>Create a new empty graph.</summary>
    public DirectedWeightedGraph()
    {
        nodes = new List<T>();
        adjacencyMatrix = new List<List<int>>();
    }

}