using Microsoft.Toolkit.HighPerformance;
using Xunit;
using CsML.Extensions;

namespace Csml.Tests.Probability;

public class Distributions
{
    [Fact]
    public void Binomial()
    {
        int[] ks = new int[] { 0, 1, 2 };
        double[] result = CsML.Probability.Distributions.Binomial(2, ks, 0.5);
        double[] expected = new double[] { 0.25, 0.5, 0.25 };
        Assert.Equal(expected[0], result[0]);
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void ProbabilityBinomial()
    {
        double result = CsML.Probability.Distributions
                            .ProbabilityBinomial(2, 1, 0.5);
        Assert.Equal(0.5, result);
    }

    [Fact]
    public void ProbabilityBinomial_kGreaterThanN()
    {
        double result = CsML.Probability.Distributions
                            .ProbabilityBinomial(9, 10, 0.5);
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void ProbabilityNormal()
    {
        double result = CsML.Probability.Distributions
                            .ProbabilityNormal(120, 110, 2975);
        Assert.Equal(0.007192295359419549, result);
    }
}

public class Functions
{
    [Fact]
    public void And()
    {
        bool[] a = new bool[] { true, false, false, true, false };
        bool[] b = new bool[] { true, true, true, false, false };
        bool[] expected = new bool[] { true, false, false, false, false };
        bool[] result = CsML.Probability.Functions.And(a, b);
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void Conditional()
    {
        bool[] a = new bool[] { true, false, false, true, false };
        bool[] b = new bool[] { true, true, true, false, false };
        Assert.Equal(1.0 / 3.0, CsML.Probability.Functions.Conditional(a, b));
    }

    [Fact]
    public void NChooseK()
    {
        double result = CsML.Probability.Functions.NChooseK(4, 4);
        Assert.Equal(1.0, result);
        result = CsML.Probability.Functions.NChooseK(9, 4);
        Assert.Equal(126.0, result);
        result = CsML.Probability.Functions.NChooseK(52, 5);
        Assert.Equal(2598960.0, result);
    }

    [Fact]
    public void Or()
    {
        bool[] a = new bool[] { true, false, false, true, false };
        bool[] b = new bool[] { true, true, true, false, false };
        bool[] expected = new bool[] { true, true, true, true, false };
        bool[] result = CsML.Probability.Functions.Or(a, b);
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void Probability()
    {
        bool[] a = new bool[] { true, false, false };
        Assert.Equal(1.0 / 3.0, CsML.Probability.Functions.Probability(a));
    }
}

public class Sample
{
    [Fact]
    public void ArrayWithoutReplacement()
    {
        string[] input = new string[] { "a", "b", "c" };
        var result = CsML.Probability.Sample.ArrayWithoutReplacement(input, 2);
        Assert.Equal(2, result.Length);
        Assert.True(result.All(x => input.Contains(x)));
        Assert.Equal(2, result.Distinct().ToArray().Length);
    }

    [Fact]
    public void RangeWithReplacement()
    {
        int[] result = CsML.Probability.Sample.RangeWithReplacement(0, 10, 3);
        Assert.Equal(3, result.Length);
        int[] possibleVals = Enumerable.Range(0, 10).ToArray();
        Assert.True(result.All(x => possibleVals.Contains(x)));
    }
}

public class Shuffle
{
    [Fact]
    public void Array()
    {
        string[] input = new string[] { "a", "b", "c" };
        var result = CsML.Probability.Shuffle.Array(input, inPlace: false);
        Assert.Equal(3, result.Length);
        Assert.True(result.All(x => input.Contains(x)));
        input = new string[] { "a", "b", "c" };
        result = CsML.Probability.Shuffle.Array(input, inPlace: true);
        Assert.Equal(3, result.Length);
        Assert.True(result.All(x => input.Contains(x)));
    }
}

public class Counter
{
    [Fact]
    public void Clear()
    {
        var counter = new CsML.Probability.Counter<string>();
        counter.Increment(new string[] { "a", "a", "b" });
        Assert.False(counter.counts.Count == 0);
        counter.Clear();
        Assert.True(counter.counts.Count == 0);
    }

    [Fact]
    public void Increment_single()
    {
        var counter = new CsML.Probability.Counter<string>();
        counter.Increment("a");
        counter.Increment("a");
        counter.Increment("b");
        Assert.Equal(2, counter["a"]);
        Assert.Equal(1, counter["b"]);
        Assert.Equal(3, counter.Total);
    }

    [Fact]
    public void Increment_multiple()
    {
        var counter = new CsML.Probability.Counter<string>();
        counter.Increment(new string[] { "a", "a", "b" });
        Assert.Equal(2, counter["a"]);
        Assert.Equal(1, counter["b"]);
        Assert.Equal(3, counter.Total);
    }

    [Fact]
    public void Max()
    {
        var counter = new CsML.Probability.Counter<string>();
        counter.Increment(new string[] { "a", "a", "b" });
        Assert.Equal(("a", 2), counter.Max());
    }

    [Fact]
    public void MaxKey()
    {
        var counter = new CsML.Probability.Counter<string>();
        counter.Increment(new string[] { "a", "a", "b" });
        Assert.Equal("a", counter.MaxKey());
    }
}

public class NaiveBayesClassifier
{
    [Fact]
    public void Train_columnMeans()
    {
        double[,] matrix = new double[,]
        {
            {1, 3}, {2, 4}, {3, 5}, {4, 6}, {5, 7}
        };
        double[] target = new double[] { 0, 0, 0, 1, 1 };
        var nbc = new CsML.Probability.NaiveBayesClassifier<double>();
        nbc.Train(matrix, target);
        var col0 = nbc.columnMeans[0];
        var col1 = nbc.columnMeans[1];
        Assert.Equal(
            col0[0],
            (2.0, CsML.Util.Statistics.Variance(new double[] { 1, 2, 3 }))
        );
        Assert.Equal(
            col0[1],
            (new double[] { 4, 5 }.Average(),
             CsML.Util.Statistics.Variance(new double[] { 4, 5 }))
        );
        Assert.Equal(
            col1[0],
            (new double[] { 3, 4, 5 }.Average(),
            CsML.Util.Statistics.Variance(new double[] { 3, 4, 5 }))
        );
        Assert.Equal(
            col1[1],
            (new double[] { 6, 7 }.Average(),
            CsML.Util.Statistics.Variance(new double[] { 6, 7 }))
        );
    }

    [Fact]
    public void Train_classProbabilities()
    {
        double[,] matrix = new double[,]
        {
            {1, 3}, {2, 4}, {3, 5}, {4, 6}, {5, 7}
        };
        double[] target = new double[] { 0, 0, 0, 1, 1 };
        var nbc = new CsML.Probability.NaiveBayesClassifier<double>();
        nbc.Train(matrix, target);
        Assert.Equal(3.0 / 5.0, nbc.classProbabilities[0]);
        Assert.Equal(2.0 / 5.0, nbc.classProbabilities[1]);
    }

    [Fact]
    public void Train_predict_iris()
    {
        var mapping = new Dictionary<int, Dictionary<string, double>>();
        mapping[4] = new Dictionary<string, double>
           {
               { "versicolor", 0 }, {"virginica", 1 }, {"setosa", 2}
           };
        string strWorkPath = Directory
                                .GetParent(Environment.CurrentDirectory)!
                                .Parent!
                                .Parent!
                                .FullName;
        string inpuPath = Path.Combine(strWorkPath, "Data/iris.csv");
        double[,] matrix = CsML.Util.Matrix.FromCSV(
                                inpuPath, mapping, loadFromRow: 1);
        Span2D<double> matrixSpan = matrix;
        double[,] features = matrixSpan.Slice(0, 0, 150, 4).ToArray();
        Assert.Equal(5.1, features[0, 0]);
        Assert.Equal(3.5, features[0, 1]);
        Assert.Equal(1.4, features[0, 2]);
        Assert.Equal(0.2, features[0, 3]);
        Assert.Equal(150, features.GetLength(0));
        double[] target = matrixSpan.GetColumn(4).ToArray();
        Assert.Equal(2, target[0]);
        Assert.Equal(150, target.Length);
        (features, target) = CsML.Util.Features.Shuffle(features, target);
        double[,] ftrain, ftest;
        double[] ttrain, ttest;
        ((ftrain, ttrain), (ftest, ttest)) = CsML.Util.Features.Split(
                                                    features, target, 0.8);
        var nbc = new CsML.Probability.NaiveBayesClassifier<double>();
        nbc.Train(ftrain, ttrain);
        double[] predictions = nbc.Predict(ftest);
        Assert.True(CsML.Util.Array.ClassificationAccuracy(
            ttest, predictions) > 0.8);
    }
}

public class ProbabilityMassFunction
{
    [Fact]
    public void BracketOperator()
    {
        string[] outcomes = new string[] { "heads", "tails" };
        var coin = new CsML.Probability
                           .ProbabilityMassFunction<string>(outcomes);
        Assert.Equal(0.5, coin["heads"]);
        Assert.Equal(0.5, coin["tails"]);
    }

    [Fact]
    public void Get_hypotheses()
    {
        string[] outcomes = new string[] { "b", "a", "c" };
        var pmf = new CsML.Probability
                          .ProbabilityMassFunction<string>(outcomes);
        Assert.True(pmf.hypotheses.SequenceEqual(
                    new string[] { "a", "b", "c" }));
    }

    [Fact]
    public void Get_probabilities()
    {
        string[] outcomes = new string[] { "b", "a", "c" };
        var pmf = new CsML.Probability
                          .ProbabilityMassFunction<string>(outcomes);
        pmf["a"] = 0.1;
        pmf["b"] = 0.2;
        pmf["c"] = 0.3;
        Assert.True(pmf.probabilities.SequenceEqual(
                    new double[] { 0.1, 0.2, 0.3 }));
    }

    [Fact]
    public void Get_zipped()
    {
        string[] outcomes = new string[] { "b", "a", "c" };
        var pmf = new CsML.Probability
                          .ProbabilityMassFunction<string>(outcomes);
        var expected = new (string, double)[] {
            ("a", 1.0/3.0), ("b", 1.0 / 3.0), ("c", 1.0 / 3.0) };
        Assert.True(expected.SequenceEqual(pmf.zipped));
    }

    [Fact]
    public void FromBinomial()
    {
        CsML.Probability.ProbabilityMassFunction<int> pmf;
        int[] ks = Enumerable.Range(1, 251).ToArray();
        pmf = CsML.Probability.ProbabilityMassFunction<int>
                              .FromBinomial(250, ks, 0.5);
        Assert.Equal((125, 0.05041221314730964), pmf.HighestProbability());
        Assert.Equal(0.0083571817249182, pmf[140]);
        Assert.Equal(0.06642115124004333, 1 - pmf.SumProbabilities(
            110, 140, includeLower: false, includeUpper: false));
    }

    [Fact]
    public void FromNormal()
    {
        CsML.Probability.ProbabilityMassFunction<double> pmf;
        double[] hypotheses = Enumerable.Range(0, 1001)
                                        .Select(x => (double)x)
                                        .ToArray();
        pmf = CsML.Probability
                  .ProbabilityMassFunction<double>
                  .FromNormal(hypotheses, 500, 200 * 200);
        Assert.Equal((500, 0.0020196170605523193), pmf.HighestProbability());
        Assert.Equal(0.6899874672473569,
                     pmf.SumProbabilities(300, 700, includeLower: false));
        Assert.Equal(0.9661436914270904,
                     pmf.SumProbabilities(100, 900, includeLower: false));
    }

    [Fact]
    public void Add()
    {
        double[] diceValues = Enumerable.Range(1, 6)
                                        .Select(x => (double)x)
                                        .ToArray();
        Assert.Equal(1, diceValues.Min());
        Assert.Equal(6, diceValues.Max());
        var dice1 = new CsML.Probability
                            .ProbabilityMassFunction<double>(
                                (double[])diceValues.Clone());
        var dice2 = new CsML.Probability
                            .ProbabilityMassFunction<double>(
                                (double[])diceValues.Clone());
        var pmf = dice1.Add(dice2);
        double[] summmedDice = Enumerable.Range(2, 11)
                                         .Select(x => (double)x)
                                         .ToArray();
        Assert.True(summmedDice.SequenceEqual(pmf.hypotheses));
        Assert.True(1.0 - pmf.probabilities.Sum() < 0.0000000001);
        Assert.Equal((7, 0.166666666666666666), pmf.HighestProbability());
    }

    [Fact]
    public void HighestProbability()
    {
        string[] outcomes = new string[] { "b", "a", "c" };
        var pmf = new CsML.Probability
                          .ProbabilityMassFunction<string>(outcomes);
        pmf["a"] = 0.1;
        pmf["b"] = 0.2;
        pmf["c"] = 0.3;
        Assert.Equal(("c", 0.3), pmf.HighestProbability());
    }

    [Fact]
    public void Mean()
    {
        double[] hypos = new double[] { 10, 20, 30 };
        var pmf = new CsML.Probability.ProbabilityMassFunction<double>(hypos);
        pmf[10] = 0.5;
        pmf[20] = 0.25;
        pmf[30] = 0.25;
        Assert.Equal(10 * 0.5 + 20 * 0.25 + 30 * 0.25, pmf.Mean());
    }

    [Fact]
    public void Normalise()
    {
        string[] outcomes = new string[] { "heads", "tails" };
        var coin = new CsML.Probability
                            .ProbabilityMassFunction<string>(outcomes);
        Assert.Equal(0.5, coin.table["heads"]);
        Assert.Equal(0.5, coin.table["tails"]);
    }

    [Fact]
    public void SumProbabilities_lowerOnly()
    {
        string[] target = new string[] { "a", "b", "c", "d", "e" };
        double[] weights = new double[] { 50, 30, 10, 5, 5 };
        var pmf = new CsML.Probability.ProbabilityMassFunction<string>();
        foreach ((string, double) pair in target.Zip(weights))
            pmf[pair.Item1] = pair.Item2;
        pmf.Normalise();
        Assert.Equal(0.5, pmf["a"]);
        Assert.Equal(0.1, pmf.SumProbabilities("c", null, false));
        Assert.Equal(0.2, pmf.SumProbabilities("c", null, true));
    }

    [Fact]
    public void SumProbabilities_upperOnly()
    {
        string[] target = new string[] { "a", "b", "c", "d", "e" };
        double[] weights = new double[] { 50, 30, 10, 5, 5 };
        var pmf = new CsML.Probability.ProbabilityMassFunction<string>();
        foreach ((string, double) pair in target.Zip(weights))
            pmf[pair.Item1] = pair.Item2;
        pmf.Normalise();
        Assert.Equal(0.5, pmf["a"]);
        Assert.Equal(0.8, pmf.SumProbabilities(null, "c", includeUpper: false));
        Assert.Equal(0.9, pmf.SumProbabilities(null, "c", includeUpper: true));
    }
    [Fact]
    public void SumProbabilities_bothUpperandLower()
    {
        string[] target = new string[] { "a", "b", "c", "d", "e" };
        double[] weights = new double[] { 50, 30, 10, 5, 5 };
        var pmf = new CsML.Probability.ProbabilityMassFunction<string>();
        foreach ((string, double) pair in target.Zip(weights))
            pmf[pair.Item1] = pair.Item2;
        pmf.Normalise();
        Assert.Equal(0.5, pmf["a"]);
        Assert.Equal(0.1, pmf.SumProbabilities("c", "d"));
        Assert.True((pmf.SumProbabilities("c", "d", includeUpper: true) - 0.15)
                     < 0.00000001);
    }

    [Fact]
    public void ToSampler()
    {
        string[] target = new string[] { "a", "b", "c", "d", "e" };
        double[] weights = new double[] { 50, 30, 10, 5, 5 };
        var pmf = new CsML.Probability.ProbabilityMassFunction<string>();
        foreach ((string, double) pair in target.Zip(weights))
            pmf[pair.Item1] = pair.Item2;
        var sampler = pmf.ToSampler();
        string[] result = sampler.SampleTarget(1000);
        var counts = result.ElementCounts();
        Assert.InRange((double)counts["a"] / 1000.0, 0.45, 0.55);
        Assert.InRange((double)counts["b"] / 1000.0, 0.25, 0.35);
        Assert.InRange((double)counts["c"] / 1000.0, 0.05, 0.15);
        Assert.InRange((double)counts["d"] / 1000.0, 0.01, 0.1);
        Assert.InRange((double)counts["e"] / 1000.0, 0.01, 0.1);
    }

    [Fact]
    public void Update_dictionary()
    {
        string[] outcomes = new string[] { "heads", "tails" };
        var coin = new CsML.Probability
                           .ProbabilityMassFunction<string>(outcomes);
        coin.Update(new Dictionary<string, double> { { "heads", 0.75 },
                                                     { "tails", 0.5 } });
        Assert.Equal(0.375, coin.table["heads"]);
        Assert.Equal(0.25, coin.table["tails"]);
        coin.Normalise();
        Assert.Equal(0.6, coin.table["heads"]);
        Assert.Equal(0.4, coin.table["tails"]);
    }

    [Fact]
    public void Update_array()
    {
        string[] outcomes = new string[] { "heads", "tails" };
        var coin = new CsML.Probability
                           .ProbabilityMassFunction<string>(outcomes);
        coin.Update(new double[] { 0.75, 0.5 });
        Assert.Equal(0.375, coin.table["heads"]);
        Assert.Equal(0.25, coin.table["tails"]);
        coin.Normalise();
        Assert.Equal(0.6, coin.table["heads"]);
        Assert.Equal(0.4, coin.table["tails"]);
    }
}

public class RandomClassifier
{
    [Fact]
    public void Train_predict()
    {
        var cfier = new CsML.Probability.RandomClassifier<string>();
        string[] target = Enumerable.Repeat("a", 50)
                            .Concat(Enumerable.Repeat("b", 30))
                            .Concat(Enumerable.Repeat("c", 10))
                            .Concat(Enumerable.Repeat("d", 5))
                            .Concat(Enumerable.Repeat("e", 5))
                            .ToArray();
        Assert.Equal(100, target.Length);
        cfier.Train(new double[,] { }, target);
        string[] result = cfier.Predict(new double[1000, 1]);
        var counts = result.ElementCounts();
        Assert.Equal(1000, result.Length);
        Assert.InRange((double)counts["a"] / 1000.0, 0.45, 0.55);
        Assert.InRange((double)counts["b"] / 1000.0, 0.25, 0.35);
        Assert.InRange((double)counts["c"] / 1000.0, 0.05, 0.15);
        Assert.InRange((double)counts["d"] / 1000.0, 0.01, 0.1);
        Assert.InRange((double)counts["e"] / 1000.0, 0.01, 0.1);
    }
}

public class WeightedIndexSampler
{
    [Fact]
    public void SampleIndex()
    {
        string[] target = new string[] { "a", "b", "c", "d", "e" };
        double[] weights = new double[] { 50, 30, 10, 5, 5 };
        var wis = new CsML.Probability
                          .WeightedIndexSampler<string>(target, weights);
        int[] result = wis.SampleIndex(1000);
        var counts = result.ElementCounts();
        Assert.Equal(new int[] { 0, 1, 2, 3, 4 },
                     counts.Keys.OrderBy(x => x).ToArray());
        Assert.InRange((double)counts[0] / 1000.0, 0.45, 0.55);
        Assert.InRange((double)counts[1] / 1000.0, 0.25, 0.35);
        Assert.InRange((double)counts[2] / 1000.0, 0.05, 0.15);
        Assert.InRange((double)counts[3] / 1000.0, 0.01, 0.1);
        Assert.InRange((double)counts[4] / 1000.0, 0.01, 0.1);
    }

    [Fact]
    public void SampleTarget()
    {
        string[] target = new string[] { "a", "b", "c", "d", "e" };
        double[] weights = new double[] { 50, 30, 10, 5, 5 };
        var wis = new CsML.Probability
                          .WeightedIndexSampler<string>(target, weights);
        string[] result = wis.SampleTarget(1000);
        var counts = result.ElementCounts();
        Assert.InRange((double)counts["a"] / 1000.0, 0.45, 0.55);
        Assert.InRange((double)counts["b"] / 1000.0, 0.25, 0.35);
        Assert.InRange((double)counts["c"] / 1000.0, 0.05, 0.15);
        Assert.InRange((double)counts["d"] / 1000.0, 0.01, 0.1);
        Assert.InRange((double)counts["e"] / 1000.0, 0.01, 0.1);
    }
}