using Microsoft.Toolkit.HighPerformance;
using Xunit;

namespace CsML.Tests.Cluster;

public class Kmeans
{
    [Fact]
    public void InitialiseCentroids()
    {
        double[,] matrix = {
            {1.0, 2.0, 0.0},
            {1.0, 3.0, 0.0},
            {1.0, 4.0, 0.0}
        };
        var km = new CsML.Cluster.KMeans();
        km.numberOfClusters = 2;
        km.InitialiseCentroids(matrix);
        Assert.Equal(matrix.GetLength(1), km.centroids.GetLength(1));
        Assert.Equal(2, km.centroids.GetLength(0));
        double val = km.centroids[0, 0];
        Assert.Equal(1.0, val);
        val = km.centroids[0, 1];
        Assert.True(val >= 2.0);
        Assert.True(val <= 4.0);
    }

    [Fact]
    public void Cluster()
    {
        double[,] matrix = {
          {1.1, 1}, {1.2, 3}, {2, 2}, {10, 11}, {12, 13}, {9, 12}
        };
        var km = new CsML.Cluster.KMeans();
        int[] result = km.Cluster(matrix, 2);
        Assert.Equal(6, result.Length);
        Assert.Equal(result[0], result[1]);
        Assert.Equal(result[0], result[2]);
        Assert.Equal(result[3], result[4]);
        Assert.Equal(result[3], result[5]);
    }

    [Fact]
    public void ClosestCentroid()
    {
        double[,] matrix = {
          {1.1, 1}, {1.2, 3}, {2, 2}, {10, 11}, {12, 13}, {9, 12}
        };
        var km = new CsML.Cluster.KMeans();
        int[] result = km.Cluster(matrix, 2);
        int clust = km.ClosestCentroid(new double[] { 0.9, 1.5 });
        Assert.Equal(result[0], clust);
    }

    [Fact]
    public void OptimalKElbow()
    {
        double[,] matrix = {
          {1.1, 1}, {1.2, 3}, {2, 2}, {10, 11}, {12, 13}, {9, 12}
        };
        double[] sses = CsML.Cluster.KMeans.OptimalKElbow(matrix, k: 5);
        Assert.Equal(5, sses.Length);
        Assert.True(sses[0] > sses[1]);
    }
}

public class NearestNeighbour
{
    [Fact]
    public void Iris()
    {
        var mapping = new Dictionary<int, Dictionary<string, double>>();
        mapping[4] = new Dictionary<string, double>
           {
               { "versicolor", 0 }, {"virginica", 1 }, {"setosa", 2}
           };
        string strWorkPath = Directory
                                .GetParent(Environment.CurrentDirectory)!
                                .Parent!
                                .Parent!
                                .FullName;
        string inpuPath = Path.Combine(strWorkPath, "Data/iris.csv");
        double[,] matrix = CsML.Utility.Matrix.FromCSV(
                                inpuPath, mapping, loadFromRow: 1);
        Span2D<double> matrixSpan = matrix;
        double[,] features = matrixSpan.Slice(0, 0, 150, 4).ToArray();
        Assert.Equal(5.1, features[0, 0]);
        Assert.Equal(3.5, features[0, 1]);
        Assert.Equal(1.4, features[0, 2]);
        Assert.Equal(0.2, features[0, 3]);
        Assert.Equal(150, features.GetLength(0));
        double[] target = matrixSpan.GetColumn(4).ToArray();
        Assert.Equal(2, target[0]);
        Assert.Equal(150, target.Length);
        (features, target) = CsML.Utility.Features.Shuffle(features, target);
        double[,] ftrain, ftest;
        double[] ttrain, ttest;
        ((ftrain, ttrain), (ftest, ttest)) = CsML.Utility.Features.Split(
                                                features, target, 0.8);
        var n = new CsML.Cluster.NearestNeighbour(CsML.Utility.ModelType.Classification);
        n.Train(ftrain, ttrain);
        double[] predictions = n.Predict(ftest);
        Assert.True(CsML.Utility.Arrays.ClassificationAccuracy(
                    ttest, predictions) > 0.8);
    }
}