using Xunit;

namespace Csml.Tests.Util;

public class Array
{
    [Fact]
    public void BestSplit_Accuracy_Test1()
    {
        double[] values = { 1.0, 1.0, 1.0, 2.0, 12.0, 1.0, 1.0, 2.0, 1.0, 2.0, 3.0, 1.0 };
        double[] target = { 0.0, 0.0, 0.0, 1.0, 1.0, 0.0, 0.0, 1.0, 1.0, 0.0, 0.0, 1.0 };
        var result = CsML.Util.Array.BestSplit(
            values, target, CsML.Util.Statistics.Gini);
        Assert.Equal(7.5, result.Item1);
        Assert.Equal(0.061868686868686684, result.Item2);
    }

    [Fact]
    public void BestSplit_Accuracy_Test2()
    {
        double[] values = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 1.0,
        1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };
        double[] target = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };
        var result = CsML.Util.Array.BestSplit(
            values, target, CsML.Util.Statistics.Gini);
        Assert.Equal(0.5, result.Item1);
        Assert.Equal(0.0625, result.Item2);
    }

    [Fact]
    public void BestSplit_ValuesAllSame()
    {
        double[] values = { 5.0, 5.0, 5.0, 5.0, 5.0 };
        double[] target = { 1.0, 2.0, 1.0, 2.0, 1.0 };
        var result = CsML.Util.Array.BestSplit(
            values, target, CsML.Util.Statistics.Gini);
        Assert.Equal(4.0, result.Item1);
        Assert.Equal(0.0, result.Item2);
    }

    [Fact]
    public void Split()
    {
        double[] vector = new double[] { 1, 1, 1, 2, 2 };
        bool[] filter = new bool[] { true, true, true, false, false };
        double[] lhs, rhs;
        (lhs, rhs) = CsML.Util.Array.Split(vector, filter);
        Assert.Equal(3, lhs.Sum());
        Assert.Equal(4, rhs.Sum());
    }

    [Fact]
    public void ToElementCounts()
    {
        double[] vector = new double[] { 1, 1, 1, 2, 2 };
        var result = CsML.Util.Array.ToElementCounts(vector);
        Assert.Equal(3, result[1]);
        Assert.Equal(2, result[2]);
    }
}

public class Statistics
{
    [Fact]
    public void Gini_Strings()
    {
        string[] stringvals = { "a", "a", "a", "b", "b", "b" };
        double result = CsML.Util.Statistics.Gini(stringvals);
        Assert.Equal(0.5, result);
    }

    [Fact]
    public void Gini_Ints()
    {
        int[] intvals = { 1, 1, 1, 2, 2, 1 };
        double result = CsML.Util.Statistics.Gini(intvals);
        Assert.Equal(0.4444444444444445, result);
    }

    [Fact]
    public void Gini_Empty()
    {
        int[] emptyvals = new int[] { };
        double result = CsML.Util.Statistics.Gini(emptyvals);
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Gini_Different()
    {
        int[] emptyvals = new int[] { 1, 2, 3 };
        double result = CsML.Util.Statistics.Gini(emptyvals);
        Assert.Equal(0.6666666666666665, result);
    }

    [Fact]
    public void SampleTake()
    {
        int[] input = new int[] { 10, 11, 1, 20, 22, 2, 30, 33, 3 };
        int[] result = CsML.Util.Statistics.SampleTake(input, 3);
        Assert.Equal(3, result.Length);
        Assert.True(result.Distinct().Count() == result.Length); // all different
        Assert.True(result.All(x => input.Contains(x))); // all in input
    }
}

public class Matrix
{
    [Fact]
    public void BestSplit()
    {
        double[,] matrix = new double[,] {
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
            {2, 1.0}
        };
        Assert.Equal(12, matrix.GetLength(0)); // rows
        Assert.Equal(2, matrix.GetLength(1)); // columns
        double[] target = { 0, 0, 0, 1, 1, 0, 1, 1, 1, 0, 0, 1 };
        var result = CsML.Util.Matrix.BestSplit(
            matrix, target, CsML.Util.Statistics.Gini, -1);
        Assert.Equal(0, result.Item1); // best column
        Assert.Equal(0.5, result.Item2.Item1); // best split
        Assert.Equal(0.05555555555555555, result.Item2.Item2); // best gain
    }

