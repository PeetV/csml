namespace CsML.Examples.Classification;

using CsML.Utility;

/// <summary>Interface for classification models.</summary>
public interface IClassifier
{
    /// <summary>Train a classifier using features matrix and target array.</summary>
    void Train(double[,] matrix, double[] target);
    /// <summary>Infer target values from new data.</summary>
    double[] Predict(double[,] matrix);
}
public static class Classify
{
    public static void RunExample(
        CsML.Examples.DataSet dataSet,
        CsML.Examples.Classifier classifier
    )
    {
        Console.WriteLine("----------------------");
        Console.WriteLine("Classification example");
        Console.WriteLine("----------------------");
        Console.WriteLine($"Dataset: {dataSet}");
        Console.WriteLine($"Classifier: {classifier}");
        double[,] features;
        double[] target;
        (features, target) = dataSet switch
        {
            CsML.Examples.DataSet.Iris => CsML.Examples.Data.Load.Iris(),
            CsML.Examples.DataSet.Led => CsML.Examples.Data.Load.Led(),
            CsML.Examples.DataSet.Sonar => CsML.Examples.Data.Load.Sonar(),
            _ => throw new ArgumentException("Dataset option error")
        };
        (features, target) = Features.Shuffle(features, target);
        List<double> results = new List<double>() { };
        double[,] ftrain, ftest;
        double[] ttrain, ttest;
        var iter = new KFoldIterator(150, 10);
        int fold = 1;
        foreach (bool[] f in iter)
        {
            Console.Write($"Fold {fold}: ");
            (ftrain, ftest) = Matrix.Split(features, f);
            (ttrain, ttest) = Arrays.Split(target, f);
            IClassifier cfier = classifier switch
            {
                CsML.Examples.Classifier.DecisionTree => (IClassifier)new CsML.Tree.BinaryTree(
                                                            ModelType.Classification,
                                                            Statistics.Gini),
                CsML.Examples.Classifier.NaiveBayes => (IClassifier)new CsML.Probability.Classification
                                                            .NaiveBayesClassifier<double>(),
                CsML.Examples.Classifier.NearestNeighbour => (IClassifier)new CsML.Cluster.NearestNeighbour(
                                                            ModelType.Classification),
                CsML.Examples.Classifier.Random => (IClassifier)new CsML.Probability.Classification
                                                            .RandomClassifier<double>(),
                CsML.Examples.Classifier.RandomForest => (IClassifier)new CsML.Tree.RandomForest(
                                                            ModelType.Classification,
                                                            Statistics.Gini),
                _ => throw new ArgumentException("Classifier option error")
            };
            cfier.Train(ftrain, ttrain);
            double[] predictions = cfier.Predict(ftest);
            var accuracy = Arrays.ClassificationAccuracy(ttest, predictions);
            Console.WriteLine($" Accuracy: {accuracy:0.0000}");
            results.Add(accuracy);
            fold++;
        }
        var mean = results.Average();
        Console.WriteLine($"Average {mean:0.0000}");
    }
}