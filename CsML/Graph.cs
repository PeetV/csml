namespace CsML.Graph;

/// <summary>Error message descriptions.</summary>
public static class ErrorMessages
{
    /// <summary>Error message if from node index is out of bounds.</summary>
    public const string E1 = "From node index out of bounds";

    /// <summary>Error message if to node index is out of bounds.</summary>
    public const string E2 = "To node index out of bounds";

    /// <summary>Error message if a node is found to not be unique. </summary>
    public const string E3 = "Nodes must be unique";
}

/// <summary>A graph with edges that have direction and weight.</summary>
public class Graph<TNode> where TNode : notnull
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
    public Graph()
    {
        nodes = new List<TNode>();
        matrix = new List<List<double>>();
    }

    /// <summary>Add a node and expand the adjacency matrix.</summary>
    /// <param name="node">The node to add.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if the node is already in the graph.
    /// </exception>
    public void AddNode(TNode node)
    {
        if (nodes.IndexOf(node) != -1)
            throw new ArgumentException(ErrorMessages.E3);
        nodes.Add(node);
        if (matrix.Count == 0)
        {
            matrix.Add(new List<double>() { 0.0 });
            return;
        }
        foreach (List<double> row in matrix)
            row.Add(0.0);
        matrix.Add(Enumerable.Repeat(0.0, matrix[0].Count).ToList());
    }

    /// <summary>Add nodes and expand the adjacency matrix.</summary>
    /// <param name="nodes">The nodes to add.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if a node is already in the graph.
    /// </exception>
    public void AddNodes(TNode[] nodes)
    {
        foreach (TNode node in nodes)
            AddNode(node);
    }

    /// <summary>
    /// Get the adjacent nodes of a node, where the nodes can be visited from
    /// the node.
    /// </summary>
    public TNode[] Neighbours(TNode node)
    {
        List<TNode> result = new();
        int row = nodes.IndexOf(node);
        if (row == -1) return result.ToArray();
        for (int col = 0; col < matrix[0].Count; col++)
        {
            if (matrix[row][col] != 0)
                result.Add(nodes[col]);
        }
        return result.ToArray();
    }

    /// <summary>
    /// Get the adjacent nodes index of a node, where the nodes can be visited
    /// from the node.
    /// </summary>
    public int[] Neighbours(int nodeIndex)
    {
        List<int> result = new();
        for (int col = 0; col < matrix[0].Count; col++)
        {
            if (matrix[nodeIndex][col] != 0)
                result.Add(col);
        }
        return result.ToArray();
    }

    /// <summary>Add an edge between nodes.</summary>
    /// <param name="from">Index of from node (row).</param>
    /// <param name="to">Index of to node (column).</param>
    /// <param name="weight">Edge weight (defaults to 1).</param>
    /// <param name="undirected">
    /// Update edige in both directions if false.
    /// </param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if a from or to are outside of bounds.
    /// </exception>
    public void UpdateEdge(
        int from, int to,
        double weight = 1.0,
        bool undirected = false
    )
    {
        if (from > (matrix.Count - 1) | from < 0)
            throw new ArgumentException(ErrorMessages.E1);
        if (to > (matrix[from].Count - 1) | to < 0)
            throw new ArgumentException(ErrorMessages.E2);
        matrix[from][to] = weight;
        if (undirected) matrix[to][from] = weight;
    }

    /// <summary>Add an edge between nodes.</summary>
    /// <param name="from">From node.</param>
    /// <param name="to">To node.</param>
    /// <param name="weight">Edge weight (defaults to 1).</param>
    /// <param name="undirected">
    /// Update edige in both directions if false.
    /// </param>
    public void UpdateEdge(TNode from, TNode to,
        double weight = 1.0, bool undirected = false)
    {
        int fromIdx = nodes.IndexOf(from);
        if (fromIdx > (matrix.Count - 1) | fromIdx < 0)
            throw new ArgumentException(ErrorMessages.E1);
        int toIdx = nodes.IndexOf(to);
        if (toIdx > (matrix[fromIdx].Count - 1) | toIdx < 0)
            throw new ArgumentException(ErrorMessages.E2);
        UpdateEdge(fromIdx, toIdx, weight);
        if (undirected) UpdateEdge(toIdx, fromIdx, weight);
    }

    /// <summary>Add an edges between nodes.</summary>
    /// <param name="fromtos">Array of from and to node tuples.</param>
    /// <param name="weight">Edge weight (defaults to 1).</param>
    /// <param name="undirected">
    /// Update edge in both directions if false.
    /// </param>
    public void UpdateEdges((TNode, TNode)[] fromtos,
        double weight = 1.0, bool undirected = false)
    {
        foreach ((TNode from, TNode to) in fromtos)
            UpdateEdge(from, to, weight, undirected);

    }

    /// <summary>Calculate the sum of weights of a path.</summary>
    /// <param name="path">List of node index values.</param>
    public double PathCost(int[] path)
    {
        double cost = 0;
        if (path.Length == 0) return cost;
        int current = path[0];
        if (current < 0 | current > nodes.Count - 1)
            throw new ArgumentException(ErrorMessages.E1);
        List<double> row;
        foreach (int next in path)
        {
            row = matrix[current];
            if (next < 0 | next > row.Count - 1)
                throw new ArgumentException(ErrorMessages.E2);
            cost += row[next];
            current = next;
        }
        return cost;
    }

    /// <summary>
    /// Walk along edges to all nodes possible using a depth first approach.
    /// </summary>
    /// <param name="start">The index of the node to start walking from.</param>
    /// <param name ="includeBacktrack">Include backtracking steps.</param>
    /// <param name ="maxSteps">
    /// Maximum number steps before stopping (default 1 million).
    /// </param>
    /// <returns>Array of node index values as steps.</returns>
    public int[] WalkDepthFirst(
        int start,
        bool includeBacktrack = true,
        int maxSteps = 1000000
    )
    {
        int idx = start;
        if (idx < 0 | idx > nodes.Count - 1) return new int[] { };
        List<int> path = new();
        bool[] visited = Enumerable.Repeat(false, nodes.Count).ToArray();
        Stack<int> stack = new();
        int[] unvisitedNeighbours, neighbours;
        int newIdx, pathIdx, steps = 0;
        while (steps <= maxSteps)
        {
            // Visit the node
            path.Add(idx);
            if (!visited[idx]) visited[idx] = true;
            // Check the neighbhours
            neighbours = Neighbours(idx);
            unvisitedNeighbours = neighbours.Where(x => !visited[x]).ToArray();
            if (unvisitedNeighbours.Length == 0 & stack.Count == 0)
                break;
            // Add unvisited neighbours to the top of stack
            foreach (int val in unvisitedNeighbours.Reverse())
                if (!stack.Contains(val)) stack.Push(val);
            // Visit the top of the stack 
            newIdx = stack.Pop();
            // If not possible to visit the top of the stack backtrack
            if (!neighbours.Contains(newIdx) & includeBacktrack)
            {
                pathIdx = path.Count - 1;
                while (pathIdx > 0)
                {
                    pathIdx--;
                    path.Add(path[pathIdx]);
                    if (Neighbours(path[pathIdx]).Contains(newIdx))
                        break;
                }
            }
            idx = newIdx;
            steps++;
        }
        return path.ToArray();
    }

    /// <summary>
    /// Walk along edges to all nodes possible using a depth first approach.
    /// </summary>
    /// <param name="start">The node to start walking from.</param>
    /// <param name ="includeBacktrack">Include backtracking steps.</param>
    /// <param name ="maxSteps">
    /// Maximum number steps before stopping (default 1 million).
    /// </param>
    /// <returns>Array of nodes as steps.</returns>
    public TNode[] WalkDepthFirst(
        TNode start,
        bool includeBacktrack = true,
        int maxSteps = 1000000
    )
    {
        int idx = nodes.IndexOf(start);
        if (idx == -1) return new TNode[] { };
        int[] path = WalkDepthFirst(idx, includeBacktrack, maxSteps);
        return path.Select(x => nodes[x]).ToArray();
    }

}
