using Microsoft.Toolkit.HighPerformance;

using CsML.Extensions;

namespace CsML.Tree;

/// <summary>
/// A binary decision tree node used to capture decision criteria for decision
/// nodes or inference data for leaf nodes.
/// </summary>
public class BinaryNode
{
    /// <summary>Index of this node in list of nodes.</summary>
    public int index;

    /// <summary>Identify the node as a decision node vs a leaf node.</summary>
    public bool isLeaf;

    /// <summary>Column to evaluate in a dicision node.</summary>
    public int? columnIndex;

    /// <summary>
    /// Value used for split (greater than goes to yes index, else to no
    /// index).
    /// </summary>
    public double? splitPoint;

    /// <summary>
    /// Index of yes node to go to on evaluation of split point.
    /// </summary>
    public int? yesIndex;

    /// <summary>
    /// Index of no node to go to on evaluation of split point.
    /// </summary>
    public int? noIndex;

    /// <summary>Gain in purity on split.</summary>
    public double? purityGain;

    /// <summary>
    /// Number of records used to calculate leaf node metrics.
    /// </summary>
    public int? recordCount;

    /// <summary>Class counts in leaf node data.</summary>
    public Dictionary<double, int>? classCounts;

    /// <summary>Class most likely from leaf node data.</summary>
    public double? predicted;
}

/// <summary>
/// A binary decision tree for classification and regression.
/// </summary>
public class BinaryTree
{
    private int _recursions;
    private int _splitCount;
    private int _depth;
    private readonly int _maxrecursions = 10000;
    private readonly int _maxsplits = 10000;

    /// <summary>The list containing nodes in the tree once trained.</summary>
    public List<BinaryNode> nodes;

    /// <summary>
    /// The number of matrix columns the model was trained on.
    /// </summary>
    public int minColumns;

    /// <summary>The number of matrix rows the model was trained on.</summary>
    public int inputRecordCount;

    /// <summary>Maximum tree depth stopping condition.</summary>
    public int maxdepth = 15;

    /// <summary>Minimum rows in a tree stopping condition.</summary>
    public int minrows = 3;

    /// <summary>
    /// Number of random features to use at each split point during training.
    /// Used to add randomisation to a random forest. Defaults to square
    /// root of the number of columns. Not relevant if a single tree is trained
    /// (ignored if less than 0).
    /// </summary>
    public int randomFeatures = -1;

    /// <summary>
    /// The distinct class labels if creating a classification model.
    /// </summary>
    public double[]? classes;

    /// <summary>
    /// The function to use to calculate the purity of a slice of the
    /// target array.
    /// <see> See
    /// <seealso cref="CsML.Util.Statistics.Gini" />
    /// for default function to use.
    /// </see>
    /// </summary>
    public Func<double[], double> purityFn;

    /// <summary>Sample input data with replacement if true.</summary>
    public bool bootstrapSampleData = false;

    /// <summary>Keep out of bag indeces, if bootstrap sampling.</summary>
    public bool retainOutOfBagIndeces = false;

    /// <summary>
    /// The indeces of out of bag samples with reference to the input matrix
    /// the tree was trained on.
    /// </summary>
    public int[]? outOfBagIndeces;

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

    /// <summary>Create an untrained model.</summary>
    public BinaryTree(string mode, Func<double[], double> purityFn)
    {
        nodes = new List<BinaryNode>();
        _recursions = 0;
        _splitCount = 0;
        minColumns = 0;
        _depth = 0;
        inputRecordCount = 0;
        _mode = "classify";
        Mode = mode;
        this.purityFn = purityFn;
    }

    /// <summary>Train the model.</summary>
    /// <param name="matrix">The features to train the model on.</param>
    /// <param name="target">The target vector to train on.</param>
    /// <param name="skipchecks">
    /// Skip input checks if true. Used in random forest to avoid repeated
    /// checking of same input data.
    /// </param>
    public void Train(
        double[,] matrix, double[] target, bool skipchecks = false)
    {
        inputRecordCount = matrix.GetLength(0);
        int targetLength = target.Length;
        if (!skipchecks)
        {
            if (inputRecordCount == 0 | targetLength == 0)
                throw new ArgumentException("Empty input");
            if (inputRecordCount != targetLength)
                throw new ArgumentException("Inputs must be the same length");
        }
        nodes = new List<BinaryNode>();
        minColumns = matrix.GetLength(1);
        _recursions = 0;
        _splitCount = 0;
        _depth = 0;
        if (_mode == "classify")
            classes = target.Distinct().ToArray();
        double[,] inputm;
        double[] inputt;
        if (bootstrapSampleData)
            if (retainOutOfBagIndeces)
                (inputm, inputt, outOfBagIndeces) = CsML.Util.Features.
                    Bootstrap(matrix, target, returnOobIdx: true);
            else
                (inputm, inputt, _) = CsML.Util.Features.
                    Bootstrap(matrix, target, returnOobIdx: false);
        else
        {
            inputm = (double[,])matrix.Clone();
            inputt = (double[])target.Clone();
        }
        Grow(inputm, inputt, 0);
    }

