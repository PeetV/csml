using Xunit;

namespace Csml.Tests.Probability;

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