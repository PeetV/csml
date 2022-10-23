using Microsoft.Toolkit.HighPerformance;
using Xunit;

namespace Csml.Tests.Forest;

public class RandomForest
{
    [Fact]
    public void Train_Predict_iris()
    {
        var mapping = new Dictionary<int, Dictionary<string, double>>();
        mapping[4] = new Dictionary<string, double>
           {
               { "versicolor", 0 }, {"virginica", 1 }, {"setosa", 2}
           };
        string strWorkPath = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
        string inpuPath = Path.Combine(strWorkPath, "Data/iris.csv");
        double[,] matrix = CsML.Util.Matrix.FromCSV(inpuPath, mapping, loadFromRow: 1);
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

        (features, target) = CsML.Util.Features.Shuffle(features, target);
        double[,] ftrain, ftest;
        double[] ttrain, ttest;
        ((ftrain, ttrain), (ftest, ttest)) = CsML.Util.Features.Split(features, target, 0.8);
        CsML.Forest.RandomForest forest = new CsML.Forest.RandomForest("classify", CsML.Util.Statistics.Gini);
        forest.Train(ftrain, ttrain);
        Assert.True(forest.trees.Count > 0);
        double[] predictions = forest.Predict(ftest);
        Assert.True(CsML.Util.Array.ClassificationAccuracy(ttest, predictions) > 0.8);
    }
}