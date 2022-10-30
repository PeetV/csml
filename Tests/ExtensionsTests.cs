using Xunit;
using CsML.Extensions;

namespace Csml.Tests.Extensions;

public class IEnumerable
{

    [Fact]
    public void CumulativeSum()
    {
        double[] input = new double[] { 1, 2, 3 };
        double[] expected = new double[] { 1, 3, 6 };
        double[] result = input.CumulativeSum().ToArray();
        Assert.True(expected.SequenceEqual(result));
    }

}