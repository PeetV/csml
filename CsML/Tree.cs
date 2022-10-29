using Microsoft.Toolkit.HighPerformance;

namespace CsML.Tree;

/// <summary>
/// A binary decision tree node used to capture decision criteria for decision nodes
/// or inference data for leaf nodes.
/// </summary>
public class BinaryNode
{
    public int index;
    public bool isLeaf;
    // Decision node criteria
    public int? columnIndex;
    public double? splitPoint; // value used for split (> goes yes, <= no)
    public int? yesIndex;
    public int? noIndex;
    // Leaf node data
    public int? recordCount;
    public Dictionary<double, int>? classCounts;
    public double? purityGain;
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
    private int _maxrecursions = 10000;
    private int _maxsplits = 10000;

    public List<BinaryNode> nodes;
    public int minColumns;
    public int inputRecordCount;
    public int maxdepth = 15;
    public int minrows = 3;
    public int randomFeatures = -1;
    public double[]? classes;
    public Func<double[], double> purityFn;
    public bool bootstrapSampleData = false;

    private string _mode;
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

    public BinaryTree(string mode, Func<double[], double> purityFn)
    {
        nodes = new List<BinaryNode>();
        _recursions = 0;
        _splitCount = 0;
        minColumns = 0;
        _depth = 0;
        inputRecordCount = 0;
        _mode = "classify";
        this.mode = mode;
        this.purityFn = purityFn;
    }

    /// <summary>
    /// Perform decision tree induction from the dataset contained in the matrix
    /// parameter and values contained in target parameter.
    /// </summary>
    public void Train(double[,] matrix, double[] target, bool skipchecks = false)
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
            (inputm, inputt) = CsML.Util.Features.Bootstrap(matrix, target);
        else
        {
            inputm = (double[,])matrix.Clone();
            inputt = (double[])target.Clone();
        }
        Grow(inputm, inputt, 0);
    }

    /// <summary>
    /// Infer target values from new data.
    /// </summary>
    public double[] Predict(double[,] matrix, bool skipchecks = false)
    {
        inputRecordCount = matrix.GetLength(0);
        if (!skipchecks)
        {
            if (nodes.Count == 0)
                throw new ArgumentException("Tree is untrained");
            if (matrix.GetLength(1) != minColumns)
                throw new ArgumentException("Tree trained on different number of columns");
            if (inputRecordCount == 0)
                throw new ArgumentException("Empty input");
        }
        Span2D<double> matrixSpan = matrix;
        double[] result = new double[inputRecordCount];
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
    /// Predict labels for new data and return corresponding class counts to use for
    /// probability estimates.
    /// </summary>
    public (double, Dictionary<double, int>)[] PredictWithClassCounts(
        double[,] matrix,
        bool skipchecks = false)
    {
        inputRecordCount = matrix.GetLength(0);
        if (!skipchecks)
        {
            if (nodes.Count == 0)
                throw new ArgumentException("Tree is untrained");
            if (matrix.GetLength(1) != minColumns)
                throw new ArgumentException("Tree trained on different number of columns");
            if (inputRecordCount == 0)
                throw new ArgumentException("Empty input");
            if (mode == "regress")
                throw new ArgumentException("Probabilities require treemode to be 'classify'");
        }
        Span2D<double> matrixSpan = matrix;
        var result = new (double, Dictionary<double, int>)[inputRecordCount];
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
                    result[i] = ((double)node.predicted!, node.classCounts!);
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
        double[] result = new double[minColumns];
        foreach (BinaryNode node in nodes)
        {
            if (node.isLeaf) continue;
            int idx = (int)node.columnIndex!;
            double weight = (double)node.recordCount! / (double)inputRecordCount;
            double weighted = (double)node.purityGain! * weight;
            result[idx] = result[idx] + weighted;
        }
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
            classCounts = CsML.Util.Array.ElementCounts(target);
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
        var bs = Util.Matrix.BestSplit<double>(matrix, target!, purityFn, randomFeatures);
        var sm = Util.Matrix.Split(matrix, bs.Item1, bs.Item2.Item1);
        var st = Util.Array.Split(target!, sm.Item2);
        int yesLength = sm.Item1.Item1.GetLength(0);
        int noLength = sm.Item1.Item2.GetLength(0);
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
        node.splitPoint = bs.Item2.Item1;
        node.purityGain = bs.Item2.Item2;
        node.recordCount = recordCount;
        nodes.Add(node);
        node.yesIndex = Grow(sm.Item1.Item1, st.Item1, depth);
        node.noIndex = Grow(sm.Item1.Item2, st.Item2, depth);
        return nodeIndex;
    }
}