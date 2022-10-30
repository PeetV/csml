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
        Assert.Equal(counts.Keys.ToArray(), new int[] { 0, 1, 2, 3, 4 });
        Assert.InRange(counts[0] / 1000, 0, 1);
    }
}