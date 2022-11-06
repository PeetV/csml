using Microsoft.Toolkit.HighPerformance;

using CsML.Extensions;

namespace CsML.Probability;

public class Distributions
{

    /// <summary>
    /// Calculate Binomial probabilities for a series of k values. The Binomial
    /// distribution is used to model the probability of a number of successes
    /// during a certain number of trials.
    /// </summary>
    /// <param name="n">
    /// Number of independent experiments, each asking a yes–no question, and 
    /// each with its own Boolean-valued outcome: success (with probability p).
    /// </param>
    /// <param name="ks">Arrays of k values representing number of successes.</param>
    /// <param name="p">Probability of experiment success.</param>
    /// <returns>
    /// An array of doubles representing the proability of each corresponding
    /// k values specified in the ks parameter.
    /// </returns>
    public static double[] Binomial(int n, int[] ks, double p)
    {
        double[] result = ks.Select(k => ProbabilityBinomial(n, k, p)).ToArray();
        return result;
    }

    /// <summary>
    /// Calculate a Binomial probability.
    /// </summary>
    /// <param name="n">
    /// Number of independent experiments, each asking a yes–no question, and 
    /// each with its own Boolean-valued outcome: success (with probability p).
    /// </param>
    /// <param name="k">Number of successes.</param>
    /// <param name="p">Probability of experiment success.</param>
    public static double ProbabilityBinomial(int n, int k, double p)
    {
        double bc = CsML.Probability.Functions.NChooseK(n, k);
        return bc * Math.Pow(p, (double)k) * Math.Pow(1.0 - p, (double)(n - k));
    }