    [Fact]
    public void Equal()
    {
        double[,] matrix1 = new double[,] { { 1, 1 }, { 2, 2 } };
        double[,] matrix2 = new double[,] { { 1, 1 }, { 2, 2 } };
        double[,] matrix3 = new double[,] { { 1, 1 }, { 3, 3 } };
        double[,] matrix4 = new double[,] { { 1, 1 } };
        Assert.True(CsML.Util.Matrix.Equal(matrix1, matrix2));
        Assert.False(CsML.Util.Matrix.Equal(matrix1, matrix3));
        Assert.False(CsML.Util.Matrix.Equal(matrix1, matrix4));
    }

    [Fact]
    public void FromCSV()
    {
        var mapping = new Dictionary<int, Dictionary<string, double>>();
        mapping[4] = new Dictionary<string, double> 
        { 
            { "versicolor", 0 }, {"virginica", 1 }, {"setosa", 2}
        };
        string strWorkPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        string inpuPath = Path.Combine(strWorkPath, "Data/iris.csv");
        double[,] matrix = CsML.Util.Matrix.FromCSV(inpuPath, mapping, loadFromRow: 1);
        Assert.Equal(150, matrix.GetLength(0));
        Assert.Equal(5.1, matrix[0, 0]);
        Assert.Equal(3.5, matrix[0, 1]);
        Assert.Equal(1.4, matrix[0, 2]);
        Assert.Equal(0.2, matrix[0, 3]);
        Assert.Equal(2, matrix[0, 4]);
        Assert.Equal(5.9, matrix[149, 0]);
        Assert.Equal(3.0, matrix[149, 1]);
        Assert.Equal(5.1, matrix[149, 2]);
        Assert.Equal(1.8, matrix[149, 3]);
        Assert.Equal(1, matrix[149, 4]);
    }

    [Fact]
    public void Split()
    {
        double[,] matrix = new double[,]{
            {0, 1.0, 0},
            {1, 1.0, 0},
            {2, 1.0, 0},
            {3, 2.0, 1},
            {4, 2.0, 1},
            {5, 1.0, 0},
            {6, 2.0, 1},
            {7, 2.0, 1},
            {8, 2.0, 1},
            {9, 1.0, 0},
            {10, 1.0, 0},
            {11, 2.0, 1},
        };
        double[,] expectedLhs =  {
            {3, 2.0, 1},
            {4, 2.0, 1},
            {6, 2.0, 1},
            {7, 2.0, 1},
            {8, 2.0, 1},
            {11, 2.0, 1}
        };
        double[,] expectedRhs =  {
            {0, 1.0, 0},
            {1, 1.0, 0},
            {2, 1.0, 0},
            {5, 1.0, 0},
            {9, 1.0, 0},
            {10, 1.0, 0}
        };
        bool[] expectedFilter = {
            false, false, false,
            true, true,
            false,
            true, true, true,
            false, false,
            true
        };
        var result = CsML.Util.Matrix.Split(matrix, 2, 0.5);
        var lhs = result.Item1.Item1;
        var rhs = result.Item1.Item2;
        var filter = result.Item2;
        Assert.True(CsML.Util.Matrix.Equal(expectedLhs, lhs));
        Assert.True(CsML.Util.Matrix.Equal(expectedRhs, rhs));
        Assert.False(CsML.Util.Matrix.Equal(lhs, rhs));
        Assert.True(expectedFilter.SequenceEqual(filter));
    }
}
