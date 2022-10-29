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
}