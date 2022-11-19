using Xunit;

namespace CsML.Tests.Util;

public class Array
{
    [Fact]
    public void BestSplit_accuracy_test1()
    {
        double[] values = { 1.0, 1.0, 1.0, 2.0, 12.0, 1.0, 1.0, 2.0, 1.0, 2.0,
                            3.0, 1.0 };
        double[] target = { 0.0, 0.0, 0.0, 1.0, 1.0, 0.0, 0.0, 1.0, 1.0, 0.0,
                            0.0, 1.0 };
        var result = CsML.Util.Array.BestSplit(
            values,
            target,
            CsML.Util.Statistics.Gini);
        Assert.Equal(7.5, result.Item1);
        Assert.Equal(0.061868686868686684, result.Item2);
    }

    [Fact]
    public void BestSplit_accuracy_test2()
    {
        double[] values = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 1.0,
        1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };
        double[] target = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };
        var result = CsML.Util.Array.BestSplit(
            values,
            target,
            CsML.Util.Statistics.Gini);
        Assert.Equal(0.5, result.Item1);
        Assert.Equal(0.0625, result.Item2);
    }

    [Fact]
    public void BestSplit_valuesAllSame()
    {
        double[] values = { 5.0, 5.0, 5.0, 5.0, 5.0 };
        double[] target = { 1.0, 2.0, 1.0, 2.0, 1.0 };
        var result = CsML.Util.Array.BestSplit(
            values,
            target,
            CsML.Util.Statistics.Gini);
        Assert.Equal(4.0, result.Item1);
        Assert.Equal(0.0, result.Item2);
    }

    [Fact]
    public void ClassificationAccuracy_all_match()
    {
        double[] values = { 5.0, 5.0, 5.0, 1.0, 1.0, 1.0 };
        double[] target = { 5.0, 5.0, 5.0, 1.0, 1.0, 1.0 };
        double result = CsML.Util.Array.ClassificationAccuracy(values, target);
        Assert.Equal(1, result);
    }

    [Fact]
    public void ClassificationAccuracy_empty()
    {
        double result = CsML.Util.Array.ClassificationAccuracy(
                            System.Array.Empty<double>(),
                            System.Array.Empty<double>());
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void ClassificationAccuracy_half_match()
    {
        double[] values = new double[] { 5.0, 5.0, 5.0, 1.0, 1.0, 1.0 };
        double[] target = new double[] { 5.0, 5.0, 5.0, 5.0, 5.0, 5.0 };
        double result = CsML.Util.Array.ClassificationAccuracy(values, target);
        Assert.Equal(0.5, result);
    }

    [Fact]
    public void ClassificationAccuracy_some_match()
    {
        var values = new double[] { 5.0, 5.0, 5.0, 1.0, 1.0, 1.0 };
        var target = new double[] { 5.0, 5.0, 1.0, 5.0, 5.0, 5.0 };
        var result = CsML.Util.Array.ClassificationAccuracy(values, target);
        Assert.Equal(2.0 / 6.0, result);
    }

    [Fact]
    public void ClassificationError_all_same()
    {
        double[] values = { 5.0, 5.0, 5.0, 1.0, 1.0, 1.0 };
        double[] target = { 5.0, 5.0, 5.0, 1.0, 1.0, 1.0 };
        double result = CsML.Util.Array.ClassificationError(values, target);
        Assert.Equal(0, result);

    }

    [Fact]
    public void ClassificationError_empty()
    {

        var result = CsML.Util.Array.ClassificationAccuracy(
                            System.Array.Empty<double>(),
                            System.Array.Empty<double>());
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void ClassificationError_half_same()
    {
        var values = new double[] { 5.0, 5.0, 5.0, 1.0, 1.0, 1.0 };
        var target = new double[] { 5.0, 5.0, 5.0, 5.0, 5.0, 5.0 };
        var result = CsML.Util.Array.ClassificationError(values, target);
        Assert.Equal(0.5, result);
    }

    [Fact]
    public void ClassificationError_some_match()
    {
        var values = new double[] { 5.0, 5.0, 5.0, 1.0, 1.0, 1.0 };
        var target = new double[] { 5.0, 5.0, 1.0, 5.0, 5.0, 5.0 };
        var result = CsML.Util.Array.ClassificationError(values, target);
        Assert.True(4.0 / 6.0 - result < 0.00000001);
    }

    [Fact]
    public void ClassificationMetrics()
    {
        var actuals = new string[] { "A", "A", "B", "A", "A", "C", "C",
                                     "C", "B" };
        var predicted = new string[] { "A", "B", "B", "A", "C", "C", "C",
                                       "A", "B" };
        var result = CsML.Util.Array.ClassificationMetrics(actuals, predicted);
        Assert.Equal(0.6666666666666666, result["A"].Item1);
        Assert.Equal(0.6666666666666666, result["B"].Item1);
        Assert.Equal(0.6666666666666666, result["C"].Item1);
        Assert.Equal(0.5, result["A"].Item2);
        Assert.Equal(1, result["B"].Item2);
        Assert.Equal(0.6666666666666666, result["C"].Item2);
    }

    [Fact]
    public void ClassificationMetrics_empty()
    {
        var result = CsML.Util.Array.ClassificationMetrics(
            new string[] { }, new string[] { });
        Assert.True(result.Keys.Count == 0);
    }

    [Fact]
    public void Split()
    {
        var vector = new double[] { 1, 1, 1, 2, 2 };
        var filter = new bool[] { true, true, true, false, false };
        double[] lhs, rhs;
        (lhs, rhs) = CsML.Util.Array.Split(vector, filter);
        Assert.Equal(3, lhs.Sum());
        Assert.Equal(4, rhs.Sum());
    }

    [Fact]
    public void ToElementCounts()
    {
        var vector = new double[] { 1, 1, 1, 2, 2 };
        var result = CsML.Util.Array.ElementCounts(vector);
        Assert.Equal(3, result[1]);
        Assert.Equal(2, result[2]);
    }
}

public class Features
{
    [Fact]
    public void ClassProportions()
    {
        var target = new double[] { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 };
        var result = CsML.Util.Features.ClassProportions(target);
        Assert.Equal(1, result[0].Item1);
        Assert.Equal(2, result[0].Item2);
        Assert.Equal(0.2, result[0].Item3);
        Assert.Equal(5, result[4].Item1);
        Assert.Equal(2, result[4].Item2);
        Assert.Equal(0.2, result[4].Item3);
    }

    [Fact]
    public void Bootstrap()
    {
        var matrix = new double[,]
        {
            {1, 1, 1},
            {2, 2, 2},
            {3, 3, 3},
            {4, 4, 4},
            {5, 5, 5}
        };
        var target = new double[] { 1, 2, 3, 4, 5 };
        (double[,] newm, double[] newt, int[] oobidx) = CsML
                            .Util.Features.Bootstrap(matrix, target);
        Assert.Equal(5, newm.GetLength(0));
        Assert.Equal(3, newm.GetLength(1));
        Assert.Equal(5, newt.Length);
        var row0 = CsML.Util.Matrix.GetRow(matrix, 0);
        double[] allowedVals = Enumerable
                                .Range(1, 6)
                                .Select(x => (double)x)
                                .ToArray();
        Assert.True(row0.All(x => allowedVals.Contains(x)));
        Assert.Equal(3, row0.Length);
        Assert.True(row0.All(x => x == row0[0]));
        var oobidxAllowed = new int[] { 0, 1, 2, 3, 4 };
        Assert.True(oobidx.All(x => oobidxAllowed.Contains(x)));
    }

    [Fact]
    public void Shuffle()
    {
        var matrix = new double[,]
        {
            {1, 1, 1},
            {2, 2, 2},
            {3, 3, 3},
            {4, 4, 4},
            {5, 5, 5}
        };
        var target = new double[] { 1, 2, 3, 4, 5 };
        (double[,] newmatrix, double[] newtarget) = CsML.Util
                                    .Features.Shuffle(matrix, target);
        Assert.Equal(5, newmatrix.GetLength(0));
        Assert.Equal(3, newmatrix.GetLength(1));
        Assert.False(CsML.Util.Matrix.Equal(matrix, newmatrix));
        Assert.False(target.SequenceEqual(newtarget));
        Assert.True(newtarget.OrderBy(x => x).ToArray().SequenceEqual(target));
    }

    [Fact]
    public void Split()
    {
        var matrix = new double[,]
         {
            {1, 1, 1},
            {2, 2, 2},
            {3, 3, 3},
            {4, 4, 4},
            {5, 5, 5}
         };
        var target = new double[] { 1, 2, 3, 4, 5 };
        double[,] mlhs, mrhs;
        double[] tlhs, trhs;
        ((mlhs, tlhs), (mrhs, trhs)) = CsML.Util.Features.Split(
                                            matrix, target, 3.0 / 5.0);
        Assert.Equal(3, mlhs.GetLength(0));
        Assert.Equal(3, mlhs.GetLength(1));
        Assert.Equal(2, mrhs.GetLength(0));
        Assert.Equal(3, mrhs.GetLength(1));
        Assert.Equal(3, mlhs[2, 2]);
        Assert.Equal(5, mrhs[1, 2]);
    }

    [Fact]
    public void Profiler_metrics()
    {
        var matrix = new double[,]
        {
            {1, 10, 100},
            {2, 20, 200},
            {3, 30, 300},
        };
        var target = new double[] { 1, 2, 3 };
        var profiler = new CsML.Util.Features.Profiler(matrix, target);
        Assert.Equal(1.5, profiler.columnData[0].q25);
        Assert.Equal(20, profiler.columnData[1].q50);
        Assert.Equal(250, profiler.columnData[2].q75);
        Assert.Equal(3, profiler.columnData[0].max);
        Assert.Equal(10, profiler.columnData[1].min);
        Assert.Equal(200, profiler.columnData[2].mean);
        Assert.Equal(0, profiler.columnData[2].outlierLower);
        Assert.Equal(400, profiler.columnData[2].outlierUpper);
    }

    [Fact]
    public void Profiler_noOutliers()
    {
        var matrix = new double[,]
       {
            {1, 10, 100},
            {2, 20, 200},
            {3, 30, 300},
            {4, 40, 400}
       };
        var target = new double[] { 1, 2, 3, 4 };
        var profiler = new CsML.Util.Features.Profiler(matrix, target);
        Assert.True(profiler.NoOutliers(matrix));
    }

    [Fact]
    public void Profiler_outliers()
    {
        var matrix = new double[,]
       {
            {1, 10, 100},
            {2, 20, 200},
            {3, 30, 300},
            {100, 30, 3},
       };
        var target = new double[] { 1, 2, 3, 4 };
        var profiler = new CsML.Util.Features.Profiler(matrix, target);
        Assert.False(profiler.NoOutliers(matrix));
    }
}

public class Matrix
{
    [Fact]
    public void BestSplit()
    {
        var matrix = new double[,] {
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
        Assert.Equal(0.5, result.Item2); // best split
        Assert.Equal(0.05555555555555555, result.Item3); // best gain
    }

    [Fact]
    public void Equal()
    {
        var matrix1 = new double[,] { { 1, 1 }, { 2, 2 } };
        var matrix2 = new double[,] { { 1, 1 }, { 2, 2 } };
        var matrix3 = new double[,] { { 1, 1 }, { 3, 3 } };
        var matrix4 = new double[,] { { 1, 1 } };
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
        string strWorkPath = Directory
                                .GetParent(Environment.CurrentDirectory)!
                                .Parent!
                                .Parent!
                                .FullName;
        string inpuPath = Path.Combine(strWorkPath, "Data/iris.csv");
        double[,] matrix = CsML.Util.Matrix.FromCSV(
                                inpuPath, mapping, loadFromRow: 1);
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
    public void GetColumn()
    {
        var matrix = new double[,]{
            {0, 3, 6},
            {1, 4, 7},
            {2, 5, 8},
        };
        var result = CsML.Util.Matrix.GetColumn(matrix, 1);
        double[] expected = { 3, 4, 5 };
        Assert.True(result.SequenceEqual(expected));
    }

    [Fact]
    public void GetRow()
    {
        var matrix = new double[,]{
            {0, 3, 6},
            {1, 4, 7},
            {2, 5, 8},
        };
        var result = CsML.Util.Matrix.GetRow(matrix, 1);
        double[] expected = { 1, 4, 7 };
        Assert.True(result.SequenceEqual(expected));
    }

    [Fact]
    public void Split_splitpoint()
    {
        var matrix = new double[,]{
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
        var lhs = result.Item1;
        var rhs = result.Item2;
        var filter = result.Item3;
        Assert.True(CsML.Util.Matrix.Equal(expectedLhs, lhs));
        Assert.True(CsML.Util.Matrix.Equal(expectedRhs, rhs));
        Assert.False(CsML.Util.Matrix.Equal(lhs, rhs));
        Assert.True(expectedFilter.SequenceEqual(filter));
    }

    [Fact]
    public void Split_filter()
    {
        var matrix = new double[,] { { 1, 2 }, { 3, 4 }, { 5, 6 } };
        var filter = new bool[] { true, false, false };
        double[,] lhs, rhs;
        (lhs, rhs) = CsML.Util.Matrix.Split(matrix, filter);
        Assert.Equal(1, lhs.GetLength(0));
        Assert.Equal(2, lhs.GetLength(1));
        Assert.Equal(2, rhs.GetLength(0));
        Assert.Equal(2, rhs.GetLength(1));
        Assert.Equal(1, lhs[0, 0]);
        Assert.Equal(3, rhs[0, 0]);
        Assert.Equal(6, rhs[1, 1]);
    }
}

public class Statistics
{
    [Fact]
    public void Gini_strings()
    {
        string[] stringvals = { "a", "a", "a", "b", "b", "b" };
        double result = CsML.Util.Statistics.Gini(stringvals);
        Assert.Equal(0.5, result);
    }

    [Fact]
    public void Gini_ints()
    {
        int[] intvals = { 1, 1, 1, 2, 2, 1 };
        double result = CsML.Util.Statistics.Gini(intvals);
        Assert.Equal(0.4444444444444445, result);
    }

    [Fact]
    public void Gini_empty()
    {
        int[] emptyvals = new int[] { };
        double result = CsML.Util.Statistics.Gini(emptyvals);
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Gini_different()
    {
        int[] emptyvals = new int[] { 1, 2, 3 };
        double result = CsML.Util.Statistics.Gini(emptyvals);
        Assert.Equal(0.6666666666666665, result);
    }

    [Fact]
    public void OutlierBounds_upperskew()
    {
        double[] input = { 1, 2, 4, 2, 1, 100 };
        var result = CsML.Util.Statistics.OutlierBounds(input);
        Assert.Equal((-2.125, 6.875), result);
    }

    [Fact]
    public void OutlierBounds_lowerskew()
    {
        double[] input = { -100, 2, 4, 2, 1, 1 };
        var result = CsML.Util.Statistics.OutlierBounds(input);
        Assert.Equal((-0.5, 3.5), result);
    }

    [Fact]
    public void OutlierBounds_upperandlowerskew()
    {
        double[] input = { -100, 2, 4, 2, 1, 100 };
        var result = CsML.Util.Statistics.OutlierBounds(input);
        Assert.Equal((-2.125, 6.875), result);
    }

    [Fact]
    public void PercentileLinear_empty()
    {
        double result = CsML.Util.Statistics
                .PercentileLinear(System.Array.Empty<double>(), 0.5);
        Assert.Equal(0, result);
    }

    [Fact]
    public void PercentileLinear_same()
    {
        double[] input = { 1, 1, 1, 1, 1, };
        double result = CsML.Util.Statistics.PercentileLinear(input, 0.5);
        Assert.Equal(1, result);
        result = CsML.Util.Statistics.PercentileLinear(input, 0.25);
        Assert.Equal(1, result);
    }

    [Fact]
    public void PercentileLinear_precalculated()
    {
        double[] input = { 2, 6, 4, 8, 13, 22, 16, 40, 35, 48, 42 };
        double result = CsML.Util.Statistics.PercentileLinear(input, 0.5);
        Assert.Equal(16, result);
        result = CsML.Util.Statistics.PercentileLinear(input, 0.75);
        Assert.Equal(37.5, result);
        result = CsML.Util.Statistics.PercentileLinear(input, 0.25);
        Assert.Equal(7, result);
    }

    [Fact]
    public void RSquared_unadjusted()
    {
        var acts = new double[] { 2.0, 2.0, 4.0 };
        var preds = new double[] { 1.0, 2.0, 3.0 };
        var result = CsML.Util.Statistics.RSquared(acts, preds, null);
        Assert.Equal((0.25, 00), result);
    }

    [Fact]
    public void RSquared_adjusted()
    {
        var acts = new double[] { 2.0, 2.0, 4.0 };
        var preds = new double[] { 1.0, 2.0, 3.0 };
        var result = CsML.Util.Statistics.RSquared(acts, preds, 1);
        var n = acts.Length;
        var adjusted = 1.0 - (1.0 - 0.25) * ((n - 1.0) / (n - 1 - 1));
        Assert.Equal((0.25, adjusted), result);
    }

    [Fact]
    public void SSE()
    {
        var actuals = new double[] { 1, 2, 3, 4, 5 };
        var predicted = new double[] { 1, 3, 5, 4, 5 };
        var result = CsML.Util.Statistics.SSE(actuals, predicted);
        Assert.Equal(5, result);
    }

    [Fact]
    public void StdevP()
    {
        var input = new double[] { 1, 2, 3, 4 };
        Assert.Equal(1.118033988749895, CsML.Util.Statistics.StdevP(input));
    }

    [Fact]
    public void Variance()
    {
        var input = new double[] { 1, 2, 3, 4 };
        Assert.Equal(1.25, CsML.Util.Statistics.Variance(input));
    }
}

public class KFoldIterator
{
    [Fact]
    public void iterator_test_correctfilter()
    {
        CsML.Util.KFoldIterator iter = new CsML.Util.KFoldIterator(100, 10);
        Assert.Equal((0, 10), iter.foldIndeces[0]);
        Assert.Equal((30, 40), iter.foldIndeces[3]);
        Assert.Equal((90, 100), iter.foldIndeces[9]);
        int cntr = 1;
        foreach (bool[] actual in iter)
        {
            if (cntr == 1)
            {
                bool[] expected = Enumerable.Repeat(false, 10).ToArray()
                                    .Concat(Enumerable.Repeat(true, 90))
                                    .ToArray();
                Assert.True(actual.SequenceEqual(expected));
            }
            if (cntr == 3)
            {
                bool[] expected = Enumerable.Repeat(true, 20).ToArray()
                                .Concat(Enumerable.Repeat(false, 10)).ToArray()
                                .Concat(Enumerable.Repeat(true, 70)).ToArray();
                Assert.True(actual.SequenceEqual(expected));
            }
            if (cntr == 10)
            {
                bool[] expected = Enumerable.Repeat(true, 90).ToArray()
                                .Concat(Enumerable.Repeat(false, 10)).ToArray();
                Assert.True(actual.SequenceEqual(expected));
            }
            cntr++;
        }
    }

    [Fact]
    public void iterator_test_numiters()
    {
        CsML.Util.KFoldIterator iter = new CsML.Util.KFoldIterator(100, 10);
        int cntr = 0;
        foreach (bool[] f in iter)
        {
            cntr++;
        }
        Assert.Equal(10, cntr);
    }
}
