namespace CsML.Extensions;

/// <summary>
/// Extensions on IEnumerable adding functionality or improving readability.
/// </summary>
public static class IEnumerable
{

    /// <summary>
    /// Turn a double enumerable into cumulative sums. [1, 2, 3 ...] becomes
    /// [1, 3, 6 ...].
    /// </summary>
    public static IEnumerable<double> Cumulative(
        this IEnumerable<double> sequence)
    {
        double sum = 0;
        foreach (var item in sequence)
        {
            sum += item;
            yield return sum;
        }
    }

    /// <summary>Count ocurrences of each array element.</summary>
    /// <returns>
    /// A dictionary containing distinct array elements as keys and counts
    /// values.</returns>
    public static Dictionary<T, int> ElementCounts<T>(
        this IEnumerable<T> sequence)
        where T : notnull
    {
        return CsML.Util.Array.ElementCounts<T>(sequence.ToArray());
    }
}