using Microsoft.Toolkit.HighPerformance;
using Xunit;

namespace CsML.Tests.Forest;

public class RandomForest
{
    private CsML.Forest.RandomForest BuildForestManually()
    {
        var tree = new CsML.Tree.BinaryTree("classify",
                                            CsML.Util.Statistics.Gini);
        tree.inputRecordCount = 20;
        tree.minColumns = 2;
        tree.classes = new double[] { 5, 6 };

        var node1 = new CsML.Tree.BinaryNode();
        node1.index = 0;
        node1.yesIndex = 1;
        node1.noIndex = 2;
        node1.isLeaf = false;
        node1.columnIndex = 0;
        node1.splitPoint = 10;
        node1.purityGain = 1.0;
        tree.nodes.Add(node1);

        var node2 = new CsML.Tree.BinaryNode();
        node2.index = 1;
        node2.isLeaf = true;
        node2.recordCount = 20;
        node2.classCounts = new Dictionary<double, int> { { 5, 5 }, { 6, 15 } };
        node2.predicted = 5;
        tree.nodes.Add(node2);

        var node3 = new CsML.Tree.BinaryNode();
        node3.index = 2;
        node3.isLeaf = true;
        node3.recordCount = 20;
        node3.classCounts = new Dictionary<double, int> { { 5, 5 }, { 6, 15 } };
        node3.predicted = 6;
        tree.nodes.Add(node3);

        var forest = new CsML.Forest.RandomForest("classify",
                                                  CsML.Util.Statistics.Gini);
        forest.minColumns = 2;
        forest.trees = new List<CsML.Tree.BinaryTree> { tree, tree, tree };

        return forest;
    }

    [Fact]
    public void Predict()
    {
        CsML.Forest.RandomForest forest = BuildForestManually();
        var data = new double[,] { { 11, 11 }, { 9, 9 } };
        var predictions = forest.Predict(data);
        Assert.True(new double[] { 5, 6 }.SequenceEqual(predictions));
    }

    [Fact]
    public void PredictWithProbabilities()
    {
        CsML.Forest.RandomForest forest = BuildForestManually();
        var data = new double[,] { { 11, 11 }, { 9, 9 } };
        var predictions = forest.PredictWithProbabilities(data);
        Assert.True(new double[] { 5, 6 }
                .SequenceEqual(predictions.Select(x => x.Item1).ToArray()));
        Assert.Equal(5.0 / 20.0, predictions[0].Item2[5.0]);
        Assert.Equal(15.0 / 20.0, predictions[0].Item2[6.0]);
    }

    [Fact]
    public void Train_predict_iris()
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
        double[,] matrix = CsML.Util.Matrix.FromCSV(
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

        (features, target) = CsML.Util.Features.Shuffle(features, target);
        double[,] ftrain, ftest;
        double[] ttrain, ttest;
        ((ftrain, ttrain), (ftest, ttest)) = CsML.Util.Features.Split(
                                                features, target, 0.8);
        CsML.Forest.RandomForest forest = new CsML.Forest.RandomForest(
                                                "classify",
                                                CsML.Util.Statistics.Gini);
        forest.Train(ftrain, ttrain);
        Assert.True(forest.trees.Count > 0);
        double[] predictions = forest.Predict(ftest);
        Assert.True(CsML.Util.Array.ClassificationAccuracy(
                    ttest, predictions) > 0.8);
    }
}