    /// <summary>
    /// Make predictions using the model.
    /// </summary>
    /// <param name="matrix">New data to infer predictions from.</param>
    /// <param name="skipchecks">
    /// Skip input checks if true. Used in random forest to avoid repeated
    /// checking of same input data.
    /// </param>
    public double[] Predict(double[,] matrix, bool skipchecks = false)
    {
        inputRecordCount = matrix.GetLength(0);
        if (!skipchecks)
        {
            if (nodes.Count == 0)
                throw new ArgumentException("Tree is untrained");
            if (matrix.GetLength(1) != minColumns)
                throw new ArgumentException(
                    "Tree trained on different number of columns");
            if (inputRecordCount == 0)
                throw new ArgumentException("Empty input");
        }
        Span2D<double> matrixSpan = matrix;
        var result = new double[inputRecordCount];
        for (int i = 0; i < inputRecordCount; i++)
        {
            double[] row = matrixSpan.GetRow(i).ToArray();
            int iterations = 0;
            BinaryNode node = nodes[0];
            while (iterations < _maxrecursions)
            {
                iterations += 1;
                if (node.isLeaf)
                {
                    result[i] = (double)node.predicted!;
                    break;
                }
                if (row[(int)node.columnIndex!] > node.splitPoint)
                    node = nodes[(int)node.yesIndex!];
                else node = nodes[(int)node.noIndex!];
            }
        }
        return result;
    }

    /// <summary>
    /// Predict labels for new data and return corresponding class probability
    /// estimates.
    /// </summary>
    public (double, Dictionary<double, double>)[] PredictWithProbabilities(
        double[,] matrix,
        bool skipchecks = false)
    {
        inputRecordCount = matrix.GetLength(0);
        if (!skipchecks)
        {
            if (nodes.Count == 0)
                throw new ArgumentException("Tree is untrained");
            if (matrix.GetLength(1) != minColumns)
                throw new ArgumentException(
                    "Tree trained on different number of columns");
            if (inputRecordCount == 0)
                throw new ArgumentException("Empty input");
            if (Mode == "regress")
                throw new ArgumentException(
                    "Probabilities require treemode to be 'classify'");
        }
        Span2D<double> matrixSpan = matrix;
        var result = new (double, Dictionary<double, double>)[inputRecordCount];
        for (int i = 0; i < inputRecordCount; i++)
        {
            double[] row = matrixSpan.GetRow(i).ToArray();
            int iterations = 0;
            BinaryNode node = nodes[0];
            while (iterations < _maxrecursions)
            {
                iterations += 1;
                if (node.isLeaf)
                {
                    var probs = ClassCountsToProbabilities(node.classCounts!);
                    result[i] = ((double)node.predicted!, probs);
                    break;
                }
                if (row[(int)node.columnIndex!] > node.splitPoint)
                    node = nodes[(int)node.yesIndex!];
                else node = nodes[(int)node.noIndex!];
            }
        }
        return result;
    }

    /// <summary>
    /// Calculate the mean purity gain across nodes, weighted by the number of
    /// samples considered at each split as a proportion of total samples.
    /// </summary>
    /// <returns>
    /// An array containing purity gains in the order of feature columns.
    /// </returns>
    public double[] PurityGains()
    {
        var result = new double[minColumns];
        foreach (BinaryNode node in nodes)
        {
            if (node.isLeaf) continue;
            int idx = (int)node.columnIndex!;
            double weight = (double)node.recordCount! /
                            (double)inputRecordCount;
            double weighted = (double)node.purityGain! * weight;
            result[idx] = result[idx] + weighted;
        }
        return result;
    }

    private static Dictionary<double, double> ClassCountsToProbabilities(
        Dictionary<double, int> counts)
    {
        double total = counts.Values.Sum();
        var result = new Dictionary<double, double>();
        foreach (double key in counts.Keys)
            result[key] = counts[key] / total;
        return result;
    }

    private int AddLeaf(double[] target)
    {
        int leafIndex = nodes.Count;
        int recordCount = target.Length;
        Dictionary<double, int>? classCounts;
        double predicted;
        if (_mode == "regress")
        {
            classCounts = null;
            predicted = target.Average();
        }
        else
        {
            classCounts = target.ElementCounts();
            predicted = classCounts.MaxBy(kvp => kvp.Value).Key;
        }
        BinaryNode node = new BinaryNode();
        node.index = leafIndex;
        node.isLeaf = true;
        node.recordCount = recordCount;
        node.classCounts = classCounts;
        node.predicted = predicted;
        nodes.Add(node);
        return leafIndex;
    }

    private int Grow(double[,]? matrix, double[]? target, int parentDepth)
    {
        _recursions += 1;
        int depth = parentDepth + 1;
        int recordCount = matrix!.GetLength(0);
        if (depth > _depth) _depth = depth;
        if (_recursions > _maxrecursions ||
            _depth > maxdepth ||
            recordCount < minrows ||
            target!.All(val => val.Equals(target![0])) ||
            _splitCount > _maxsplits)
            return AddLeaf(target!);
        var bs = Util.Matrix.BestSplit<double>(
                    matrix, target!, purityFn, randomFeatures);
        var sm = Util.Matrix.Split(matrix, bs.Item1, bs.Item2);
        var st = Util.Array.Split(target!, sm.Item3);
        int yesLength = sm.Item1.GetLength(0);
        int noLength = sm.Item2.GetLength(0);
        if (yesLength == 0 ||
            noLength == 0 ||
            yesLength < minrows ||
            noLength < minrows)
            return AddLeaf(target!);
        // Release memory
        matrix = null;
        target = null;
        int nodeIndex = nodes.Count;
        _splitCount += 1;
        BinaryNode node = new BinaryNode();
        node.index = nodeIndex;
        node.isLeaf = false;
        node.columnIndex = bs.Item1;
        node.splitPoint = bs.Item2;
        node.purityGain = bs.Item3;
        node.recordCount = recordCount;
        nodes.Add(node);
        node.yesIndex = Grow(sm.Item1, st.Item1, depth);
        node.noIndex = Grow(sm.Item2, st.Item2, depth);
        return nodeIndex;
    }
}