namespace CsML.Probability;

public class Sample
{
    /// <summary>
    /// Sample an array withuot replacement.
    /// </summary>
    /// <param name="input">The array to sample from.</param>
    /// <param name="count">The number of sample items.</param>
    public static T[] ArrayWithoutReplacement<T>(T[] input, int count)
    {
        T[] working = CsML.Probability.Shuffle.Array(input, inPlace: false);
        return working[0..count];
    }

    /// <summary>
    /// Sample integers in a range with replacement.
    /// </summary>
    /// <param name="minvalue">Range starting value.</param>
    /// <param name="maxValue">Range stopping value (not included in sample).</param>
    /// <param name="count">The number of sample items.</param>
    public static int[] RangeWithReplacement(int minValue, int maxValue, int count)
    {
        Random random = new Random();
        return Enumerable.Range(0, count)
                         .Select(_ => random.Next(minValue, maxValue))
                         .ToArray();
    }
}
public class Shuffle
{
    /// <summary>
    /// Shuffle an array into a random order.
    /// </summary>
    /// <param name="input">The array to shuffle.</param>
    /// <param name="inPlace">
    /// Shuffle input array if true. Otherwise return a shuffled clone.
    /// </param>
    public static T[] Array<T>(T[] input, bool inPlace = false)
    {
        Random random = new Random();
        T[] result = inPlace ? input : (T[])input.Clone();
        return result.OrderBy(x => random.Next()).ToArray();
    }
}