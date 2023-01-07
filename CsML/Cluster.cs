using Microsoft.Toolkit.HighPerformance;

using CsML.Utility;

namespace CsML.Cluster;

/// <summary>Cluster data using a k-means clustering algorithm.</summary>
public class KMeans
{
    /// <summary>Number of clusters, each with a centroid.</summary>
    public int numberOfClusters;

    /// <summary>Cluster labels.</summary>
    public int[] ClusterLabels { get { return _clusterLabels; } }

    /// <summary>
    /// The number of matrix columns the model was trained on.
    /// </summary>
    public int minColumns;

    /// <summary>
    /// Centroid locations with columns related to training data
    /// colums.
    /// </summary>
    public double[,] centroids;

    private int _maxIterations;
    private int[] _clusterLabels;

    /// <summary>Create a new k-means clusterer.</summary>
    public KMeans()
    {
        numberOfClusters = 5;
        centroids = new double[,] { };
        minColumns = 0;
        _maxIterations = 10_000;
        _clusterLabels = Array.Empty<int>();
    }

    /// <summary>Get a string representation of an instance.</summary>
    public override string ToString()
            => $"KMeans(clusters:{numberOfClusters})";

    /// <summary>
    /// Cluster a matrix into the specified number of clusters. Consider
    /// scaling data first e.g. using Z-scores.
    /// </summary>
    /// <param name="matrix">The features to find clusters in.</param>
    /// <param name="clusters">The number clusters to create.</param>
    /// <returns>
    /// An array containing the cluster number assigned to each row in
    /// the input matrix.
    /// </returns>
    public int[] Cluster(double[,] matrix, int clusters = 5)
    {
        numberOfClusters = clusters;
        minColumns = matrix.GetLength(1);
        InitialiseCentroids(matrix);
        int iterations = 0;
        while (iterations < _maxIterations)
        {
            iterations++;
            AssignCentroids(matrix);
            if (!CentroidsMoving(matrix)) break;
        }
        if (iterations >= _maxIterations)
            throw new ArgumentException(ErrorMessages.E7);
        return _clusterLabels;
    }

    /// <summary>
    /// Create centroids at random points within max and min
    /// bounds of each column.
    /// </summary>
    public void InitialiseCentroids(double[,] matrix)
    {
        Span2D<double> matrixSpan = matrix;
        Random random = new Random();
        _clusterLabels = Enumerable.Repeat(0, matrix.GetLength(0)).ToArray();
        centroids = new double[numberOfClusters, matrix.GetLength(1)];
        double colMin, colMax;
        for (int colIdx = 0; colIdx < matrix.GetLength(1); colIdx++)
        {
            var col = matrixSpan.GetColumn(colIdx).ToArray();
            colMin = col.Min();
            colMax = col.Max();
            for (int cIdx = 0; cIdx < numberOfClusters; cIdx++)
                centroids[cIdx, colIdx] = ((colMax - colMin) *
                                            random.NextDouble()) +
                                            colMin;
        }
    }

    /// <summary>Assign each record to its closest centroid.</summary>
    public void AssignCentroids(double[,] matrix)
    {
        Span2D<double> matrixSpan = matrix;
        Span2D<double> centroidSpan = centroids;
        var distancesFromCentroid = Enumerable.Repeat(0.0, numberOfClusters)
                                              .ToList();
        double distance, minDistance;
        for (int rowIdx = 0; rowIdx < matrix.GetLength(0); rowIdx++)
        {
            var row = matrixSpan.GetRow(rowIdx).ToArray();
            for (int cIdx = 0; cIdx < numberOfClusters; cIdx++)
            {
                var centroid = centroidSpan.GetRow(cIdx).ToArray();
                distance = Utility.Arrays.DistanceEuclidian(row, centroid);
                distancesFromCentroid[cIdx] = distance;
            }
            minDistance = distancesFromCentroid.Min();
            _clusterLabels[rowIdx] = distancesFromCentroid.IndexOf(minDistance);
        }
    }

    /// <summary>
    /// Recompute the centroid for each cluster. Return true if centroids
    /// are still moving, false if they have stopped.
    /// </summary>
    public bool CentroidsMoving(double[,] matrix)
    {
        Span2D<double> matrixSpan = matrix;
        double[,] oldCentroids = (double[,])centroids.Clone();
        bool[] filter;
        for (int centroidIdx = 0; centroidIdx < numberOfClusters; centroidIdx++)
        {
            filter = _clusterLabels.Select(x => x == centroidIdx).ToArray();
            for (int colIdx = 0; colIdx < matrix.GetLength(1); colIdx++)
            {
                var col = matrixSpan.GetColumn(colIdx).ToArray();
                col = col.Zip(filter)
                         .Where(x => x.Item2)
                         .Select(x => x.Item1)
                         .ToArray();
                if (col.Length == 0) continue;
                centroids[centroidIdx, colIdx] = col.Average();
            }
        }
        return !CsML.Utility.Matrix.Equal(centroids, oldCentroids);
    }

