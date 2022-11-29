namespace CsML.Graph;

/// <summary>A graph with edges that have direction and weight.</summary>
public class DirectedWeightedGraph<TNode>
    where TNode : notnull
{
    ///<summary>Graph nodes.</summary>
    private List<TNode> nodes;

    /// <summary>
    /// Adjacency matrix where rows represent sources (from) and columns
    /// represent destinations (to) nodes. A node with 0 weight
    /// represents no link. A number other than 0 indicates a link and the
    /// number represents the weight of the edge.
    /// </summary>
    private List<List<double>> adjacencyMatrix;

    /// <summary>Create a new empty graph.</summary>
    public DirectedWeightedGraph()
    {
        nodes = new List<TNode>();
        adjacencyMatrix = new List<List<double>>();
    }

    /// <summary>Add a node and expand the adjacency matrix.</summary>
    public void AddNode(TNode node)
    {
        nodes.Add(node);
        foreach (List<double> row in adjacencyMatrix)
            row.Add(0.0);
        adjacencyMatrix.Add(
            Enumerable
                .Repeat(0.0, adjacencyMatrix[0].Count)
                .ToList()
        );
    }

}