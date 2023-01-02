namespace CsML.Examples.Classification;

using CsML.Utility;
using CsML.Examples;

public static class Classify
{
    public static void RunExample(
        DataSet dataSet, 
        Classifier classifier
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
            DataSet.Iris => Data.Load.Iris(),
            DataSet.Led => Data.Load.Led(),
            DataSet.Sonar => Data.Load.Sonar(),
            _ => throw new ArgumentException("Dataset option error")
        };
        (features, target) = Features.Shuffle(features, target);
        List<double> results = new List<double>() { };
        double[,] ftrain, ftest;
        double[] ttrain, ttest, predictions;
        Console.WriteLine("10-fold cross validation:");
        var iter = new KFoldIterator(features.GetLength(0), 10);
        int fold = 1;
        foreach (bool[] f in iter)
        {
            Console.Write($"Fold {fold}: ");
            (ftrain, ftest) = Matrix.Split(features, f);
            (ttrain, ttest) = Arrays.Split(target, f);
            IModel m = classifier switch
            {
                Classifier.DecisionTree => 
                    new CsML.Tree.BinaryTree(ModelType.Classification, Statistics.Gini),
                Classifier.NaiveBayes => 
                    new CsML.Probability.Classification.NaiveBayesClassifier(),
                Classifier.NearestNeighbour => 
                    new CsML.Cluster.NearestNeighbour(ModelType.Classification),
                Classifier.RandomForest => 
                    new CsML.Tree.RandomForest(ModelType.Classification, Statistics.Gini),
                Classifier.Random => 
                    new CsML.Probability.Classification.RandomClassifier()
                _ => throw new ArgumentException("Classifier option error")
            };
            m.Train(ftrain, ttrain);
            predictions = m.Predict(ftest);
            var accuracy = Arrays.ClassificationAccuracy(ttest, predictions);
            Console.WriteLine($" Accuracy: {accuracy:0.0000}");
            results.Add(accuracy);
            fold++;
        }
        var mean = results.Average();
        Console.WriteLine($"Average {mean:0.0000}");
    }
}