namespace CsML.Graph;

/// <summary>Error message descriptions.</summary>
public static class ErrorMessages
{
    /// <summary>Error message if from node index is out of bounds.</summary>
    public const string E1 = "From node index out of bounds";

    /// <summary>Error message if to node index is out of bounds.</summary>
    public const string E2 = "To node index out of bounds";

    /// <summary>Error message if a node is found to not be unique.</summary>
    public const string E3 = "Nodes must be unique";

    /// <summary>Error message if a node cannot be found.</summary>
    public const string E4 = "Node not found in graph";

    /// <summary>Error message if edge weight cannot be negative.</summary>
    public const string E5 = "Edge weight can not be negative";

    /// <summary>Error message if maximum search steps is exceeded.</summary>
    public const string E6 = "Maximum search steps exceeded";
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

    /// <summary>The number of nodes in the graph.</summary>
    public int Order { get { return nodes.Count(); } }

    /// <summary>The number of edges in the graph.</summary>
    public int Size
    {
        get
        {
            if (matrix.Count() == 0) return 0;
            int size = 0;
            int duplicates = 0;
            for (int from = 0; from < matrix.Count(); from++)
                for (int to = 0; to < matrix[0].Count(); to++)
                {
                    if (matrix[from][to] != 0)
                        size++;
                    if (matrix[from][to] != 0 & matrix[to][from] != 0)
                        duplicates++;
                }
            return size - (duplicates / 2);
        }
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
    /// <param name="fromNodeIndex">Index of from node (row).</param>
    /// <param name="toNodeIndex">Index of to node (column).</param>
    /// <param name="weight">Edge weight (defaults to 1).</param>
    /// <param name="undirected">
    /// Update edige in both directions if false.
    /// </param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if a from or to are outside of bounds.
    /// </exception>
    public void UpdateEdge(
        int fromNodeIndex, int toNodeIndex,
        double weight = 1.0,
        bool undirected = false
    )
    {
        if (fromNodeIndex > (matrix.Count - 1) | fromNodeIndex < 0)
            throw new ArgumentException(ErrorMessages.E1);
        if (toNodeIndex > (matrix[fromNodeIndex].Count - 1) | toNodeIndex < 0)
            throw new ArgumentException(ErrorMessages.E2);
        matrix[fromNodeIndex][toNodeIndex] = weight;
        if (undirected) matrix[toNodeIndex][fromNodeIndex] = weight;
    }

    /// <summary>Add an edge between nodes.</summary>
    /// <param name="fromNode">From node.</param>
    /// <param name="toNode">To node.</param>
    /// <param name="weight">Edge weight (defaults to 1).</param>
    /// <param name="undirected">
    /// Update edige in both directions if false.
    /// </param>
    public void UpdateEdge(TNode fromNode, TNode toNode,
        double weight = 1.0, bool undirected = false)
    {
        int fromIdx = nodes.IndexOf(fromNode);
        if (fromIdx > (matrix.Count - 1) | fromIdx < 0)
            throw new ArgumentException(ErrorMessages.E1);
        int toIdx = nodes.IndexOf(toNode);
        if (toIdx > (matrix[fromIdx].Count - 1) | toIdx < 0)
            throw new ArgumentException(ErrorMessages.E2);
        UpdateEdge(fromIdx, toIdx, weight);
        if (undirected) UpdateEdge(toIdx, fromIdx, weight);
    }

    /// <summary>Add edges between nodes.</summary>
    /// <param name="fromToNodes">Array of from and to node tuples.</param>
    /// <param name="weight">Edge weight (defaults to 1).</param>
    /// <param name="undirected">
    /// Update edge in both directions if false.
    /// </param>
    public void UpdateEdges((TNode, TNode)[] fromToNodes,
        double weight = 1.0, bool undirected = false)
    {
        foreach ((TNode from, TNode to) in fromToNodes)
            UpdateEdge(from, to, weight, undirected);
    }

    /// <summary>Calculate the sum of weights of a path.</summary>
    /// <param name="pathByNodeIndex">List of node index values.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if a from or to are outside of bounds.
    /// </exception>
    public double PathCost(int[] pathByNodeIndex)
    {
        double cost = 0;
        if (pathByNodeIndex.Length == 0) return cost;
        int current = pathByNodeIndex[0];
        if (current < 0 | current > nodes.Count - 1)
            throw new ArgumentException(ErrorMessages.E1);
        List<double> row;
        foreach (int next in pathByNodeIndex)
        {
            row = matrix[current];
            if (next < 0 | next > row.Count - 1)
                throw new ArgumentException(ErrorMessages.E2);
            cost += row[next];
            current = next;
        }
        return cost;
    }

    /// <summary>Calculate the sum of weights of a path.</summary>
    /// <param name="pathByNode">List of node index values.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if a node cannot be found in the graph.
    /// </exception>
    public double PathCost(TNode[] pathByNode)
    {
        List<int> index = new();
        int idx;
        foreach (TNode node in pathByNode)
        {
            idx = nodes.IndexOf(node);
            if (idx == -1)
                throw new ArgumentException(ErrorMessages.E4);
            index.Add(idx);
        }
        return PathCost(index.ToArray());
    }

    /// <summary>
    /// Find the shortest path between two nodes, taking into account edge
    /// weights.
    /// </summary>
    /// <param name="fromNodeIndex">Index of start node.</param>
    /// <param name="toNodeIndex">Index of end node.</param>
    /// <returns>
    /// A tuple containing the sum of edge weights along the path (path cost)
    /// and path index list.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown if a negative edge weight is found, which breaks the Dijkstra
    /// algorithm.
    /// </exception>
    public (double, int[]) ShortestPathDijkstra(
        int fromNodeIndex, int toNodeIndex)
    {
        if (fromNodeIndex > (matrix.Count - 1) | fromNodeIndex < 0)
            throw new ArgumentException(ErrorMessages.E1);
        if (toNodeIndex > (matrix[fromNodeIndex].Count - 1) | toNodeIndex < 0)
            throw new ArgumentException(ErrorMessages.E2);
        bool[] visited = Enumerable.Repeat(false, nodes.Count).ToArray();
        double[] dist = Enumerable.Repeat(double.PositiveInfinity,
                                          nodes.Count).ToArray();
        dist[fromNodeIndex] = 0;
        List<int> queue = Enumerable.Range(0, nodes.Count).ToList();
        List<int> prev = Enumerable.Repeat(0, nodes.Count).ToList();
        int current = 0;
        double workingDist, neighbDist;
        while (queue.Count != 0)
        {
            // Find the node in queue with lowest distance 
            (current, _) = queue.Zip(queue.Select(x => dist[x]))
                                .OrderBy(x => x.Second).First();
            visited[current] = true;
            queue.Remove(current);
            // Check each neighbhour and update the distance table with the
            // shortest distance
            int[] neighbours = Neighbours(current);
            if (neighbours.Length == 0) break;
            foreach (int neighbour in neighbours)
            {
                if (!queue.Contains(neighbour)) continue;
                neighbDist = matrix[current][neighbour];
                if (neighbDist < 0)
                    throw new ArgumentException(ErrorMessages.E5);
                workingDist = dist[current] + neighbDist;
                if (workingDist < dist[neighbour])
                {
                    dist[neighbour] = workingDist;
                    prev[neighbour] = current;
                }
            }
        }
        // Extract path from closest node list (prev)
        if (dist[toNodeIndex] == double.PositiveInfinity) return (0, Array.Empty<int>());
        List<int> path = new() { toNodeIndex };
        current = toNodeIndex;
        while (current != fromNodeIndex)
        {
            path.Add(prev[current]);
            current = prev[current];
        }
        path.Reverse();
        return (dist[toNodeIndex], path.ToArray());
    }

    /// <summary>
    /// Find the shortest path between two nodes, taking into account edge
    /// weights.
    /// </summary>
    /// <param name="fromNode">Start node.</param>
    /// <param name="toNode">End node.</param>
    /// <returns>
    /// A tuple containing the sum of edge weights along the path (path cost)
    /// and path as a node list.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown if a negative edge weight is found, which breaks the Dijkstra
    /// algorithm.
    /// </exception>
    public (double, TNode[]) ShortestPathDijkstra(
        TNode fromNode, TNode toNode)
    {
        int fromIdx = nodes.IndexOf(fromNode);
        if (fromIdx == -1) throw new ArgumentException(ErrorMessages.E4);
        int toIdx = nodes.IndexOf(toNode);
        if (toIdx == -1) throw new ArgumentException(ErrorMessages.E4);
        (double distance, int[] path) = ShortestPathDijkstra(fromIdx, toIdx);
        return (distance, path.Select(x => nodes[x]).ToArray());
    }

    /// <summary>
    /// Walk along edges to all nodes possible using a depth first approach.
    /// </summary>
    /// <param name="startNodeIndex">
    /// The index of the node to start walking from.
    /// </param>
    /// <param name ="includeBacktrack">Include backtracking steps.</param>
    /// <param name ="maxSearchSteps">
    /// Maximum number steps before stopping (default 1 million).
    /// </param>
    /// <returns>Array of node index values as steps.</returns>
    /// <exception cref="System.IndexOutOfRangeException">
    /// Thrown if maxSearchSteps parameter is exceeded.
    /// </exception>
    public int[] WalkDepthFirst(
        int startNodeIndex,
        bool includeBacktrack = true,
        int maxSearchSteps = 1_000_000
    )
    {
        int idx = startNodeIndex;
        if (idx < 0 | idx > nodes.Count - 1) return Array.Empty<int>();
        List<int> path = new();
        bool[] visited = Enumerable.Repeat(false, nodes.Count).ToArray();
        Stack<int> stack = new();
        int[] unvisitedNeighbours, neighbours;
        int newIdx, pathIdx, steps = 0;
        while (steps <= maxSearchSteps)
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
        if (steps == maxSearchSteps)
            throw new IndexOutOfRangeException(ErrorMessages.E6);
        return path.ToArray();
    }

    /// <summary>
    /// Walk along edges to all nodes possible using a depth first approach.
    /// </summary>
    /// <param name="startNode">The node to start walking from.</param>
    /// <param name ="includeBacktrack">Include backtracking steps.</param>
    /// <param name ="maxSteps">
    /// Maximum number steps before stopping (default 1 million).
    /// </param>
    /// <returns>Array of nodes as steps.</returns>
    /// <exception cref="System.IndexOutOfRangeException">
    /// Thrown if maxSearchSteps parameter is exceeded.
    /// </exception>
    public TNode[] WalkDepthFirst(
        TNode startNode,
        bool includeBacktrack = true,
        int maxSteps = 1_000_000
    )
    {
        int idx = nodes.IndexOf(startNode);
        if (idx == -1) return Array.Empty<TNode>();
        int[] path = WalkDepthFirst(idx, includeBacktrack, maxSteps);
        return path.Select(x => nodes[x]).ToArray();
    }

}