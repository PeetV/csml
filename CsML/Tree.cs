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
    // Private properties
    public List<BinaryNode> nodes;
    public int minColumns;
    private int _recursions;
    private int _splitCount;
    private int _depth;
    private int _inputRecordCount;

    // Public properties
    public int maxrecursions = 10000;
    public int maxsplits = 10000;
    public int maxdepth = 15;
    public int minrows = 3;
    public int randomfeatures = -1;
    public double[]? classes;

    // Backing fields
    private string _mode;
    private string _purityfn;

    public string treemode
    {
        get { return _mode; }
        set
        {
            if (value != "classify" & value != "regress")
                throw new ArgumentException("Mode must be 'classify' or 'regress'");
            _mode = value;
        }
    }

    public string purityfn
    {
        get { return _purityfn; }
        set
        {
            if (value != "gini")
                throw new ArgumentException("Purity function must be 'gini'");
            _purityfn = value;
        }
    }

    public BinaryTree(string mode, string purityfn)
    {
        nodes = new List<BinaryNode>();
        _recursions = 0;
        _splitCount = 0;
        minColumns = 0;
        _depth = 0;
        _inputRecordCount = 0;
        _mode = "classify";
        _purityfn = "gini";
        this.treemode = mode;
        this.purityfn = purityfn;
    }

    /// <summary>
    /// Perform decision tree induction from the dataset contained in the matrix
    /// parameter and values contained in target parameter.
    /// </summary>
    public void Train(double[,] matrix, double[] target, bool skipchecks=false)
    {
        _inputRecordCount = matrix.GetLength(0);
        int targetLength = target.Length;
        if (!skipchecks)
        {
            if (_inputRecordCount == 0 | targetLength == 0)
                throw new ArgumentException("Empty input");
            if (_inputRecordCount != targetLength) 
                throw new ArgumentException("Inputs must be the same length");
        }
        nodes = new List<BinaryNode>();
        _recursions = 0;
        _splitCount = 0;
        minColumns = matrix.GetLength(1);
        _depth = 0;
        if (_mode == "classification")
            classes = target.Distinct().ToArray();
        Grow((double[,])matrix.Clone(), (double[])target.Clone(), 0);
    }

    /// <summary>
    /// Infer target values from new data.
    /// </summary>
    public double[] Predict(double[,] matrix, bool skipchecks=false)
    {
        _inputRecordCount = matrix.GetLength(0);
        if (!skipchecks)
        {
            if (nodes.Count == 0)
                throw new ArgumentException("Tree is untrained");
            if (matrix.GetLength(1) != minColumns)
                throw new ArgumentException("Tree trained on different number of columns");
            if (_inputRecordCount == 0)
                throw new ArgumentException("Empty input");
        }
        Span2D<double> matrixSpan = matrix;
        double[] result = new double[_inputRecordCount];
        for (int i = 0; i < _inputRecordCount; i++)
        {
            double[] row = matrixSpan.GetRow(i).ToArray();
            int iterations = 0;
            BinaryNode node = nodes[0];
            while (iterations < maxrecursions)
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

    private int AddLeaf(double[] target)
    {
        int leafIndex = nodes.Count;
        int recordCount = target.Length;
        Dictionary<double, int>? classCounts;
        double predicted;
        if (_mode == "regresion")
        {
            classCounts = null;
            predicted = target.Average();
        }
        else
        {
            classCounts = CsML.Util.Array.ToElementCounts(target);
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

    private Func<double[], double> LookupPurityfn(string name)
    {
        return name switch
        {
            "gini" => Util.Statistics.Gini,
            _ => Util.Statistics.Gini
        };
    }

    private int Grow(double[,]? matrix, double[]? target, int parentDepth)
    {
        _recursions += 1;
        int depth = parentDepth + 1;
        int recordCount = matrix!.GetLength(0);
        if (depth > _depth) _depth = depth;
        if (_recursions > maxrecursions ||
            _depth > maxdepth ||
            recordCount < minrows ||
            target!.All(val => val.Equals(target![0])) ||
            _splitCount > maxsplits)
            return AddLeaf(target!);
        Func<double[], double> purityfn = LookupPurityfn(_purityfn);
        var bs = Util.Matrix.BestSplit<double>(matrix, target!, purityfn, randomfeatures);
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