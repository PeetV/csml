namespace CsML.Graph;

/// <summary>Error message descriptions.</summary>
public static class ErrorMessages
{
    /// <summary>Error message if from node index is out of bounds.</summary>
    public const string E1 = "From node index out of bounds";

    /// <summary>Error message if to node index is out of bounds.</summary>
    public const string E2 = "To node index out of bounds";
}

/// <summary>A graph with edges that have direction and weight.</summary>
public class DirectedWeightedGraph<TNode>
    where TNode : notnull
{
    ///<summary>Graph nodes.</summary>
    public List<TNode> nodes;

    /// <summary>
    /// Adjacency matrix where rows represent sources (from) and columns
    /// represent destinations (to) nodes. A node with 0 weight
    /// represents no link. A number other than 0 indicates a link and the
    /// number represents the weight of the edge.
    /// </summary>
    public List<List<double>> matrix;

    /// <summary>Create a new empty graph.</summary>
    public DirectedWeightedGraph()
    {
        nodes = new List<TNode>();
        matrix = new List<List<double>>();
    }

    /// <summary>Add a node and expand the adjacency matrix.</summary>
    public void AddNode(TNode node)
    {
        nodes.Add(node);
        if (matrix.Count == 0)
        {
            matrix.Add(new List<double>() { 0.0 });
            return;
        }
        foreach (List<double> row in matrix)
            row.Add(0.0);
        matrix.Add(
            Enumerable
                .Repeat(0.0, matrix[0].Count)
                .ToList()
        );
    }

    /// <summary>Add nodes and expand the adjacency matrix.</summary>
    public void AddNode(TNode[] nodes)
    {
        foreach (TNode node in nodes)
            AddNode(node);
    }

    // / <summary>
    // / Get the adjacent nodes of a node, where the nodes can be visited from
    // / the node.
    // / </summary>
    // public void TNodes[] AdjacentNodes(TNode node)
    // {
    //     int idx = nodes.IndexOf(node));
    // }

    /// <summary>Add an edge between nodes.</summary>
    /// <param name="from">Index of from node (row).</param>
    /// <param name="to">Index of to node (column).</param>
    /// <param name="weight">Edge weight (defaults to 1).</param>
    public void UpdateEdge(int from, int to, double weight = 1.0)
    {
        if (from > (matrix.Count - 1) | from < 0)
            throw new ArgumentException(ErrorMessages.E1);
        if (to > (matrix[from].Count - 1) | to < 0)
            throw new ArgumentException(ErrorMessages.E2);
        matrix[from][to] = weight;
    }

}