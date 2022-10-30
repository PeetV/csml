namespace CsML.Extensions;

public static class IEnumerable
{

    /// <summary>
    /// Turn a double enumerable into cumulative sums.
    /// </summary>
    public static IEnumerable<double> CumulativeSum(this IEnumerable<double> sequence)
    {
        double sum = 0;
        foreach (var item in sequence)
        {
            sum += item;
            yield return sum;
        }
    }
}