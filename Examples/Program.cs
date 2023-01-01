using System.CommandLine;

namespace CsML.Examples;

public enum DataSet
{
    Iris,
    Led,
    Sonar
}

class Program
{
    static int Main(string[] args)
    {
        var dataSetOption = new Option<DataSet>(
            name: "--dataset",
            description: "Example dataset to use",
            getDefaultValue: () => DataSet.Iris);

        var rootCommand = new RootCommand("CsML examples");

        var classifyCommand = new Command("classify", "Classification example.")
            {
                dataSetOption,
            };
        rootCommand.AddCommand(classifyCommand);

        //readCommand.SetHandler(async (file, delay, fgcolor, lightMode) =>
        //    {
        //        await ReadFile(file!, delay, fgcolor, lightMode);
        //    },
        //    fileOption, delayOption, fgcolorOption, lightModeOption);

        return rootCommand.InvokeAsync(args).Result;
    }
}