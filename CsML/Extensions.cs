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

    /// <summary>
    /// Count of ocurrences of each array element.
    /// </summary>
    public static Dictionary<T, int> ElementCounts<T>(this IEnumerable<T> sequence)
        where T : notnull
    {
        return CsML.Util.Array.ElementCounts<T>(sequence.ToArray());
    }
}