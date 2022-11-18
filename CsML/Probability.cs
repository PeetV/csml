using Microsoft.Toolkit.HighPerformance;

using CsML.Extensions;

namespace CsML.Probability;

/// <summary>
/// A collection of functions dealing with or to generate distributions.
/// </summary>
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
    /// <param name="ks">
    /// Array of k values representing number of successes.
    /// </param>
    /// <param name="p">Probability of experiment success.</param>
    /// <returns>
    /// An array of doubles representing the proability of each corresponding
    /// k values specified in the ks parameter.
    /// </returns>
    public static double[] Binomial(int n, int[] ks, double p)
    {
        double[] result = ks
            .Select(k => ProbabilityBinomial(n, k, p))
            .ToArray();
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
    public static double ProbabilityNormal(
        double value, double mean, double variance)
    {
        return 1.0 /
               (Math.Sqrt(2.0 * Math.PI) * Math.Sqrt(variance)) *
               Math.Pow(Math.Exp(1.0),
                        -(Math.Pow(value - mean, 2.0) / (2.0 * variance)));
    }
}

/// <summary>
/// A general collection of functions for probability calculations.
/// </summary>
public class Functions
{
    /// <summary>
    /// Merge two boolean arrays using an And operator on elements.
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
        bool[] cond = a.Zip(b)
            .Where(x => x.Second)
            .Select(x => x.First)
            .ToArray();
        return CsML.Probability.Functions.Probability(cond);
    }

    /// <summary>
    /// Calculate the Binomial Coefficient.
    /// </summary>
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
    /// Merge two boolean arrays using an Or operator on elements.
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

/// <summary>A collection of functions for random sampling.</summary>
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
    /// <param name="minValue">Range starting value.</param>
    /// <param name="maxValue">
    /// Range stopping value (not included in sample).
    /// </param>
    /// <param name="count">The number of sample items.</param>
    public static int[] RangeWithReplacement(
        int minValue, int maxValue, int count)
    {
        var random = new Random();
        return Enumerable.Range(0, count)
                         .Select(_ => random.Next(minValue, maxValue))
                         .ToArray();
    }
}

/// <summary>
/// A collection of functions for shuffling things randomly.
/// </summary>
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
        var random = new Random();
        T[] result = inPlace ? input : (T[])input.Clone();
        return result.OrderBy(x => random.Next()).ToArray();
    }
}

/// <summary>Count occurences of T.</summary>
public class Counter<T> where T : notnull
{
    /// <summary>
    /// A dictionary containing counts.
    /// </summary>
    public Dictionary<T, int> counts;

    /// <summary>
    /// Overload the square-bracket operator to work on the table field.
    /// </summary>
    public object this[T target]
    {
        get { return counts[target]; }
        set { counts[target] = (int)value; }
    }

    /// <summary>Get the sum of individual counts.</summary>
    public int Total { get { return counts.Values.Sum(); } }

    /// <summary>Create an empty counter.</summary>
    public Counter()
    {
        counts = new Dictionary<T, int>();
    }

    /// <summary>Reset the counter to empty.</summary>
    public void Clear()
    {
        counts.Clear();
    }

    /// <summary>Increment target count.</summary>
    public void Increment(T target, int increment = 1)
    {
        if (counts.ContainsKey(target))
            counts[target] += increment;
        else counts[target] = increment;
    }

    /// <summary>Increment multiple target counts.</summary>
    public void Increment(T[] targets, int increment = 1)
    {
        foreach (T target in targets)
            Increment(target, increment: increment);
    }

    /// <summary>Get the highest count key, value pair.</summary>
    public (T, int) Max()
    {
        var pair = counts.MaxBy(kvp => kvp.Value);
        return (pair.Key, pair.Value);
    }

    /// <summary>Get the item (key) with highest count.</summary>
    public T MaxKey()
    {
        return counts.MaxBy(kvp => kvp.Value).Key;
    }
}

