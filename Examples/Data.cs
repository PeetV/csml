
// Iris data

var mapping = new Dictionary<int, Dictionary<string, double>>();
mapping[4] = new Dictionary<string, double>
{
    { "versicolor", 0 }, {"virginica", 1 }, {"setosa", 2}
};
string inputPath = "/Users/peet/Sources/csml/Tests/Data/iris.csv";
// string inputPath = @"C:\Users\peet.vermeulen\source\repos\csml\Tests\Data\iris.csv";
double[,] data = Matrix.FromCSV(inputPath, mapping, loadFromRow: 1);

int dataLength = data.GetLength(0);
double[,] features = new double[dataLength, 4];
double[] target = new double[dataLength];
for (int r=0; r < data.GetLength(0); r++)
{
    for (int c=0; c < 4; c++)
    {
        features[r, c] = data[r, c];
    }
    target[r] = data[r, 4];
}

// LED data

var inputPath = "/Users/peet/Sources/csml/Tests/Data/led7.csv";
// inputPath = @"C:\Users\peet.vermeulen\source\repos\csml\Tests\Data\led7.csv";
var data = CsML.Utility.Matrix.FromCSV(inputPath, null, loadFromRow: 1);

var dataLength = data.GetLength(0);
var features = new double[dataLength, 7];
var target = new double[dataLength];
for (int r=0; r < data.GetLength(0); r++)
{
    for (int c=0; c < 7; c++)
    {
        features[r, c] = data[r, c];
    }
    target[r] = data[r, 7];
}

// Sonar data

var mapping = new Dictionary<int, Dictionary<string, double>>();
mapping[60] = new Dictionary<string, double>{ { "R", 0 }, {"M", 1 }};
var inputPath = "/Users/peet/Sources/csml/Tests/Data/sonar.csv";
// inputPath = @"C:\Users\peet.vermeulen\source\repos\csml\Tests\Data\sonar.csv";
var data = CsML.Utility.Matrix.FromCSV(inputPath, mapping, loadFromRow: 0);

var dataLength = data.GetLength(0);
var features = new double[dataLength, 59];
var target = new double[dataLength];
for (int r=0; r < data.GetLength(0); r++)
{
    for (int c=0; c < 59; c++)
    {
        features[r, c] = data[r, c];
    }
    target[r] = data[r, 60];
}

