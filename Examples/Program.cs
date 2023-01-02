using System.CommandLine;

namespace CsML.Examples;

public enum DataSet
{
    Iris,
    Led,
    Sonar
}

public enum Classifier
{
    DecisionTree,
    NaiveBayes,
    NearestNeighbour,
    Random,
    RandomForest,
}

public enum Regressor
{
    DecisionTree,
    NearestNeighbour,
    RandomForest,
}

class Program
{
    static int Main(string[] args)
    {
        var dataSetOption = new Option<DataSet>(
            name: "--dataset",
            description: "Example dataset to use",
            getDefaultValue: () => DataSet.Iris);
        dataSetOption.AddAlias("-d");

        var classifierOption = new Option<Classifier>(
            name: "--classifier",
            description: "Classification algorithm to use",
            getDefaultValue: () => Classifier.Random);
        classifierOption.AddAlias("-c");

        var regressorOption = new Option<Regressor>(
            name: "--regressor",
            description: "Regression algorithm to use",
            getDefaultValue: () => Classifier.DecisionTree);
        classifierOption.AddAlias("-r");

        var rootCommand = new RootCommand("CsML examples");

        var classifyCommand = new Command("classify", "Classification examples")
            {
                dataSetOption,
                classifierOption
            };
        rootCommand.AddCommand(classifyCommand);

        classifyCommand.SetHandler((dataSet, classifier) =>
            { CsML.Examples.Classification.Classify.RunExample(dataSet, classifier); },
            dataSetOption, classifierOption);

        return rootCommand.Invoke(args);
    }
}