    /// <summary>
    /// Calculate the probability of a value assuming a normal distribution
    /// defined by the mean and variance parameters.
    /// </summary>
    public static double ProbabilityNormal(double value, double mean, double variance)
    {
        return 1.0 /
               (Math.Sqrt(2.0 * Math.PI) * Math.Sqrt(variance)) *
               Math.Pow(Math.Exp(1.0), -(Math.Pow(value - mean, 2.0) / (2.0 * variance)));
    }
}

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

    /// <summary> Calculate the Binomial Coefficient.</summary>
    /// From https://stackoverflow.com/a/12983878/4285191.
    public static double NChooseK(int n, int k)
    {
        if (k > n) return 0;
        if (k > n - k) k = n - k;

        double val = 1;
        for (int rk = 1, rn = n - k + 1; rk <= k; ++rk, ++rn)
        {
            val = (rn) * val / (rk);
        }
        return val;
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
    /// Sample an array without replacement.
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
/// A naive Bayesian classifier (naive given assumption of column
/// independence and normal distribution of features).
/// </summary>
public class NaiveBayesClassifier<T>
    where T : IComparable<T>
{
    public Dictionary<T, double> classProbabilities;
    public Dictionary<int, Dictionary<T, (double, double)>> columnMeans;
    public int minColumns;

    public NaiveBayesClassifier()
    {
        classProbabilities = new Dictionary<T, double> { };
        columnMeans = new Dictionary<int, Dictionary<T, (double, double)>> { };
    }
    public void Train(double[,] matrix, T[] target)
    {
        int inputRecordCount = matrix.GetLength(0);
        int targetLength = target.Length;
        if (inputRecordCount == 0 | targetLength == 0)
            throw new ArgumentException("Empty input");
        if (inputRecordCount != targetLength)
            throw new ArgumentException("Inputs must be the same length");
        classProbabilities = new Dictionary<T, double> { };
        columnMeans = new Dictionary<int, Dictionary<T, (double, double)>> { };
        minColumns = matrix.GetLength(1);
        CalculateClassProbabilities(target);
        foreach (var colidx in Enumerable.Range(0, minColumns))
            CalculateColumnMeans(matrix, target, colidx);
    }

    public T[] Predict(double[,] matrix)
    {
        int inputRecordCount = matrix.GetLength(0);
        if (classProbabilities.Count == 0)
            throw new ArgumentException("Classifier not trained");
        if (matrix.GetLength(1) != minColumns)
            throw new ArgumentException("Tree trained on different number of columns");
        if (inputRecordCount == 0)
            throw new ArgumentException("Empty input");
        T[] result = new T[inputRecordCount];
        Span2D<double> matrixSpan = matrix;
        Dictionary<T, double> probs;
        Dictionary<T, (double, double)> colvals;
        double prob, mn, var;
        for (int i = 0; i < inputRecordCount; i++)
        {
            double[] row = matrixSpan.GetRow(i).ToArray();
            probs = classProbabilities.ToDictionary(e => e.Key, e => e.Value);
            for (int c = 0; c < minColumns; c++)
            {
                foreach (var classLabel in probs.Keys)
                {
                    if (columnMeans.ContainsKey(c))
                    {
                        colvals = columnMeans[c];
                        (mn, var) = colvals[classLabel];
                        prob = CsML.Probability.Distributions.ProbabilityNormal(row[c], mn, var);
                    }
                    else prob = 0.0001;
                    // Some underflow protection given values can get really small
                    if (probs[classLabel] < 2.2250738585072014e-50)
                        probs[classLabel] = 2.2250738585072014e-50;
                    else
                        probs[classLabel] = probs[classLabel] * prob;
                }
            }
            result[i] = probs.MaxBy(kvp => kvp.Value).Key;
        }
        return result;
    }

    private void CalculateClassProbabilities(T[] target)
    {
        int lenTarget = target.Length;
        var counts = target.ElementCounts();
        foreach (var key in counts.Keys)
            classProbabilities[key] = (double)counts[key] / (double)lenTarget;
    }

    private void CalculateColumnMeans(double[,] matrix, T[] target, int columnIndex)
    {
        Span2D<double> matrixSpan = matrix;
        double[] workingColumn = matrixSpan.GetColumn(columnIndex).ToArray();
        double[] values;
        double mean, variance;
        var valuesDict = new Dictionary<T, (double, double)> { };
        foreach (var classLabel in classProbabilities.Keys)
        {
            values = workingColumn.Zip(target)
                        .Where(x => x.Second.CompareTo(classLabel) == 0)
                        .Select(x => x.First)
                        .ToArray();
            mean = values.Average();
            variance = CsML.Util.Statistics.Variance(values);
            valuesDict[classLabel] = (mean, variance);
        }
        columnMeans[columnIndex] = valuesDict;
    }
}

/// <summary>
/// A Probability Mass Function for modelling discrete outcomes. Adapted from
/// Think Bayes by Allen B. Downey.
/// </summary>
public class ProbabilityMassFunction<T>
    where T : IComparable
{
    public Dictionary<T, double> table;

    // Overload the square-bracket operator to work on the table field
    public object this[T hypothesis]
    {
        get { return table[hypothesis]; }
        set { table[hypothesis] = (double)value; }
    }

    public T[] hypotheses { get { return table.Keys.OrderBy(x => x).ToArray(); } }

    public double[] probabilities
    {
        get
        {
            return table.Keys.Zip(table.Values)
                .OrderBy(x => x.First)
                .Select(x => x.Second)
                .ToArray();
        }
    }

    public (T, double)[] zipped
    {
        get
        {
            return table.Keys.Zip(table.Values)
                .OrderBy(x => x.First)
                .ToArray();
        }
    }

    /// <summary>
    /// Create an empty PMF.
    /// </summary>
    public ProbabilityMassFunction()
    {
        table = new Dictionary<T, double>();
    }

    /// <summary>
    /// Create a PMF from an array of hypotheses (each hypothesis is set to equal
    /// probability).
    /// </summary>
    /// <param name="hypotheses"></param>
    public ProbabilityMassFunction(T[] hypotheses)
    {
        table = new Dictionary<T, double>();
        foreach (T hypothesis in hypotheses)
            table[hypothesis] = 1;
        Normalise();
    }

    /// <summary>
    /// Create a PMF from a series of k values.
    /// </summary>
    /// <param name="n">
    /// Number of independent experiments, each asking a yes–no question, and 
    /// each with its own Boolean-valued outcome: success (with probability p).
    /// </param>
    /// <param name="ks">Arrays of k values representing number of successes.</param>
    /// <param name="p">Probability of experiment success.</param>
    public static ProbabilityMassFunction<int> FromBinomial(int n, int[] ks, double p)
    {
        ProbabilityMassFunction<int> intPMF = new ProbabilityMassFunction<int>();
        double[] probs = CsML.Probability.Distributions.Binomial(n, ks, p);
        for (int i = 0; i < ks.Length; i++)
            intPMF[ks[i]] = probs[i];
        return intPMF;
    }

    /// <summary>
    /// Get the hypothesis with the highest probability, together with the
    /// probability value.
    /// </summary>
    /// <returns>
    /// A tuple containing the hypothesis and corresponding probability.
    /// </returns>
    public (T, double) HighestProbability()
    {
        T mostProbablyHypothesis = table.MaxBy(kvp => kvp.Value).Key;
        return (mostProbablyHypothesis, table[mostProbablyHypothesis]);
    }

    /// <summary>
    /// Normalise the hypotheses table, making the probabilities add up to 1.
    /// </summary>
    public void Normalise()
    {
        double total = table.Values.Sum();
        foreach (T hypothesis in table.Keys)
        {
            table[hypothesis] /= total;
        }
    }

    /// <summary>
    /// Sum the probablities across a range of hypotheses. Include all values
    /// less than, or greater than range boundaries.
    /// </summary>
    /// <param name="lower">Lower range boundary. Can be null.</param>
    /// <param name="upper">Upper range boundary. Can be null.</param>
    /// <param name="includeLower">
    /// Inlude lower bounderay i.e. greater than or equal. Defaults to true.
    /// </param>
    /// <param name="includeUpper">
    /// Include the upper bounderay i.e. less than or equal. Defaults to false.
    /// </param>
    public double SumProbabilities(
        T? lower, T? upper, bool includeLower = true, bool includeUpper = false)
    {
        if (lower == null & upper == null)
            return 0.0;
        return this.zipped.Where((x) =>
        {
            // Both boundaries apply
            if (lower != null & upper != null & includeLower & includeUpper)
                return x.Item1.CompareTo(lower) >= 0 & x.Item1.CompareTo(upper) <= 0;
            if (lower != null & upper != null & !includeLower & includeUpper)
                return x.Item1.CompareTo(lower) > 0 & x.Item1.CompareTo(upper) <= 0;
            if (lower != null & upper != null & includeLower & !includeUpper)
                return x.Item1.CompareTo(lower) >= 0 & x.Item1.CompareTo(upper) < 0;
            if (lower != null & upper != null & !includeLower & !includeUpper)
                return x.Item1.CompareTo(lower) > 0 & x.Item1.CompareTo(upper) < 0;
            // Only lower boundary applies
            if (lower != null & upper == null & includeLower)
                return x.Item1.CompareTo(lower) >= 0;
            if (lower != null & upper == null & !includeLower)
                return x.Item1.CompareTo(lower) > 0;
            // Only upper boundary applies
            if (lower == null & upper != null & includeUpper)
                return x.Item1.CompareTo(upper) <= 0;
            if (lower == null & upper != null & !includeUpper)
                return x.Item1.CompareTo(upper) < 0;
            return false;
        }).Select(x => x.Item2).Sum();
    }

    /// <summary>
    /// Convert the PMF into a weighted random sampler, sampling the hypotheses
    /// using the probabilities as weights.
    /// </summary>
    public WeightedIndexSampler<T> ToSampler()
    {
        return new WeightedIndexSampler<T>(hypotheses, probabilities);
    }

    /// <summary>
    /// Update the hypotheses table (priors, P(H)) with likelihoods (P(D|H).
    /// </summary>
    /// <param name="likelihoods">A dictionary containing the likelihood for each outcome</param>
    public void Update(Dictionary<T, double> likelihoods)
    {
        if (!table.Keys.SequenceEqual(likelihoods.Keys))
            throw new ArgumentException("Input needs same keys as outcome table");
        foreach (T hypothesis in table.Keys)
            table[hypothesis] *= likelihoods[hypothesis];
    }

    /// <summary>
    /// Update the hypotheses table (priors, P(H)) with likelihoods (P(D|H).
    /// </summary>
    /// <param name="likelihoods">An array of likelihood values in order of hypotheses sorted</param>
    public void Update(double[] likelihoods)
    {
        if (likelihoods.Length != table.Count)
            throw new ArgumentException("Input does not have same number of values as PMF hypotheses");
        foreach ((T, double) pair in this.hypotheses.Zip(likelihoods))
            table[pair.Item1] *= pair.Item2;
    }

}

/// <summary>
/// A classifier making a weighted random guess from potential class labels
/// for benchmarking purposes.
/// </summary>
public class RandomClassifier<T>
    where T : notnull
{
    public T[] classes;
    public double[] weights;

    public RandomClassifier()
    {
        classes = new T[] { };
        weights = new double[] { };
    }

    /// <summary>
    /// Train the classifier by calculating class label weightings from the
    /// target parameter array.
    /// </summary>
    public void Train(double[,] matrix, T[] target)
    {
        var counts = target.ElementCounts();
        classes = counts.Keys.OrderBy(x => x).ToArray();
        weights = new double[classes.Length];
        T key;
        for (int i = 0; i < classes.Length; i++)
        {
            key = classes[i];
            weights[i] = counts[key];
        }
    }
    /// <summary>
    /// Randomly choose from class labels applying weightings to emulate
    /// the distribution of the labels identified in train method.
    /// </summary>
    public T[] Predict(double[,] matrix)
    {
        var sampler = new WeightedIndexSampler<T>(classes, weights);
        return sampler.SampleTarget(matrix.GetLength(0));
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
        this.weights = this.weights
                    .Select(x => x / weightsSum)
                    .CumulativeSum()
                    .ToArray();
        random = new Random();
    }

    /// <summary>
    /// Sample a specified number of times, returning index numbers in
    /// the target array set at construction. Use SampleTarget to sample
    /// the target array directly.
    /// </summary>
    /// <param name="count">The number of samples to draw.</param>
    /// <returns>An array of index values.</returns>
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

    /// <summary>
    /// Sample the target array a specified number of times.
    /// </summary>
    /// <param name="count">The number of samples to draw.</param>
    /// <returns>An array of sample values.</returns>
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