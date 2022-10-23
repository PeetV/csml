using Microsoft.Toolkit.HighPerformance;
using Xunit;

namespace Csml.Tests.Tree;

public class BinaryTree
{
    [Fact]
    public void Train_classify_gini()
    {
        double[,] features = {
                {0, 1.0},
                {0, 1.0},
                {0, 1.0},
                {0, 2.0},
                {0, 12.0},
                {0, 1.0},
                {1, 1.0},
                {1, 2.0},
                {1, 1.0},
                {2, 2.0},
                {2, 3.0},
                {2, 1.0},
            };
        double[] target = { 0, 0, 0, 1, 1, 0, 1, 1, 1, 0, 0, 1 };
        CsML.Tree.BinaryTree tree = new CsML.Tree.BinaryTree("classify", CsML.Util.Statistics.Gini);
        tree.Train(features, target);
        Assert.Equal(0.5, tree.nodes[0].splitPoint);
        Assert.Equal(0.05555555555555555, tree.nodes[0].purityGain);
        Assert.Equal(0, tree.nodes[0].index);
        Assert.False(tree.nodes[0].isLeaf);
        Assert.True(tree.nodes[0].yesIndex > 0);
        Assert.True(tree.nodes[0].noIndex > 0);
        Assert.Equal(2, tree.minColumns);
    }

    [Fact]
    public void Train_regression_gini()
    {
        double[,] features = {
                {0, 1.0},
                {0, 1.0},
                {0, 1.0},
                {0, 2.0},
                {0, 12.0},
                {0, 1.0},
                {1, 1.0},
                {1, 2.0},
                {1, 1.0},
                {2, 2.0},
                {2, 3.0},
                {2, 1.0},
            };
        double[] target = { 0, 0, 0, 1, 1, 0, 1, 1, 1, 0, 0, 1 };
        CsML.Tree.BinaryTree tree = new CsML.Tree.BinaryTree("regress", CsML.Util.Statistics.Gini);
        tree.Train(features, target);
        Assert.Equal(0.5, tree.nodes[0].splitPoint);
        Assert.Equal(0.05555555555555555, tree.nodes[0].purityGain);
        Assert.Equal(0, tree.nodes[0].index);
        Assert.False(tree.nodes[0].isLeaf);
        Assert.True(tree.nodes[0].yesIndex > 0);
        Assert.True(tree.nodes[0].noIndex > 0);
    }

    [Fact]
    public void Train_constructor_exceptions()
    {
        double[,] features = {
                {0, 1.0},
                {0, 1.0},
                {0, 1.0},
                {0, 2.0},
                {0, 12.0},
                {0, 1.0},
                {1, 1.0},
                {1, 2.0},
                {1, 1.0},
                {2, 2.0},
                {2, 3.0},
                {2, 1.0},
            };
        double[] target = { 0, 0, 0, 1, 1, 0, 1, 1, 1, 0, 0, 1 };
        Assert.Throws<ArgumentException>(() =>
        {
            CsML.Tree.BinaryTree tree = new CsML.Tree.BinaryTree("regression", CsML.Util.Statistics.Gini);
        }
        );
    }

    [Fact]
    public void Train_input_exceptions()
    {
        CsML.Tree.BinaryTree tree = new CsML.Tree.BinaryTree("classify", CsML.Util.Statistics.Gini);
        Assert.Throws<ArgumentException>(() =>
        {
            tree.Train(new double[,] { }, new double[] { });
        });
        Assert.Throws<ArgumentException>(() =>
        {
            tree.Train(new double[,] { { 1, 1 } }, new double[] { 1, 2 });
        });
    }


    [Fact]
    public void Train_Regression()
    {
        var matrix = new double[,]{
            { 0, 1.0 },
            { 0, 1.0 },
            { 0, 1.0 },
            { 0, 2.0 },
            { 0, 12.0 },
            { 0, 1.0 },
            { 1, 1.0 },
            { 1, 2.0 },
            { 1, 1.0 },
            { 2, 2.0 },
            { 2, 3.0 },
            { 2, 1.0 }
        };
        var target = new double[] { 1, 2, 1, 1, 2, 2.1, 1, 2, 5, 5, 5, 5 };
        CsML.Tree.BinaryTree tree = new CsML.Tree.BinaryTree("regress", CsML.Util.Statistics.StdevP);
        Assert.Equal("regress", tree.mode);
        tree.Train(matrix, target);
        Assert.Equal(0, tree.nodes[0].columnIndex);
        Assert.Equal(1.5, tree.nodes[0].splitPoint);
        Assert.False(tree.nodes[0].isLeaf);
        Assert.Equal(5, tree.nodes[1].predicted);
        Assert.Equal(3, tree.nodes[1].recordCount);
        Assert.True(tree.nodes[1].isLeaf);
        Assert.Equal(0, tree.nodes[2].columnIndex);
        Assert.Equal(0.5, tree.nodes[2].splitPoint);
        Assert.Equal(2.6666666666666665, tree.nodes[3].predicted);
        Assert.Equal(3, tree.nodes[3].recordCount);
        Assert.True(tree.nodes[3].isLeaf);
        Assert.Equal(1.5166666666666666, tree.nodes[4].predicted);
        Assert.Equal(6, tree.nodes[4].recordCount);
        Assert.True(tree.nodes[4].isLeaf);
    }

