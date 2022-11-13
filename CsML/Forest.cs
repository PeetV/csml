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

    /// <summary>
    /// The number of matrix columns the model was trained on.
    /// </summary>
    public int minColumns;

    /// <summary>The number of matrix rows the model was trained on.</summary>
    public int inputRecordCount;

    /// <summary>Maximum tree depth stopping condition.</summary>
    public int maxdepth = 1000;

    /// <summary>Minimum rows stopping condition.</summary>
    public int minrows = 3;

    /// <summary>
    /// Number of random features to use at each split point during training.
    /// Used to add randomisation. Defaults to square root of the number of
    /// columns.</summary>
    public int randomFeatures;

    /// <summary>
    /// The distinct class labels if creating a classification model.
    /// </summary>
    public double[]? classes;

    /// <summary>
    /// The function to use to calculate the purity of a slice of the target
    /// array.<see> See
    /// <seealso cref="CsML.Util.Statistics.Gini" />
    /// for default function to use.
    /// </see>
    /// </summary>
    public Func<double[], double> purityFn;

    /// <summary>
    /// Sample input data with replacement when training each tree if true.
    /// </summary>
    public bool bootstrapSampleData = true;

    /// <summary>Keep out of bag indeces when bootstrap sampling.</summary>
    public bool retainOutOfBagIndeces = false;

    private string _mode;

    /// <summary>Mode can be either "classify" or "regress".</summary>
    public string Mode
    {
        get { return _mode; }
        set
        {
            if (value != "classify" & value != "regress")
                throw new ArgumentException(
                    "Mode must be 'classify' or 'regress'");
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
        Mode = mode;
        this.purityFn = purityFn;
        this.treeCount = treeCount;
    }

    private int DefaultRandomFeatures(int n)
    {
        if (n < 0)
            throw new ArgumentException("Features cannot be less than zero");
        return (int)Math.Sqrt(n);
    }

    /// <summary>Train the model.</summary>
    /// <param name="matrix">The features to train the model on.</param>
    /// <param name="target">The target vector to train on.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length or empty.
    /// </exception>
    public void Train(double[,] matrix, double[] target)
    {
        inputRecordCount = matrix.GetLength(0);
        int targetLength = target.Length;
        if (inputRecordCount == 0 | targetLength == 0)
            throw new ArgumentException(CsML.Errors.Types.E1);
        if (inputRecordCount != targetLength)
            throw new ArgumentException(CsML.Errors.Types.E2);
        trees = new List<BinaryTree>();
        minColumns = matrix.GetLength(1);
        if (_mode == "classify")
            classes = target.Distinct().ToArray();
        randomFeatures = randomFeatures == 0 ?
                         DefaultRandomFeatures(minColumns) :
                         randomFeatures;
        foreach (int cntr in Enumerable.Range(0, treeCount))
        {
            // Note that mode and purityFn is set in the constructor
            var tree = new BinaryTree(this.Mode, this.purityFn);
            tree.maxdepth = this.maxdepth;
            tree.minrows = this.minrows;
            tree.randomFeatures = this.randomFeatures;
            tree.bootstrapSampleData = this.bootstrapSampleData;
            tree.retainOutOfBagIndeces = this.retainOutOfBagIndeces;
            trees.Add(tree);
        }
        Parallel.ForEach(trees, tree => tree.Train(matrix, target, true));
    }

    /// <summary>Make predictions using the model.</summary>
    /// <param name="matrix">New data to infer predictions from.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if input is empty, or model has not been trained, or if trained
    /// on a different number of columns.
    /// </exception>
    public double[] Predict(double[,] matrix)
    {
        inputRecordCount = matrix.GetLength(0);
        if (inputRecordCount == 0)
            throw new ArgumentException(CsML.Errors.Types.E1);
        if (trees.Count == 0)
            throw new ArgumentException("Forest is untrained");
        if (matrix.GetLength(1) != minColumns)
            throw new ArgumentException(
                "Forest trained on different number of columns");
        var result = new double[inputRecordCount];
        Parallel.For(0, inputRecordCount, i =>
        {
            var input = new List<double[]>();
            input = input.Append(
                CsML.Util.Matrix.GetRow(matrix, i, false)).ToList();
            if (Mode == "regress")
            {
                var predictions = new List<double>(trees.Count);
                foreach (var tree in trees)
                    predictions.Add(
                        tree.Predict(
                            CsML.Util.Matrix.FromList2D(input))[0]);
                result[i] = predictions.Average();
            }
            else
            {
                var counts = new Dictionary<double, int>();
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
    /// Predict labels for new data and return corresponding class probability
    /// estimates.
    /// </summary>
    /// <exception cref="System.ArgumentException">
    /// Thrown if model has not been trained, or input is empty or the model
    /// has not been trained.
    /// </exception>
    public (double, Dictionary<double, double>)[] PredictWithProbabilities(
        double[,] matrix,
        bool skipchecks = false)
    {
        inputRecordCount = matrix.GetLength(0);
        if (!skipchecks)
        {
            if (trees.Count == 0)
                throw new ArgumentException("Forest is untrained");
            if (matrix.GetLength(1) != minColumns)
                throw new ArgumentException(
                    "Forest trained on different number of columns");
            if (inputRecordCount == 0)
                throw new ArgumentException(CsML.Errors.Types.E1);
            if (Mode == "regress")
                throw new ArgumentException(
                    "Probabilities require treemode to be 'classify'");
        }
        var result = new (double, Dictionary<double, double>)[inputRecordCount];
        Parallel.For(0, inputRecordCount, i =>
        {
            var input = new List<double[]>();
            input = input.Append(
                CsML.Util.Matrix.GetRow(matrix, i, false)).ToList();
            var counts = new Dictionary<double, int>();
            var probs = new Dictionary<double, double>();
            (double, Dictionary<double, double>) vote;
            foreach (var tree in trees)
            {
                vote = tree.PredictWithProbabilities(
                    CsML.Util.Matrix.FromList2D(input))[0];
                if (counts.ContainsKey(vote.Item1)) counts[vote.Item1] += 1;
                else counts[vote.Item1] = 1;
                foreach (double key in vote.Item2.Keys)
                {
                    if (probs.ContainsKey(key))
                        probs[key] += vote.Item2[key];
                    else
                        probs[key] = vote.Item2[key];
                }
            }
            double sumprobs = probs.Values.Sum();
            foreach (double key in probs.Keys)
                probs[key] = probs[key] / sumprobs;
            result[i] = (counts.MaxBy(kvp => kvp.Value).Key, probs);
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
        var result = new double[minColumns];
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