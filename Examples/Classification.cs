namespace CsML.Examples.Classification;

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
    }
}

// (features, target) = Features.Shuffle(features, target);

// // Random classifier

// var results = new List<double>(){};
// var iter = new KFoldIterator(dataLength, 10);
// double[,] ftrain, ftest;
// double[] ttrain, ttest;
// foreach(bool[] f in iter)
// {    
//     Console.Write(".");
//     (ftrain, ftest) = Matrix.Split(features, f);
//     (ttrain, ttest) = Array.Split(target, f);
//     var rcfier = new Classification.RandomClassifier<double>();
//     rcfier.Train(ftrain, ttrain);
//     double[] predictions = rcfier.Predict(ftest);
//     results.Add(Array.ClassificationAccuracy(ttest, predictions));
// }
// var mean = results.Average();
// results = results.Select(x => Math.Round(x, 4)).ToList();
// Console.WriteLine("");
// Console.WriteLine(string.Join(", ", results.ToArray()));
// Console.WriteLine($"Average {mean}");

// // Decision tree

// List<double> results = new List<double>(){};
// double[,] ftrain, ftest;
// double[] ttrain, ttest;
// var iter = new KFoldIterator(150, 10);
// int fold = 1;
// foreach(bool[] f in iter)
// {
//     Console.Write($"Fold {fold}: ");
//     (ftrain, ftest) = Matrix.Split(features, f);
//     (ttrain, ttest) = Array.Split(target, f);
//     var props = CsML.Utility.Features.ClassProportions<double>(ttrain)
//         .Select(x => Math.Round(x.Item3, 4));
//     Console.Write(String.Join(",", props));
//     var tree = new CsML.Tree.BinaryTree("classify", Statistics.Gini);
//     tree.maxdepth = 15;
//     tree.minrows = 3;
//     tree.Train(ftrain, ttrain);
//     double[] predictions = tree.Predict(ftest);
//     var accuracy = Array.ClassificationAccuracy(ttest, predictions);
//     Console.WriteLine($" Accuracy: {accuracy:0.0000}");
//     results.Add(accuracy);
//     fold++;
// }
// var mean = results.Average();
// Console.WriteLine($"Average {mean:0.0000}");

// // Random Forest

// List<double> results = new List<double>(){};
// double[,] ftrain, ftest;
// double[] ttrain, ttest;
// var iter = new KFoldIterator(150, 10);
// int fold = 1;
// foreach(bool[] f in iter)
// {
//     Console.Write($"Fold {fold}: ");
//     (ftrain, ftest) = Matrix.Split(features, f);
//     (ttrain, ttest) = Array.Split(target, f);
//     var props = CsML.Utility.Features.ClassProportions<double>(ttrain)
//         .Select(x => Math.Round(x.Item3, 4));
//     Console.Write(String.Join(",", props));
//     var tree = new CsML.Tree.RandomForest("classify", Statistics.Gini);
//     tree.Train(ftrain, ttrain);
//     double[] predictions = tree.Predict(ftest);
//     var accuracy = Array.ClassificationAccuracy(ttest, predictions);
//     Console.WriteLine($" Accuracy: {accuracy:0.0000}");
//     results.Add(accuracy);
//     fold++;
// }
// var mean = results.Average();
// Console.WriteLine($"Average {mean:0.0000}");
