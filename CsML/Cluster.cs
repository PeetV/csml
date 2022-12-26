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

    /// <summary>Get a string representation of an instance.</summary>
    public override string ToString() =>
            $"KMeansClusterer(clusters:{_numberOfClusters})";

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

    /// <summary>Determine the closest centroid to a new data point.</summary>
    /// <param name="row">
    /// Row of data containing same number of columns as matrix used to cluster
    /// data.
    /// </param>
    /// <returns>The index of the closest centroid.</returns>
    public int ClosestCentroid(double[] row)
    {
        //    if len(row) != self.min_columns:
        //        raise util.PymlError(f"want {self.min_columns} columns, got {len(row)}")
        //    distance_from_centroid = np.zeros(self.number_of_clusters)
        //    for centroid_index, centroid_data in enumerate(self.centroids):
        //        distance = util.distance_euclidian(row, centroid_data)
        //        distance_from_centroid[centroid_index] = distance
        //    return np.argmin(distance_from_centroid)
        return 0;
    }

    /// <summary>
    /// Determine the optimal number of clusters for kmeans using the elbow
    /// method.
    /// </summary>
    /// <param name="matrix">The features to find clusters in.</param>
    /// <returns>
    /// The sum of squared error (SSE) of each cluster number in an array.
    /// Array value 0 is the SSE for 1 cluster, array value 2 is the SSE for 2
    /// clusters etc.
    /// </returns>
    public static void OptimalKElbow(double[,] matrix)
    {
        //def optimal_k_elbow(
        //    cls, table: np.array, max_k: int = 15, callback: T.Optional[T.Callable] = None
        //	 ) -> np.array:
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

// class KNearestNeighbourClassifier:
//     """
//     A k-nearest neighbour classifier.
//     """

//     def __init__(self):
//         self.min_columns: int = 0
//         self.neighbour_count: int = DEFAULT_NEIGHBOUR_COUNT
//         self.train_table: np.array = np.empty(0)
//         self.labels: np.array = np.empty(0)
//         self.distancefn: T.Callable = util.distance_euclidian

//     def train(self, table: np.array, target: np.array) -> None:
//         """
//         Train the classifier by saving a copy of the data points and their
//         labels. Consider scaling data first e.g. z-scores.
//         """
//         util.checks_before_training(table, target)
//         self.min_columns = table.shape[1]
//         self.train_table = table.copy()
//         self.labels = target.copy()

//     def predict(self, table: np.array) -> np.array:
//         """
//         Predict class labels of new data.
//         """
//         if len(self.train_table) == 0:
//             raise util.PymlError("classifier has not been trained")
//         util.check_table_dimensions(table, self.min_columns)
//         util.raise_if_contains_nanNone(table)
//         result: T.List[int] = []
//         for row_index in range(table.shape[0]):
//             row = np.array([table[row_index]])
//             len_train_table = len(self.train_table)
//             distances = np.zeros(len_train_table)
//             for train_row_index in range(len_train_table):
//                 distances[train_row_index] = self.distancefn(
//                     row, self.train_table[train_row_index]
//                 )
//             # find the neighbour_count number of closest points
//             index_closest = np.argpartition(distances, self.neighbour_count)[
//                 : self.neighbour_count
//             ]
//             votes = self.labels[index_closest]
//             counts: dict = collections.Counter(votes)
//             result.append(max(counts, key=lambda k: counts[k]))
//         return np.array(result)

//     def __str__(self):
//         return "K-Nearest Neighbour Classifier"