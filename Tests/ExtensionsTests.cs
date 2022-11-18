using Xunit;
using CsML.Extensions;

namespace CsML.Tests.Extensions;

public class IEnumerable
{

    [Fact]
    public void CumulativeSum()
    {
        var input = new double[] { 1, 2, 3 };
        var expected = new double[] { 1, 3, 6 };
        double[] result = input.Cumulative().ToArray();
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void ElementCounts_int()
    {
        var vector = new double[] { 1, 1, 1, 2, 2 };
        var result = vector.ElementCounts();
        Assert.Equal(3, result[1]);
        Assert.Equal(2, result[2]);
    }

    [Fact]
    public void ElementCounts_string()
    {
        var vector = new string[] { "1", "1", "1", "2", "2" };
        var result = vector.ElementCounts();
        Assert.Equal(3, result["1"]);
        Assert.Equal(2, result["2"]);
    }

}