/// <summary>
/// A naive Bayesian classifier (naive given assumption of column
/// independence and normal distribution of features).
/// </summary>
public class NaiveBayesClassifier<T>
    where T : IComparable<T>
{
    /// <summary>
    /// The probability of each class, estimated from data in model training.
    /// </summary>
    public Dictionary<T, double> classProbabilities;

    /// <summary>
    /// Mean values calculated from each column. The outer dictionary maps
    /// column indices to inner dictionaries. Inner dictionaries contain
    /// tuples with mean and variance as dictionary values, and class labels
    /// as keys (i.e. for column values related to each class label).
    /// </summary>
    public Dictionary<int, Dictionary<T, (double, double)>> columnMeans;

    /// <summary>
    /// The number of matrix columns the model was trained on.
    /// </summary>
    public int minColumns;

    /// <summary>Create an untrained Naive Bayes classifier.</summary>
    public NaiveBayesClassifier()
    {
        classProbabilities = new Dictionary<T, double> { };
        columnMeans = new Dictionary<int, Dictionary<T, (double, double)>> { };
    }

    /// <summary>
    /// Train the model.
    /// </summary>
    /// <param name="matrix">The features to train the model on.</param>
    /// <param name="target">The target vector to train on.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length or empty.
    /// </exception> 
    public void Train(double[,] matrix, T[] target)
    {
        int inputRecordCount = matrix.GetLength(0);
        int targetLength = target.Length;
        if (inputRecordCount == 0 | targetLength == 0)
            throw new ArgumentException(CsML.Errors.Messages.E1);
        if (inputRecordCount != targetLength)
            throw new ArgumentException(CsML.Errors.Messages.E2);
        classProbabilities = new Dictionary<T, double> { };
        columnMeans = new Dictionary<int, Dictionary<T, (double, double)>> { };
        minColumns = matrix.GetLength(1);
        CalculateClassProbabilities(target);
        foreach (var colidx in Enumerable.Range(0, minColumns))
            CalculateColumnMeans(matrix, target, colidx);
    }

    /// <summary>
    /// Make predictions using the model.
    /// </summary>
    /// <param name="matrix">New data to infer predictions from.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if input empty, or if the model has not been trained or if it
    /// has been trained on a different number of columns.
    /// </exception>
    public T[] Predict(double[,] matrix)
    {
        int inputRecordCount = matrix.GetLength(0);
        if (inputRecordCount == 0)
            throw new ArgumentException(CsML.Errors.Messages.E1);
        if (classProbabilities.Count == 0)
            throw new ArgumentException(CsML.Errors.Messages.E3);
        if (matrix.GetLength(1) != minColumns)
            throw new ArgumentException(CsML.Errors.Messages.E4);
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
                        prob = CsML.Probability.Distributions.ProbabilityNormal(
                            row[c], mn, var);
                    }
                    else prob = 0.0001;
                    // Some underflow protection given values can get really
                    // small
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

    private void CalculateColumnMeans(
        double[,] matrix, T[] target, int columnIndex)
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
    /// <summary>
    /// A dictionary containing hypotheses as keys and corresponding
    /// probabilities as values.
    /// </summary>
    public Dictionary<T, double> table;

    /// <summary>
    /// Overload the square-bracket operator to work on the table field.
    /// </summary>
    public object this[T hypothesis]
    {
        get { return table[hypothesis]; }
        set { table[hypothesis] = (double)value; }
    }

    /// <summary>Hypotheses sorted.</summary>
    public T[] hypotheses
    {
        get { return table.Keys.OrderBy(x => x).ToArray(); }
    }

    /// <summary>Probabilities in order of hypotheses sorted.</summary>
    public double[] probabilities
    {
        get
        {
            return table.Keys
                .Zip(table.Values)
                .OrderBy(x => x.First)
                .Select(x => x.Second)
                .ToArray();
        }
    }

    /// <summary>
    /// Array of tuples containing each hypotheses and corresponding
    /// probability, in order of hypotheses sorted.
    /// </summary>
    public (T, double)[] zipped
    {
        get
        {
            return table.Keys
                .Zip(table.Values)
                .OrderBy(x => x.First)
                .ToArray();
        }
    }

    /// <summary>Create an empty PMF.</summary>
    public ProbabilityMassFunction()
    {
        table = new Dictionary<T, double>();
    }

    /// <summary>
    /// Create a PMF from an array of hypotheses (each hypothesis is set to
    /// equal probability).
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
    /// Create a Binomial distribution PMF from a series of k values
    /// and p value.
    /// </summary>
    /// <param name="n">
    /// Number of independent experiments, each asking a yes–no question, and 
    /// each with its own Boolean-valued outcome: success (with probability p).
    /// </param>
    /// <param name="ks">
    /// Array of k values representing number of successes.
    /// </param>
    /// <param name="p">Probability of experiment success.</param>
    public static ProbabilityMassFunction<int> FromBinomial(
        int n, int[] ks, double p)
    {
        var intPMF = new ProbabilityMassFunction<int>();
        double[] probs = CsML.Probability.Distributions.Binomial(n, ks, p);
        for (int i = 0; i < ks.Length; i++)
            intPMF[ks[i]] = probs[i];
        intPMF.Normalise();
        return intPMF;
    }

    /// <summary>
    /// Create a Normal distribution PMF from a series of hypotheses values,
    /// mean and variance.
    /// </summary>
    public static ProbabilityMassFunction<double> FromNormal(
        double[] hypotheses, double mean, double variance)
    {
        var doublePMF = new ProbabilityMassFunction<double>();
        for (int i = 0; i < hypotheses.Length; i++)
            doublePMF[hypotheses[i]] = CsML.Probability.Distributions
                .ProbabilityNormal(hypotheses[i], mean, variance);
        doublePMF.Normalise();
        return doublePMF;
    }

    /// <summary>Add two PMFs.</summary>
    /// <returns>A PMF with hypotheses summed.</returns>
    /// <param name="pmf">A PMF with hypotheses double type.</param>
    /// <exception cref="System.ArithmeticException">
    /// Throws an exception if the hypotheses type is not double.
    /// </exception>
    public ProbabilityMassFunction<double> Add(
        ProbabilityMassFunction<double> pmf)
    {
        var newPmf = new ProbabilityMassFunction<double>();
        foreach (var key1 in this.table.Keys)
        {
            foreach (var key2 in pmf.table.Keys)
            {
                double sumHypos = Convert.ToDouble(key1) +
                    Convert.ToDouble(key2);
                if (!newPmf.table.ContainsKey(sumHypos))
                    newPmf.table[sumHypos] = this.table[key1] * pmf.table[key2];
                else
                    newPmf.table[sumHypos] = newPmf.table[sumHypos] +
                        (this.table[key1] * pmf.table[key2]);
            }
        }
        newPmf.Normalise();
        return newPmf;
    }

    /// <summary>Add two PMFs.</summary>
    /// <returns>A PMF with hypotheses summed.</returns>
    /// <param name="pmf">A PMF with hypotheses double type.</param>
    /// <exception cref="System.ArithmeticException">
    /// Throws an exception if the hypotheses type is not double.
    /// </exception>
    public ProbabilityMassFunction<int> Add(
        ProbabilityMassFunction<int> pmf)
    {
        var newPmf = new ProbabilityMassFunction<int>();
        foreach (var key1 in this.table.Keys)
        {
            foreach (var key2 in pmf.table.Keys)
            {
                int sumHypos = Convert.ToInt32(key1) +
                    Convert.ToInt32(key2);
                if (!newPmf.table.ContainsKey(sumHypos))
                    newPmf.table[sumHypos] = this.table[key1] * pmf.table[key2];
                else
                    newPmf.table[sumHypos] = newPmf.table[sumHypos] +
                        (this.table[key1] * pmf.table[key2]);
            }
        }
        newPmf.Normalise();
        return newPmf;
    }

    /// <summary>
    /// Get the hypothesis with the highest probability, together with the
    /// probability value.
    /// </summary>
    /// <returns>
    /// A tuple containing the hypothesis and corresponding probability.
    /// </returns>
    public (T, double) Max()
    {
        T mostProbablyHypothesis = table.MaxBy(kvp => kvp.Value).Key;
        return (mostProbablyHypothesis, table[mostProbablyHypothesis]);
    }

    /// <summary>
    /// Calculate the mean of the PMF.
    /// </summary>
    /// <exception cref="System.ArithmeticException">
    /// Throws an exception if the hypotheses type is not double.
    /// </exception>
    public double Mean()
    {
        double result = 0.0;
        foreach (var pair in zipped)
            result += Convert.ToDouble(pair.Item1) * pair.Item2;
        return result;
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
                return x.Item1.CompareTo(lower) >= 0 &
                       x.Item1.CompareTo(upper) <= 0;
            if (lower != null & upper != null & !includeLower & includeUpper)
                return x.Item1.CompareTo(lower) >
                       0 & x.Item1.CompareTo(upper) <= 0;
            if (lower != null & upper != null & includeLower & !includeUpper)
                return x.Item1.CompareTo(lower) >=
                       0 & x.Item1.CompareTo(upper) < 0;
            if (lower != null & upper != null & !includeLower & !includeUpper)
                return x.Item1.CompareTo(lower) >
                       0 & x.Item1.CompareTo(upper) < 0;
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
    /// <param name="likelihoods">
    /// A dictionary containing the likelihood for each outcome.
    /// </param>
    public void Update(Dictionary<T, double> likelihoods)
    {
        foreach (T hypothesis in table.Keys)
            table[hypothesis] *= likelihoods[hypothesis];
    }

    /// <summary>
    /// Update the hypotheses table (priors, P(H)) with likelihoods (P(D|H).
    /// </summary>
    /// <param name="likelihoods">
    /// An array of likelihood values in order ofhypotheses sorted.
    /// </param>
    public void Update(double[] likelihoods)
    {
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
    /// <summary>
    /// The distinct class labels from the target vector.
    /// </summary>
    public T[] classes;

    /// <summary>
    /// Weights to apply to class labels. As the size of the sample increases,
    /// class proportions will get closer to the weights.
    /// </summary>
    public double[] weights;

    /// <summary>Create an untrained model.</summary>
    public RandomClassifier()
    {
        classes = new T[] { };
        weights = new double[] { };
    }

    /// <summary>
    /// Train the model.
    /// </summary>
    /// <param name="matrix">The features to train the model on.</param>
    /// <param name="target">The target vector to train on.</param>
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
    /// Make predictions using the model.
    /// </summary>
    /// <param name="matrix">New data to infer predictions from.</param>
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
    private readonly T[] _target;
    private readonly double[] _weights;
    private readonly Random _random;

    /// <summary>
    /// Create a new sampler.
    /// </summary>
    /// <param name="target">The array to draw samples from.</param>
    /// <param name="weights">
    /// The corresponding weights to apply when sampling. As the size of the
    /// sample increases the proportion will become closer to the weights
    /// proportions.
    /// </param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length or empty.
    /// </exception>
    public WeightedIndexSampler(T[] target, double[] weights)
    {
        if (target.Length == 0 | weights.Length == 0)
            throw new ArgumentException(CsML.Errors.Messages.E1);
        if (target.Length != weights.Length)
            throw new ArgumentException(CsML.Errors.Messages.E2);
        this._target = target;
        this._weights = (double[])weights.Clone();
        double weightsSum = weights.Sum();
        this._weights = this._weights
                    .Select(x => x / weightsSum)
                    .Cumulative()
                    .ToArray();
        _random = new Random();
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
        var result = new int[count];
        for (int i = 0; i < count; i++)
        {
            randNum = _random.NextDouble();
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
        var result = new T[count];
        for (int i = 0; i < count; i++)
        {
            randNum = _random.NextDouble();
            result[i] = _target[IndexAtCumVal(randNum)];
        }
        return result;
    }

    private int IndexAtCumVal(double randNum)
    {
        for (int i = 0; i < _weights.Length; i++)
        {
            if (randNum <= _weights[i])
                return i;

        }
        return _weights.Length - 1;
    }
}