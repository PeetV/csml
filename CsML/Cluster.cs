namespace CsML.Cluster;

/// <summary>
/// Cluster data using a k-means clustering algorithm.
/// </summary>
public class KMeansClusterer
{
    /// <summary>Number of clusters with centroids.</summary>
    public int numberOfClusters { get { return _numberOfClusters; } }

    private int _numberOfClusters;
    private int _maxIterations;
    private int[] _centroids;
    private int _minColumns;
    private int[] _clusterLabels;

    /// <summary>Create a new k-means clusterer.</summary>
    public KMeansClusterer()
    {
        _numberOfClusters = 0;
        _maxIterations = 10_000;
        _centroids = Array.Empty<int>();
        _minColumns = 0;
        _clusterLabels = Array.Empty<int>();
    }

    public override string ToString()
    {
        return $"KMeansClusterer(clusters={_numberOfClusters})";
    }

    /// <summary>
    /// Cluster a matrix into the specified number of clusters.  Consider
    /// scaling data first e.g. z-scores.
    /// </summary>
    /// <param name="matrix">The features to find clusters in.</param>
    /// <param name="clusters">The number clusters to create.</param>
    /// <returns>
    /// An array containing the cluster number assigned to each row in
    /// the input matrix.
    /// </returns>
    public int[] Cluster(double[,] matrix, int clusters = 5)
    {
        _numberOfClusters = clusters;
        _minColumns = matrix.GetLength(1);
        InitialiseCentroids(matrix);
        int iterations = 0;
        while (iterations < _maxIterations)
        {
            iterations++;
            AssignCentroids(matrix);
            if (!CentroidsMoving(matrix)) break;
        }
        return _clusterLabels;
    }

    private void InitialiseCentroids(double[,] matrix)
    {
        //self.cluster_labels = np.zeros(len(table))
        //    self.centroids = np.zeros((self.number_of_clusters, table.shape[1]))
        //    for centroid_index in range(self.number_of_clusters):
        //        for column_index in range(table.shape[1]):
        //            min_val = np.amin(table[:, column_index])
        //            max_val = np.amax(table[:, column_index])
        //            rand = np.random.random_sample()
        //            rand_val = (max_val - min_val) * rand + min_val
        //            self.centroids[centroid_index, column_index] = rand_val				
    }

    private void AssignCentroids(double[,] matrix)
    {
        //# Form k clusters by assigning each record to its closest centroid.
        //    distance_from_centroid = np.zeros(self.number_of_clusters)
        //    for row_index in range(len(table)):
        //        row_data = table[row_index]
        //        for centroid_index in range(self.number_of_clusters):
        //            centroid_data = self.centroids[centroid_index]
        //            distance = util.distance_euclidian(row_data, centroid_data)
        //            distance_from_centroid[centroid_index] = distance
        //        self.cluster_labels[row_index] = np.argmin(distance_from_centroid)
    }

    private bool CentroidsMoving(double[,] matrix)
    {
        // # Recompute the centroid for each cluster. Return true if centroids
        //    # are still moving, false if they have stopped.
        //    old_centroids = np.copy(self.centroids)
        //    for centroid_index in range(self.number_of_clusters):
        //        filter = self.cluster_labels == centroid_index
        //        for column_index in range(table.shape[1]):
        //            vals = table[filter, column_index]
        //            if len(vals) > 0:
        //                new_val = np.mean(vals)
        //                self.centroids[centroid_index, column_index] = new_val
        //									return not np.array_equal(old_centroids, self.centroids)
        return false;
    }

    public int ClosestCentroid(double[] row)
    {
        //	"""
        //    Determine the closest centroid to the new data point.
        //    """
        //    if len(row) != self.min_columns:
        //        raise util.PymlError(f"want {self.min_columns} columns, got {len(row)}")
        //    distance_from_centroid = np.zeros(self.number_of_clusters)
        //    for centroid_index, centroid_data in enumerate(self.centroids):
        //        distance = util.distance_euclidian(row, centroid_data)
        //        distance_from_centroid[centroid_index] = distance
        //    return np.argmin(distance_from_centroid)
        return 0;
    }

    public static void OptimalKElbow(double[,] matrix)
    {
        //def optimal_k_elbow(
        //    cls, table: np.array, max_k: int = 15, callback: T.Optional[T.Callable] = None
        //	 ) -> np.array:
        //    """
        //    Determine the optimal number of clusters for kmeans using the elbow
        //    method. Returns the sum of squared error (SSE) of each cluster number
        //    in an array. Note: array value 0 is the SSE for 1 cluster, array
        //    value 2 is the SSE for 2 clusters etc.
        //    """
        //    result: np.array = np.zeros(max_k)
        //    for k in range(1, max_k + 1):
        //        if callback is not None:
        //            callback()
        //        km = KMeansClusterer()
        //        km.cluster(table, k)
        //        sse: float = 0.0
        //        for centroid_index in range(km.number_of_clusters):
        //            centroid_filter = km.cluster_labels == centroid_index
        //            if centroid_filter.sum() == 0:
        //                continue
        //            for column_index in range(table.shape[1]):
        //                column_values = table[centroid_filter, column_index]
        //                mn = column_values.mean()
        //                se = np.square(column_values - mn)
        //                sse += se.sum()
        //        result[k - 1] = sse
        //    return result
    }
}