    private static CsML.Tree.BinaryTree ManualTree()
    {
        CsML.Tree.BinaryTree tree = new CsML.Tree.BinaryTree("classify", CsML.Util.Statistics.Gini);
        tree.minColumns = 3;
        tree.classes = new double[] { 3, 4, 5, 6 };
        tree.inputRecordCount = 60;

        CsML.Tree.BinaryNode node1 = new CsML.Tree.BinaryNode();
        node1.index = 0;
        node1.yesIndex = 1;
        node1.noIndex = 2;
        node1.isLeaf = false;
        node1.columnIndex = 0;
        node1.splitPoint = 10;
        node1.recordCount = 10;
        node1.purityGain = 1;
        tree.nodes.Add(node1);

        CsML.Tree.BinaryNode node2 = new CsML.Tree.BinaryNode();
        node2.index = 1;
        node2.yesIndex = 3;
        node2.noIndex = 4;
        node2.isLeaf = false;
        node2.columnIndex = 1;
        node2.splitPoint = 20;
        node2.recordCount = 20;
        node2.purityGain = 2;
        tree.nodes.Add(node2);

        CsML.Tree.BinaryNode node3 = new CsML.Tree.BinaryNode();
        node3.index = 2;
        node3.yesIndex = 5;
        node3.noIndex = 6;
        node3.isLeaf = false;
        node3.columnIndex = 2;
        node3.splitPoint = 30;
        node3.recordCount = 30;
        node3.purityGain = 3;
        tree.nodes.Add(node3);

        CsML.Tree.BinaryNode node4 = new CsML.Tree.BinaryNode();
        node4.index = 3;
        node4.isLeaf = true;
        node4.recordCount = 10;
        node4.classCounts = new Dictionary<double, int> { { 3, 6 }, { 4, 2 }, { 5, 1 }, { 6, 1 } };
        node4.predicted = 3;
        tree.nodes.Add(node4);

        CsML.Tree.BinaryNode node5 = new CsML.Tree.BinaryNode();
        node5.index = 4;
        node5.isLeaf = true;
        node5.recordCount = 20;
        node5.classCounts = new Dictionary<double, int> { { 3, 4 }, { 4, 10 }, { 5, 2 }, { 6, 4 } };
        node5.predicted = 4;
        tree.nodes.Add(node5);

        CsML.Tree.BinaryNode node6 = new CsML.Tree.BinaryNode();
        node6.index = 5;
        node6.isLeaf = true;
        node6.recordCount = 30;
        node6.classCounts = new Dictionary<double, int> { { 3, 1 }, { 4, 2 }, { 5, 25 }, { 6, 2 } };
        node6.predicted = 5;
        tree.nodes.Add(node6);

        CsML.Tree.BinaryNode node7 = new CsML.Tree.BinaryNode();
        node7.index = 6;
        node7.isLeaf = true;
        node7.recordCount = 40;
        node7.classCounts = new Dictionary<double, int> { { 3, 1 }, { 4, 2 }, { 5, 2 }, { 6, 35 } };
        node7.predicted = 6;
        tree.nodes.Add(node7);

        return tree;
    }

    [Fact]
    public void Predict_manually_created_tree()
    {
        CsML.Tree.BinaryTree tree = ManualTree();
        Assert.Equal(7, tree.nodes.Count);
        double[,] newdata = new double[,] { { 1, 1, 1 }, { 1, 1, 40 }, { 20, 1, 1 }, { 20, 30, 1 } };
        double[] expected = new double[] { 6, 5, 4, 3 };
        var result = tree.Predict(newdata);
        Assert.True(result.SequenceEqual(expected));
    }

    [Fact]
    public void Predict_exceptions()
    {
        CsML.Tree.BinaryTree tree = ManualTree();
        Assert.Equal(7, tree.nodes.Count);
        Assert.Throws<ArgumentException>(() =>
        {
            tree.Predict(new double[,] { { 1, 1 }, { 1, 1 } });
        }
        );
        Assert.Throws<ArgumentException>(() =>
        {
            tree.Predict(new double[,] { });
        }
        );
    }

    [Fact]
    public void PredictWithClassCounts()
    {
        CsML.Tree.BinaryTree tree = ManualTree();
        var newData = new double[,]{
            { 1, 1, 1}, { 1, 1, 40}, { 20, 1, 1}, { 20, 30, 1}
        };
        var results = tree.PredictWithClassCounts(newData);
        Assert.Equal(1, results[0].Item2[3]);
        Assert.Equal(2, results[0].Item2[4]);
        Assert.Equal(2, results[0].Item2[5]);
        Assert.Equal(35, results[0].Item2[6]);
        Assert.Equal(1, results[1].Item2[3]);
        Assert.Equal(2, results[1].Item2[4]);
        Assert.Equal(25, results[1].Item2[5]);
        Assert.Equal(2, results[1].Item2[6]);
        Assert.Equal(4, results[2].Item2[3]);
        Assert.Equal(10, results[2].Item2[4]);
        Assert.Equal(2, results[2].Item2[5]);
        Assert.Equal(4, results[2].Item2[6]);
        Assert.Equal(6, results[3].Item2[3]);
        Assert.Equal(2, results[3].Item2[4]);
        Assert.Equal(1, results[3].Item2[5]);
        Assert.Equal(1, results[3].Item2[6]);
    }

    [Fact]
    public void WeightedPurityGains()
    {
        CsML.Tree.BinaryTree tree = ManualTree();
        Assert.Equal(7, tree.nodes.Count);
        double[] gains = tree.WeightedPurityGains();
        Assert.Equal(1.0 * 10.0 / 60.0, gains[0]);
        Assert.Equal(2.0 * 20.0 / 60.0, gains[1]);
        Assert.Equal(3.0 * 30.0 / 60.0, gains[2]);
    }

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
        CsML.Tree.BinaryTree tree = new CsML.Tree.BinaryTree("classify", CsML.Util.Statistics.Gini);
        tree.Train(ftrain, ttrain);
        Assert.True(tree.nodes.Count > 0);
        double[] predictions = tree.Predict(ftest);
        Assert.True(CsML.Util.Array.ClassificationAccuracy(ttest, predictions) > 0.8);
    }
}