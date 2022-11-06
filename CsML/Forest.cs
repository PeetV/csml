using CsML.Tree;

namespace CsML.Forest;

/// <summary>
/// A forest of binary decision trees for classification and regression.
/// </summary>
public class RandomForest
{
    /// <summary>The trees that make up the forest once trained.</summary>
    public List<BinaryTree> trees;

    /// <summary>The number of tress in the forest.</summary>
    public int treeCount;

    /// <summary>The number of matrix columns the model was trained on.</summary>
    public int minColumns;

    /// <summary>The number of matrix rows the model was trained on.</summary>
    public int inputRecordCount;

    /// <summary>Maximum tree depth stopping condition.</summary>
    public int maxdepth = 1000;

    /// <summary>Minimum rows in a tree stopping condition.</summary>
    public int minrows = 3;

    /// <summary>
    /// Number of random features to use at each split point during training.
    /// Used to add randomisation to a random forest. Defaults to square
    /// root of the number of columns.
    /// </summary>
    public int randomFeatures;

    /// <summary>
    /// The distinct class labels if creating a classification model.
    /// </summary>
    public double[]? classes;

    /// <summary>
    /// The function to use to calculate the purity of a slice of the
    /// target array.
    /// <see> See
    /// <seealso cref="CsML.Util.Statistics.Gini{T}(IEnumerable{T})" />
    /// for default function to use.
    /// </see>
    /// </summary>
    public Func<double[], double> purityFn;

    /// <summary>
    /// Sample input data with replacement when training each tree if true.
    /// </summary>
    public bool bootstrapSampleData = true;

    private string _mode;

    /// <summary>Mode can be either "classify" or "regress".</summary>
    public string mode
    {
        get { return _mode; }
        set
        {
            if (value != "classify" & value != "regress")
                throw new ArgumentException("Mode must be 'classify' or 'regress'");
            _mode = value;
        }
    }

    /// <summary>Create an untrained random forest.</summary>
    public RandomForest(string mode,
        Func<double[], double> purityFn,
        int treeCount = 103,
        int randomFeatures = 0)
    {
        trees = new List<BinaryTree>();
        minColumns = 0;
        inputRecordCount = 0;
        _mode = "classify";
        this.randomFeatures = randomFeatures;
        this.mode = mode;
        this.purityFn = purityFn;
        this.treeCount = treeCount;
    }

    private int DefaultRandomFeatures(int n)
    {
        if (n < 0)
            throw new ArgumentException("Features cannot be less than zero");
        return (int)Math.Sqrt(n);
    }

    /// <summary>
    /// Train the model.
    /// </summary>
    /// <param name="matrix">The features to train the model on.</param>
    /// <param name="target">The target vector to train on.</param>
    public void Train(double[,] matrix, double[] target)
    {
        inputRecordCount = matrix.GetLength(0);
        int targetLength = target.Length;
        if (inputRecordCount == 0 | targetLength == 0)
            throw new ArgumentException("Empty input");
        if (inputRecordCount != targetLength)
            throw new ArgumentException("Inputs must be the same length");
        trees = new List<BinaryTree>();
        minColumns = matrix.GetLength(1);
        if (_mode == "classify")
            classes = target.Distinct().ToArray();
        randomFeatures = randomFeatures == 0 ?
                         DefaultRandomFeatures(minColumns) :
                         randomFeatures;
        foreach (int cntr in Enumerable.Range(0, treeCount))
        {
            var tree = new BinaryTree(mode, purityFn);
            tree.maxdepth = maxdepth;
            tree.minrows = minrows;
            tree.randomFeatures = randomFeatures;
            tree.bootstrapSampleData = bootstrapSampleData;
            trees.Add(tree);
        }
        Parallel.ForEach(trees, tree => tree.Train(matrix, target, true));
    }

    /// <summary>
    /// Make predictions using the model.
    /// </summary>
    /// <param name="matrix">New data to infer predictions from.</param>
    public double[] Predict(double[,] matrix)
    {
        inputRecordCount = matrix.GetLength(0);
        if (trees.Count == 0)
            throw new ArgumentException("Forest is untrained");
        if (matrix.GetLength(1) != minColumns)
            throw new ArgumentException("Forest trained on different number of columns");
        if (inputRecordCount == 0)
            throw new ArgumentException("Empty input");
        double[] result = new double[inputRecordCount];
        Parallel.For(0, inputRecordCount, i =>
        {
            List<double[]> input = new List<double[]>();
            input = input.Append(CsML.Util.Matrix.GetRow(matrix, i, false)).ToList();
            if (mode == "regress")
            {
                List<double> predictions = new List<double>(trees.Count);
                foreach (var tree in trees)
                    predictions.Add(tree.Predict(CsML.Util.Matrix.FromList2D(input))[0]);
                result[i] = predictions.Average();
            }
            else
            {
                Dictionary<double, int> counts = new Dictionary<double, int>();
                double vote;
                foreach (var tree in trees)
                {
                    vote = tree.Predict(CsML.Util.Matrix.FromList2D(input))[0];
                    if (counts.ContainsKey(vote)) counts[vote] += 1;
                    else counts[vote] = 1;
                }
                result[i] = counts.MaxBy(kvp => kvp.Value).Key;
            }
        });
        return result;
    }

    /// <summary>
    /// Calculate the mean weighted purity gains across trees.
    /// </summary>
    /// <returns>
    /// An array containing purity gains in the order of feature columns.
    /// </returns>
    public double[] PurityGains()
    {
        double[] result = new double[minColumns];
        foreach (var tree in trees)
        {
            double[] treeGains = tree.PurityGains();
            for (int idx = 0; idx < minColumns; idx++)
                result[idx] = result[idx] + treeGains[idx];
        }
        for (int idx = 0; idx < minColumns; idx++)
            result[idx] = result[idx] / (double)trees.Count;
        return result;
    }
}