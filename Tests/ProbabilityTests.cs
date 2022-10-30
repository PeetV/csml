using Xunit;

namespace Csml.Tests.Probability;

public class Functions
{
    [Fact]
    public void And()
    {
        bool[] a = new bool[] { true, false, false, true, false };
        bool[] b = new bool[] { true, true, true, false, false };
        bool[] expected = new bool[] { true, false, false, false, false };
        bool[] result = CsML.Probability.Functions.And(a, b);
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void Conditional()
    {
        bool[] a = new bool[] { true, false, false, true, false };
        bool[] b = new bool[] { true, true, true, false, false };
        Assert.Equal(1.0 / 3.0, CsML.Probability.Functions.Conditional(a, b));
    }

    [Fact]
    public void Or()
    {
        bool[] a = new bool[] { true, false, false, true, false };
        bool[] b = new bool[] { true, true, true, false, false };
        bool[] expected = new bool[] { true, true, true, true, false };
        bool[] result = CsML.Probability.Functions.Or(a, b);
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void Probability()
    {
        bool[] a = new bool[] { true, false, false };
        Assert.Equal(1.0 / 3.0, CsML.Probability.Functions.Probability(a));
    }
}

public class Sample
{
    [Fact]
    public void ArrayWithoutReplacement()
    {
        string[] input = new string[] { "a", "b", "c" };
        var result = CsML.Probability.Sample.ArrayWithoutReplacement(input, 2);
        Assert.Equal(2, result.Length);
        Assert.True(result.All(x => input.Contains(x)));
        Assert.Equal(2, result.Distinct().ToArray().Length);
    }

    [Fact]
    public void RangeWithReplacement()
    {
        int[] result = CsML.Probability.Sample.RangeWithReplacement(0, 10, 3);
        Assert.Equal(3, result.Length);
        int[] possibleVals = Enumerable.Range(0, 10).ToArray();
        Assert.True(result.All(x => possibleVals.Contains(x)));
    }
}

public class Shuffle
{
    [Fact]
    public void Array()
    {
        string[] input = new string[] { "a", "b", "c" };
        var result = CsML.Probability.Shuffle.Array(input, inPlace: false);
        Assert.Equal(3, result.Length);
        Assert.True(result.All(x => input.Contains(x)));
    }
}

public class RandomClassifier
{
    [Fact]
    public void TrainPredict()
    {
        var cfier = new CsML.Probability.RandomClassifier<string>();
        string[] target = Enumerable.Repeat("a", 50)
                            .Concat(Enumerable.Repeat("b", 30))
                            .Concat(Enumerable.Repeat("c", 10))
                            .Concat(Enumerable.Repeat("d", 5))
                            .Concat(Enumerable.Repeat("e", 5))
                            .ToArray();
        Assert.Equal(100, target.Length);
        cfier.Train(new double[,] { }, target);
        string[] result = cfier.Predict(new double[1000, 1]);
        var counts = CsML.Util.Array.ElementCounts(result);
        Assert.Equal(1000, result.Length);
        Assert.InRange((double)counts["a"] / 1000.0, 0.45, 0.55);
        Assert.InRange((double)counts["b"] / 1000.0, 0.25, 0.35);
        Assert.InRange((double)counts["c"] / 1000.0, 0.05, 0.15);
        Assert.InRange((double)counts["d"] / 1000.0, 0.01, 0.1);
        Assert.InRange((double)counts["e"] / 1000.0, 0.01, 0.1);
    }
}

public class WeightedIndexSampler
{
    [Fact]
    public void SampleIndex()
    {
        string[] target = new string[] { "a", "b", "c", "d", "e" };
        double[] weights = new double[] { 50, 30, 10, 5, 5 };
        var wis = new CsML.Probability.WeightedIndexSampler<string>(target, weights);
        int[] result = wis.SampleIndex(1000);
        var counts = CsML.Util.Array.ElementCounts(result);
        Assert.Equal(new int[] { 0, 1, 2, 3, 4 }, counts.Keys.OrderBy(x => x).ToArray());
        Assert.InRange((double)counts[0] / 1000.0, 0.45, 0.55);
        Assert.InRange((double)counts[1] / 1000.0, 0.25, 0.35);
        Assert.InRange((double)counts[2] / 1000.0, 0.05, 0.15);
        Assert.InRange((double)counts[3] / 1000.0, 0.01, 0.1);
        Assert.InRange((double)counts[4] / 1000.0, 0.01, 0.1);
    }

    [Fact]
    public void SampleTarget()
    {
        string[] target = new string[] { "a", "b", "c", "d", "e" };
        double[] weights = new double[] { 50, 30, 10, 5, 5 };
        var wis = new CsML.Probability.WeightedIndexSampler<string>(target, weights);
        string[] result = wis.SampleTarget(1000);
        var counts = CsML.Util.Array.ElementCounts(result);
        Assert.InRange((double)counts["a"] / 1000.0, 0.45, 0.55);
        Assert.InRange((double)counts["b"] / 1000.0, 0.25, 0.35);
        Assert.InRange((double)counts["c"] / 1000.0, 0.05, 0.15);
        Assert.InRange((double)counts["d"] / 1000.0, 0.01, 0.1);
        Assert.InRange((double)counts["e"] / 1000.0, 0.01, 0.1);
    }
}