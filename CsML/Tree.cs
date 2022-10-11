namespace CsML.Tree;

/// <summary>
/// A binary decision tree node used to capture decision criteria for 
/// decision nodes or inference data for leaf nodes.
/// </summary>
public struct BinaryNode<T>
    where T : notnull
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
    public Dictionary<T, int>? classCounts;
    public double? purityGain;
    public double? predicted;
}

/// <summary>
/// A binary decision tree for classification and regression.
/// </summary>
public class BinaryTree<T>
    where T : notnull
{
    // Private properties
    private List<BinaryNode<T>> _nodes;
    private int _recursions;
    private int _splitCount;
    private int _minColumns;
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
            if (value != "classify" | value != "regress")
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

    public BinaryTree(string treemode, string purityfn)
    {
        _nodes = new List<BinaryNode<T>>();
        _recursions = 0;
        _splitCount = 0;
        _minColumns = 0;
        _depth = 0;
        _inputRecordCount = 0;
        _mode = "classify";
        _purityfn = "gini";
        this.treemode = treemode;
        this.purityfn = purityfn;
    }

    /// <summary>
    /// Perform decision tree induction from the dataset contained in the matrix
    /// parameter and values contained in target parameter.
    /// </summary>
    public void Train(double[,] matrix, double[] target)
       
    {
        _nodes = new List<BinaryNode<T>>();
        _recursions = 0;
        _splitCount = 0;
        _minColumns = matrix.GetLength(1);
        _depth = 0;
        _inputRecordCount = matrix.GetLength(0);
        if (_mode == "classification")
            classes = target.Distinct().ToArray();
        Grow((double[,])matrix.Clone(), (double[])target.Clone(), 0);
    }

    private void AddLeaf(double[] target)
    {
        int leafIndex = _nodes.Count;
        int recordCount = target.Length;
        // int? classCounts;
        double predicted;
        if (_mode == "regresion")
        {
            // classCounts = null;
            predicted = target.Average();
        }
        else
        {
            // classCounts
        }
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
            AddLeaf(target!);
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
            AddLeaf(target!);
        // Release memory
        matrix = null;
        target = null;
        int nodeIndex = _nodes.Count;
        _splitCount += 1;
        BinaryNode<T> node = new BinaryNode<T>();
        node.index = nodeIndex;
        node.isLeaf = false;
        node.columnIndex = bs.Item1;
        node.splitPoint = bs.Item2.Item1;
        node.purityGain = bs.Item2.Item2;
        node.recordCount = recordCount;
        _nodes.Add(node);
        node.yesIndex = Grow(sm.Item1.Item1, st.Item1, depth);
        node.noIndex = Grow(sm.Item1.Item2, st.Item2, depth);
        return nodeIndex;
    }
}