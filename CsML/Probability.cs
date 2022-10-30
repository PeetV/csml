using CsML.Extensions;

namespace CsML.Probability;

public class Functions
{
    /// <summary>
    /// Merge two boolean arrays using an & operator on elements.
    /// </summary>
    public static bool[] And(bool[] a, bool[] b)
    {
        return a.Zip(b).Select(x => x.First & x.Second).ToArray();
    }

    /// <summary>
    /// Compute probability of a, conditioned on b.
    /// </summary>
    public static double Conditional(bool[] a, bool[] b)
    {
        bool[] cond = a.Zip(b).Where(x => x.Second).Select(x => x.First).ToArray();
        return CsML.Probability.Functions.Probability(cond);
    }

    /// <summary>
    /// Merge two boolean arrays using an | operator on elements.
    /// </summary>
    public static bool[] Or(bool[] a, bool[] b)
    {
        return a.Zip(b).Select(x => x.First | x.Second).ToArray();
    }

    /// <summary>
    /// Compute a probability from a boolean array.
    /// </summary>
    public static double Probability(bool[] a)
    {
        return a.Select(x => x ? 1 : 0).Sum() / (double)a.Length;
    }
}

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

/// <summary>
/// Draw a weighted sample of class labels or array index.
/// </summary>
public class WeightedIndexSampler<T>
{
    private T[] target;
    private double[] weights;
    private Random random;

    public WeightedIndexSampler(T[] target, double[] weights)
    {
        if (target.Length != weights.Length)
            throw new ArgumentException("Inputs must be same length");
        if (target.Length == 0)
            throw new ArgumentException("Empty input");
        this.target = target;
        this.weights = (double[])weights.Clone();
        double weightsSum = weights.Sum();
        this.weights = this.weights.Select(x => x / weightsSum)
                    .CumulativeSum()
                    .ToArray();
        random = new Random();
    }

    public int[] SampleIndex(int count)
    {
        double randNum;
        int[] result = new int[count];
        for (int i = 0; i < count; i++)
        {
            randNum = random.NextDouble();
            result[i] = IndexAtCumVal(randNum);
        }
        return result;
    }

    public T[] SampleTarget(int count)
    {
        double randNum;
        T[] result = new T[count];
        for (int i = 0; i < count; i++)
        {
            randNum = random.NextDouble();
            result[i] = target[IndexAtCumVal(randNum)];
        }
        return result;
    }

    private int IndexAtCumVal(double randNum)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            if (randNum <= weights[i])
                return i;

        }
        return weights.Length - 1;
    }
}