    /// <summary>Determine the closest centroid to a new data point.</summary>
    /// <param name="row">
    /// Row of data containing same number of columns as the matrix used to
    /// cluster data.
    /// </param>
    /// <returns>The index of the closest centroid.</returns>
    public int ClosestCentroid(double[] row)
    {
        if (row.Length != minColumns)
            throw new ArgumentException(Utility.ErrorMessages.E4);
        Span2D<double> centroidsSpan = centroids;
        var distancesFromCentroid = Enumerable.Repeat(0.0, numberOfClusters).ToList();
        double distance, minDistance;
        double[] centroid;
        for (int centroidIdx = 0; centroidIdx < numberOfClusters; centroidIdx++)
        {
            centroid = centroidsSpan.GetRow(centroidIdx).ToArray();
            distance = Utility.Arrays.DistanceEuclidian(row, centroid);
            distancesFromCentroid[centroidIdx] = distance;
        }
        minDistance = distancesFromCentroid.Min();
        return distancesFromCentroid.IndexOf(minDistance);
    }

    /// <summary>
    /// Determine the optimal number of clusters for kmeans using the elbow
    /// method.
    /// </summary>
    /// <param name="matrix">The features to find clusters in.</param>
    /// <param name="k"></param>
    /// <returns>
    /// The sum of squared error (SSE) of each cluster number in an array.
    /// Array value 0 is the SSE for 1 cluster, array value 2 is the SSE for 2
    /// clusters etc.
    /// </returns>
    public static double[] OptimalKElbow(double[,] matrix, int k = 15)
    {
        double[] result = Enumerable.Repeat(0.0, k).ToArray();
        Span2D<double> matrixSpan = matrix;
        double sse;
        bool[] filter;
        double[] col, colFiltered;
        for (int i = 1; i <= k; i++)
        {
            KMeans km = new();
            km.Cluster(matrix, i);
            sse = 0;
            for (int centroidIdx = 0; centroidIdx < i; centroidIdx++)
            {
                filter = km.ClusterLabels.Select(x => x == centroidIdx).ToArray();
                for (int colIdx = 0; colIdx < matrix.GetLength(1); colIdx++)
                {
                    col = matrixSpan.GetColumn(colIdx).ToArray();
                    colFiltered = col.Zip(filter)
                                     .Where(x => x.Item2)
                                     .Select(x => x.Item1)
                                     .ToArray();
                    if (colFiltered.Length == 0) continue;
                    sse += CsML.Utility.Statistics.SSEvsMean(colFiltered);
                }
            }
            result[i - 1] = sse;
        }
        return result;
    }
}

/// <summary>A nearest neighbour model.</summary>
public class NearestNeighbour : IModel
{
    /// <summary>
    /// A copy of the training data. Consider scaling
    /// data first e.g. using Z-scores.
    /// </summary>
    public double[,] train;

    /// <summary>A copy of training data labels.</summary>
    public double[] target;

    /// <summary>
    /// Number of neighbours taken into account when
    /// deciding class labels.
    /// </summary>
    public int numberOfNeighbours;

    /// <summary>Mode defined by ModelType enum.</summary>
    public ModelType Mode = ModelType.Classification;

    /// <summary>
    /// The number of matrix columns the model was trained on.
    /// </summary>
    public int minColumns;

    /// <summary>Create an untrained model.</summary>
    public NearestNeighbour(ModelType mode)
    {
        Mode = mode;
        train = new double[,] { };
        target = new double[] { };
        numberOfNeighbours = 5;
        minColumns = 0;
    }

    /// <summary>Get a string representation of an instance.</summary>
    public override string ToString() => $"NearestNeighbour(mode:{Mode})";

    /// <summary>Train the model.</summary>
    /// <param name="matrix">The features to train the model on.</param>
    /// <param name="target">The target vector to train on.</param>
    /// <exception cref="System .ArgumentException">
    /// Thrown if inputs aren't the same length.
    /// </exception>
    public void Train(double[,] matrix, double[] target)
    {
        minColumns = matrix.GetLength(1);
        train = matrix;
        this.target = target;
    }

    /// <summary>Make predictions using the model.</summary>
    /// <param name="matrix">New data to infer predictions from.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if input is empty, or model has not been trained, or if trained
    /// on a different number of columns.
    /// </exception>
    public double[] Predict(double[,] matrix)
    {
        if (matrix.GetLength(1) != minColumns)
            throw new ArgumentException(Utility.ErrorMessages.E4);
        Span2D<double> matrixSpan = matrix;
        Span2D<double> trainSpan = train;
        int inputRecordCount = matrix.GetLength(0);
        int trainRecordCount = train.GetLength(0);
        var result = new double[inputRecordCount];
        double[] row, trainRow, distances;
        int[] neighbours;
        for (int i = 0; i < inputRecordCount; i++)
        {
            row = matrixSpan.GetRow(i).ToArray();
            distances = Enumerable.Repeat(0.0, trainRecordCount).ToArray();
            for (int t = 0; t < trainRecordCount; t++)
            {
                trainRow = trainSpan.GetRow(t).ToArray();
                distances[t] = CsML.Utility.Arrays
                                    .DistanceEuclidian(row, trainRow);
            }
            neighbours = distances.Zip(Enumerable.Range(0, distances.Length))
                                  .OrderBy(x => x.Item1)
                                  .Select(x => x.Item2)
                                  .ToArray();
            if (Mode == ModelType.Regression)
                result[i] = neighbours[0..numberOfNeighbours]
                                      .Select(x => target[x])
                                      .ToArray()
                                      .Average();
            else
            {
                double[] nearest = neighbours[0..numberOfNeighbours]
                                    .Select(x => target[x])
                                    .ToArray();
                var counter = new CsML.Probability.Counter<double>(nearest);
                result[i] = counter.MaxKey();
            }
        }
        return result;
    }
}