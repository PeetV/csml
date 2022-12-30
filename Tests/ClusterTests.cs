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
}