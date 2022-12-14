using Xunit;
using CsML.Extensions;

namespace CsML.Tests.Extensions;

public class IEnumerable
{

    [Fact]
    public void CumulativeSum_ints()
    {
        var input = new int[] { 1, 2, 3 };
        var expected = new int[] { 1, 3, 6 };
        int[] result = input.Cumulative().ToArray();
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void CumulativeSum_doubles()
    {
        var input = new double[] { 1.1, 2.2, 3.3 };
        var expected = new double[] { 1.1, 3.3, 6.6 };
        double[] result = input.Cumulative().ToArray();
        Assert.True(Math.Abs(result[0] - expected[0]) < 0.00000001);
        Assert.True(Math.Abs(result[1] - expected[1]) < 0.00000001);
        Assert.True(Math.Abs(result[2] - expected[2]) < 0.00000001);
    }

    [Fact]
    public void CumulativeSum_empty()
    {
        var input = Array.Empty<double>();
        double[] result = input.Cumulative().ToArray();
        Assert.Empty(result);
    }

    [Fact]
    public void Delimted_ints()
    {
        int[] x = { 1, 2, 3, 4, 5 };
        Assert.Equal("1,2,3,4,5", x.Delimited());
        Assert.Equal("1|2|3|4|5", x.Delimited(delimiter: "|"));
    }

    [Fact]
    public void Delimted_empty()
    {
        int[] x = Array.Empty<int>();
        Assert.Equal("", x.Delimited());
    }

    [Fact]
    public void ElementCounts_doubles()
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

    [Fact]
    public void Product_ints()
    {
        var vector = new int[] { 1, 2, 3 };
        var result = vector.Product();
        Assert.Equal(6, result);
    }

    [Fact]
    public void Product_empty()
    {
        var vector = Array.Empty<int>();;
        var result = vector.Product();
        Assert.Equal(0, result);
    }

    [Fact]
    public void Split_doubles()
    {
        var vector = new double[] { 1, 1, 1, 2, 2 };
        var filter = new bool[] { true, true, true, false, false };
        double[] lhs, rhs;
        (lhs, rhs) = vector.Split(filter);
        Assert.Equal(3, lhs.Sum());
        Assert.Equal(4, rhs.Sum());
    }

    [Fact]
    public void Split_empty()
    {
        var vector = new double[] { };
        var filter = new bool[] { };
        double[] lhs, rhs;
        (lhs, rhs) = vector.Split(filter);
        Assert.Empty(lhs);
        Assert.Empty(rhs);
    }

}