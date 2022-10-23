namespace CsML.Forest;

using CsML.Tree;

/// <summary>
/// A forest of binary decision trees for classification and regression.
/// </summary>
public class RandomForest
{

    public List<BinaryTree> trees;
    public int treeCount;
    public int minColumns;
    public int inputRecordCount;
    public int maxdepth = 1000;
    public int minrows = 3;
    public int randomFeatures;
    public double[]? classes;
    public Func<double[], double> purityFn;

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

    public RandomForest(string mode, Func<double[], double> purityFn, int treeCount = 103, int randomFeatures = 0)
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
    /// Perform forest induction from the dataset contained in the matrix
    /// parameter and values contained in target parameter.
    /// </summary>
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
        if (_mode == "classification")
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
            trees.Add(tree);
        }
        Parallel.ForEach(trees, tree => tree.Train(matrix, target, true));
    }

}