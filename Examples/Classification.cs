namespace CsML.Examples.Classification;

using CsML.Utility;

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
        double[] predictions;
        Console.WriteLine("10-fold cross validation:");
        var iter = new KFoldIterator(features.GetLength(0), 10);
        int fold = 1;
        foreach (bool[] f in iter)
        {
            Console.Write($"Fold {fold}: ");
            (ftrain, ftest) = Matrix.Split(features, f);
            (ttrain, ttest) = Arrays.Split(target, f);
            switch (classifier)
            {
                case CsML.Examples.Classifier.DecisionTree:
                    var bt = new CsML.Tree.BinaryTree(ModelType.Classification, Statistics.Gini);
                    bt.Train(ftrain, ttrain);
                    predictions = bt.Predict(ftest);
                    break;
                case CsML.Examples.Classifier.NaiveBayes:
                    var nb = new CsML.Probability.Classification.NaiveBayesClassifier<double>();
                    nb.Train(ftrain, ttrain);
                    predictions = nb.Predict(ftest);
                    break;
                case CsML.Examples.Classifier.NearestNeighbour:
                    var nn =new CsML.Cluster.NearestNeighbour(ModelType.Classification);
                    nn.Train(ftrain, ttrain);
                    predictions = nn.Predict(ftest);
                    break;
                case CsML.Examples.Classifier.RandomForest:
                    var rf = new CsML.Tree.RandomForest(ModelType.Classification, Statistics.Gini);
                    rf.Train(ftrain, ttrain);
                    predictions = rf.Predict(ftest);
                    break;
                default:
                    var rnd = new CsML.Probability.Classification.RandomClassifier<double>();
                    rnd.Train(ftrain, ttrain);
                    predictions = rnd.Predict(ftest);
                    break;
            }
            var accuracy = Arrays.ClassificationAccuracy(ttest, predictions);
            Console.WriteLine($" Accuracy: {accuracy:0.0000}");
            results.Add(accuracy);
            fold++;
        }
        var mean = results.Average();
        Console.WriteLine($"Average {mean:0.0000}");
    }
}