using Microsoft.Toolkit.HighPerformance;

using CsML.Extensions;

namespace CsML.Tree;

/// <summary>Enumeration of model types e.g. to set model mode.</summary>
public enum ModelType
{
    /// <summary>Make inferences about class labels.</summary>
    Classification,
    /// <summary>Infer values.</summary>
    Regression
}

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
    /// <seealso cref="Utility.Statistics.Gini" />
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

    /// <summary>Mode defined by ModelType enum.</summary>
    public ModelType Mode = ModelType.Classification;

    /// <summary>Create an untrained model.</summary>
    public BinaryTree(ModelType mode, Func<double[], double> purityFn)
    {
        _recursions = 0;
        _splitCount = 0;
        _depth = 0;
        nodes = new List<BinaryNode>();
        minColumns = 0;
        inputRecordCount = 0;
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
    /// /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length.
    /// </exception>
    public void Train(
        double[,] matrix, double[] target, bool skipchecks = false)
    {
        inputRecordCount = matrix.GetLength(0);
        int targetLength = target.Length;
        if (!skipchecks)
        {
            if (inputRecordCount == 0 | targetLength == 0)
                throw new ArgumentException(Utility.ErrorMessages.E1);
            if (inputRecordCount != targetLength)
                throw new ArgumentException(Utility.ErrorMessages.E2);
        }
        nodes = new List<BinaryNode>();
        minColumns = matrix.GetLength(1);
        _recursions = 0;
        _splitCount = 0;
        _depth = 0;
        if (Mode == ModelType.Classification)
            classes = target.Distinct().ToArray();
        double[,] inputm;
        double[] inputt;
        if (bootstrapSampleData)
            if (retainOutOfBagIndeces)
                (inputm, inputt, outOfBagIndeces) = Utility.Features.
                    Bootstrap(matrix, target, returnOobIdx: true);
            else
                (inputm, inputt, _) = Utility.Features.
                    Bootstrap(matrix, target, returnOobIdx: false);
        else
        {
            inputm = (double[,])matrix.Clone();
            inputt = (double[])target.Clone();
        }
        Grow(inputm, inputt, 0);
    }

    /// <summary>Make predictions using the model.</summary>
    /// <param name="matrix">New data to infer predictions from.</param>
    /// <param name="skipchecks">
    /// Skip input checks if true. Used in random forest to avoid repeated
    /// checking of same input data.
    /// </param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if input is empty, or model has not been trained, or if trained
    /// on a different number of columns.
    /// </exception>
    public double[] Predict(double[,] matrix, bool skipchecks = false)
    {
        inputRecordCount = matrix.GetLength(0);
        if (!skipchecks)
        {
            if (inputRecordCount == 0)
                throw new ArgumentException(Utility.ErrorMessages.E1);
            if (nodes.Count == 0)
                throw new ArgumentException(Utility.ErrorMessages.E3);
            if (matrix.GetLength(1) != minColumns)
                throw new ArgumentException(Utility.ErrorMessages.E4);

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
    /// <exception cref="System.ArgumentException">
    /// Thrown if input is empty, or model has not been trained, or if trained
    /// on a different number of columns.
    /// </exception>
    public (double, Dictionary<double, double>)[] PredictWithProbabilities(
        double[,] matrix,
        bool skipchecks = false)
    {
        inputRecordCount = matrix.GetLength(0);
        if (!skipchecks)
        {
            if (inputRecordCount == 0)
                throw new ArgumentException(Utility.ErrorMessages.E1);
            if (nodes.Count == 0)
                throw new ArgumentException(Utility.ErrorMessages.E3);
            if (matrix.GetLength(1) != minColumns)
                throw new ArgumentException(Utility.ErrorMessages.E4);
            if (Mode == ModelType.Regression)
                throw new ArgumentException(Utility.ErrorMessages.E6);
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
        if (Mode == ModelType.Regression)
        {
            classCounts = null;
            predicted = target.Average();
        }
        else
        {
            classCounts = target.ElementCounts();
            predicted = classCounts.MaxBy(kvp => kvp.Value).Key;
        }
        BinaryNode node = new()
        {
            index = leafIndex,
            isLeaf = true,
            recordCount = recordCount,
            classCounts = classCounts,
            predicted = predicted
        };
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
        var bs = Utility.Matrix.BestSplit<double>(
                    matrix, target!, purityFn, randomFeatures);
        var sm = Utility.Matrix.Split(matrix, bs.Item1, bs.Item2);
        var st = target!.Split(sm.Item3);
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
        BinaryNode node = new()
        {
            index = nodeIndex,
            isLeaf = false,
            columnIndex = bs.Item1,
            splitPoint = bs.Item2,
            purityGain = bs.Item3,
            recordCount = recordCount
        };
        nodes.Add(node);
        node.yesIndex = Grow(sm.Item1, st.Item1, depth);
        node.noIndex = Grow(sm.Item2, st.Item2, depth);
        return nodeIndex;
    }
}

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
    /// <seealso cref="Utility.Statistics.Gini" />
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

    /// <summary>Mode defined by ModelType enum.</summary>
    public ModelType Mode = ModelType.Classification;

    /// <summary>Create an untrained random forest.</summary>
    public RandomForest(ModelType mode,
        Func<double[], double> purityFn,
        int treeCount = 103,
        int randomFeatures = 0)
    {
        trees = new List<BinaryTree>();
        minColumns = 0;
        inputRecordCount = 0;
        this.randomFeatures = randomFeatures;
        Mode = mode;
        this.purityFn = purityFn;
        this.treeCount = treeCount;
    }

    /// <summary>
    /// Calculate the number of features to include in node split.
    /// </summary>
    /// <param name="n">Number of columns.</param>
    /// <returns>Number of features to split on.</returns>
    public static int DefaultFeatureCount(int n)
    {
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
            throw new ArgumentException(Utility.ErrorMessages.E1);
        if (inputRecordCount != targetLength)
            throw new ArgumentException(Utility.ErrorMessages.E2);
        trees = new List<BinaryTree>();
        minColumns = matrix.GetLength(1);
        if (Mode == ModelType.Classification)
            classes = target.Distinct().ToArray();
        randomFeatures = randomFeatures == 0 ?
                         DefaultFeatureCount(minColumns) :
                         randomFeatures;
        foreach (int cntr in Enumerable.Range(0, treeCount))
        {
            // Note that mode and purityFn is set in the constructor
            var tree = new BinaryTree(this.Mode, this.purityFn)
            {
                maxdepth = this.maxdepth,
                minrows = this.minrows,
                randomFeatures = this.randomFeatures,
                bootstrapSampleData = this.bootstrapSampleData,
                retainOutOfBagIndeces = this.retainOutOfBagIndeces
            };
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
            throw new ArgumentException(Utility.ErrorMessages.E1);
        if (trees.Count == 0)
            throw new ArgumentException(Utility.ErrorMessages.E3);
        if (matrix.GetLength(1) != minColumns)
            throw new ArgumentException(Utility.ErrorMessages.E4);
        var result = new double[inputRecordCount];
        Parallel.For(0, inputRecordCount, i =>
        {
            var input = new List<double[]>();
            input = input.Append(
                Utility.Matrix.GetRow(matrix, i, false)).ToList();
            if (Mode == ModelType.Regression)
            {
                var predictions = new List<double>(trees.Count);
                foreach (var tree in trees)
                    predictions.Add(
                        tree.Predict(
                            Utility.Matrix.FromListRows(input))[0]);
                result[i] = predictions.Average();
            }
            else
            {
                var counts = new CsML.Probability.Counter<double>();
                double vote;
                foreach (var tree in trees)
                {
                    vote = tree.Predict(Utility.Matrix.FromListRows(input))[0];
                    counts.Increment(vote);
                }
                result[i] = counts.MaxKey();
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
            if (inputRecordCount == 0)
                throw new ArgumentException(Utility.ErrorMessages.E1);
            if (trees.Count == 0)
                throw new ArgumentException(Utility.ErrorMessages.E3);
            if (matrix.GetLength(1) != minColumns)
                throw new ArgumentException(Utility.ErrorMessages.E4);
            if (Mode == ModelType.Regression)
                throw new ArgumentException(Utility.ErrorMessages.E6);
        }
        var result = new (double, Dictionary<double, double>)[inputRecordCount];
        Parallel.For(0, inputRecordCount, i =>
        {
            var input = new List<double[]>();
            input = input.Append(
                Utility.Matrix.GetRow(matrix, i, false)).ToList();
            var counts = new CsML.Probability.Counter<double>();
            var probs = new Dictionary<double, double>();
            (double, Dictionary<double, double>) vote;
            foreach (var tree in trees)
            {
                vote = tree.PredictWithProbabilities(
                    Utility.Matrix.FromListRows(input))[0];
                counts.Increment(vote.Item1);
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
            result[i] = (counts.MaxKey(), probs);
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