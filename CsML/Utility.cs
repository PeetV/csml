using Microsoft.Toolkit.HighPerformance;

using CsML.Extensions;

namespace CsML.Utility;

/// <summary>A collection of array utility functions.</summary>
public static class Arrays
{
    /// <summary>
    /// Determine which value to split an array on to maximise the weighted
    /// gain in purity when the split is applied to a corresponding target
    /// array.
    /// </summary>
    /// <param name="vector">Numeric array to test split points on.</param>
    /// <param name="target">
    /// Target array to find best gain in purity on split.
    /// </param>
    /// <param name="purityfn">
    /// Function that calculates purity of an array, for example Gini.
    /// </param>
    /// <returns>
    /// A tuple containing the best split value and the gain calculated by the
    /// gain function specified through the purityfn parameter.
    /// </returns>
    public static (double, double)
    BestSplit<T>(
        double[] vector,
        T[] target,
        Func<T[], double> purityfn
    )
        where T : notnull
    {
        if (vector.Length == 0) return (0.0, 0.0);
        double bestsplit = 0.0, bestgain = 0.0;
        int lenVals = vector.Length;
        (double, T)[] zipped = vector
                                .Zip(target)
                                .OrderBy(x => x.First)
                                .ToArray();
        List<T> lhs = new(), rhs = new(target);
        double purityPreSplit = purityfn(target);
        bool allSame = true;
        // Iterate through the sorted arrays
        for (int idx = 0; idx < lenVals - 1; idx++)
        {
            T targetval = zipped[idx].Item2;
            int targetvalIdx = rhs.IndexOf(targetval);
            // Add to the left
            lhs.Add(targetval);
            // Remove from the right
            rhs.Remove(targetval);
            // Ignore this split potential if not unique
            if (zipped[idx].Item1 == zipped[idx + 1].Item1) continue;
            allSame = false;
            // Ignore this split, if not enough data for averages
            if (lhs.Count == 0 || rhs.Count == 0) continue;
            // Calculate Gini index
            double slice1Purity = purityfn(lhs.ToArray()) * lhs.Count / lenVals;
            double slice2Purity = purityfn(rhs.ToArray()) * rhs.Count / lenVals;
            double gain = purityPreSplit - slice1Purity - slice2Purity;
            if (gain > bestgain)
            {
                var midpoint = (zipped[idx].Item1 +
                                zipped[idx + 1].Item1) / 2.0;
                bestgain = gain;
                bestsplit = midpoint;
            }
        }
        if (allSame)
            return (zipped[0].Item1 - 1.0, 0.0);
        return (bestsplit, bestgain);
    }

    /// <summary>
    /// Calculate classification accuracy from a predictions array compared to
    /// an actuals array.
    /// </summary>
    /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length.
    /// </exception>
    public static double ClassificationAccuracy<T>(
        T[] actuals,
        T[] predictions
    )
        where T : IComparable<T>
    {
        int lenActuals = actuals.Length, lenPredictions = predictions.Length;
        if (lenActuals != lenPredictions)
            throw new ArgumentException(ErrorMessages.E2);
        if (lenActuals == 0) return 0.0;
        return actuals.Zip(predictions)
                .Select(x => x.First.CompareTo(x.Second) == 0 ? 1.0 : 0.0)
                .Sum() / lenActuals;
    }

    /// <summary>
    /// Calculate classification error from a predictions array compared to an
    /// actuals array.
    /// </summary>
    /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length.
    /// </exception>
    public static double ClassificationError<T>(
        T[] actuals, T[] predictions) where T : IComparable<T>
    {
        return 1.0 - ClassificationAccuracy(actuals, predictions);
    }

    /// <summary>
    /// Calculate Precision (proportion of positives predictived correctly)
    /// and Recall (proportion of true positives found) from a predictions
    /// array compared to an actuals array.
    /// </summary>
    /// <returns>
    /// A dictionary with distinct elements as keys and a tuple containing
    /// precision and recall as the value.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length.
    /// </exception>
    public static Dictionary<T, (double, double)>
    ClassificationMetrics<T>(
        T[] actuals,
        T[] predictions
    )
        where T : IComparable<T>
    {
        int lenActuals = actuals.Length, lenPredictions = predictions.Length;
        if (lenActuals != lenPredictions)
            throw new ArgumentException(ErrorMessages.E2);
        (T, T)[] zipped = actuals.Zip(predictions).ToArray();
        Dictionary<T, double[]> counts = new();
        foreach ((T, T) pair in zipped)
        {
            // True positive
            if (pair.Item1.CompareTo(pair.Item2) == 0)
            {
                if (counts.ContainsKey(pair.Item1))
                {
                    double[] vals = counts[pair.Item1];
                    vals[0] += 1;
                    counts[pair.Item1] = vals;
                }
                else counts[pair.Item1] = new double[] { 1.0, 0.0, 0.0 };
                continue;
            }
            // False positive
            if (counts.ContainsKey(pair.Item2))
            {
                double[] vals = counts[pair.Item2];
                vals[1] += 1;
                counts[pair.Item2] = vals;
            }
            else counts[pair.Item2] = new double[] { 0.0, 1.0, 0.0 };
            // False negative
            if (counts.ContainsKey(pair.Item1))
            {
                double[] vals = counts[pair.Item1];
                vals[2] += 1;
                counts[pair.Item1] = vals;
            }
            else counts[pair.Item1] = new double[] { 0.0, 0.0, 1.0 };
        }
        Dictionary<T, (double, double)> result = new();
        double[] tpfpfn;
        double prec, rec;
        foreach (T k in counts.Keys)
        {
            tpfpfn = counts[k];
            prec = tpfpfn[0] / (tpfpfn[0] + tpfpfn[1]);
            rec = tpfpfn[0] / (tpfpfn[0] + tpfpfn[2]);
            result[k] = (prec, rec);
        }
        return result;
    }

    /// <summary>
    /// Calculate the Euclidian distqnce between two double arrays.
    /// </summary>
    /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length.
    /// </exception>
    public static double DistanceEuclidian(double[] a, double[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException(ErrorMessages.E2);
        return Math.Sqrt(
                 a.Zip(b)
                  .Select(x => Math.Pow(x.Item1 - x.Item2, 2))
                  .Sum());
    }

    /// <summary>
    /// Count of ocurrences of each array element.
    /// <see>Also see <seealso cref="CsML.Probability.Counter{T}">Counter
    /// </seealso></see></summary>
    /// <returns>
    /// A dictionary containing array elements as keys and element counts as
    /// values.
    /// </returns>
    public static Dictionary<T, int> ElementCounts<T>(T[] input)
        where T : notnull
    {
        Dictionary<T, int> counts = new();
        foreach (T item in input)
        {
            if (counts.ContainsKey(item)) counts[item] += 1;
            else counts[item] = 1;
        }
        return counts;
    }

    /// <summary>
    /// Split an array using a boolean filter array, with related true values 
    /// going to the left and false values going to the right.
    /// </summary>
    /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length.
    /// </exception>
    public static (T[], T[]) Split<T>(
        T[] input,
        bool[] filter)
    {
        if (input.Length != filter.Length)
            throw new ArgumentException(ErrorMessages.E2);
        List<T> lhs = new(), rhs = new();
        for (int index = 0; index < input.Length; index++)
        {
            if (filter[index])
                lhs.Add(input[index]);
            else
                rhs.Add(input[index]);
        }
        return (lhs.ToArray(), rhs.ToArray());
    }
}

/// <summary>Error message descriptions.</summary>
public static class ErrorMessages
{
    /// <summary>Error message if input is empty.</summary>
    public const string E1 = "Input must not be empty";

    /// <summary>Error message if input lengths differ.</summary>
    public const string E2 = "Inputs must be same length";

    /// <summary>Error message if model has not been trained.</summary>
    public const string E3 = "Model must be trained first";

    /// <summary>
    /// Error message if the model was trained on a different number of
    /// columns.
    /// </summary>
    public const string E4 = "Same number of columns as trained on needed";

    /// <summary>Error message if model mode is not recognised.</summary>
    public const string E5 = "Mode must be 'classify' or 'regress'";

    /// <summary>Error message if method does not apply given mode.</summary>
    public const string E6 = "Method only valid if method is 'classify'";

    /// <summary>Error message if maximum iterations are exceeded.</summary>
    public const string E7 = "Maximum iterations exceeded";
}

/// <summary>
/// A collection of functions that do work on model training inputs, comprising
/// a matrix of features and a target variable array.
/// </summary>
public static class Features
{
    /// <summary>Calculate class proportions in a target array.</summary>
    /// <param name="target">The target input array.</param>
    /// <returns>
    /// An array of tuples containing class labels and proportions.
    /// </returns>
    public static (T, int, double)[] ClassProportions<T>(T[] target)
        where T : notnull
    {
        var counts = target.ElementCounts();
        int total = counts.Values.Sum();
        return counts.Keys
            .OrderBy(x => x)
            .Select(key =>
                (key, counts[key], (double)counts[key] / (double)total))
            .ToArray();
    }

    /// <summary>
    /// Bootstrap sample from feature matrix and correspondnig target array,
    /// with replacement, e.g. to add ramdomisation to a Random Forest.
    /// </summary>
    /// <returns>
    /// A new matrix and target array containing bootstrap samples. The indeces
    /// of out of bag items.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length.
    /// </exception>
    public static (double[,], double[], int[])
    Bootstrap(
        double[,] matrix,
        double[] target,
        bool returnOobIdx = false)
    {
        int numRows = matrix.GetLength(0), numCols = matrix.GetLength(1);
        if (numRows != target.Length)
            throw new ArgumentException(ErrorMessages.E2);
        var resultmatrix = new double[numRows, numCols];
        var resulttarget = new double[numRows];
        var resultIndex = CsML.Probability
                            .Sampling.RangeWithReplacement(0, numRows, numRows);
        int[] oobidx;
        if (returnOobIdx)
            oobidx = Enumerable
                        .Range(0, numRows)
                        .Where(x => !resultIndex
                        .Contains(x))
                        .ToArray();
        else oobidx = System.Array.Empty<int>();
        int idx;
        for (int i = 0; i < numRows; i++)
        {
            idx = resultIndex[i];
            for (int c = 0; c < numCols; c++)
                resultmatrix[i, c] = matrix[idx, c];
            resulttarget[i] = target[idx];
        }
        return (resultmatrix, resulttarget, oobidx);
    }

    /// <summary>
    /// Shuffle a matrix containing features and a target array maintaining
    /// the relationship between matrix rows and array items.
    /// </summary>
    /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length.
    /// </exception>
    public static (double[,], double[])
    Shuffle(
        double[,] matrix,
        double[] target
    )
    {
        int inputLength = matrix.GetLength(0), inputWidth = matrix.GetLength(1);
        if (inputLength != target.Length)
            throw new ArgumentException(ErrorMessages.E2);
        int[] startingIndex = Enumerable.Range(0, inputLength).ToArray();
        int[] shuffledIndex = Probability.Shuffling.Shuffle(
                                startingIndex, inPlace: false);
        var fromtoIndex = startingIndex.Zip(shuffledIndex);
        var newmatrix = new double[inputLength, inputWidth];
        var newtarget = new double[inputLength];
        foreach (var (from, to) in fromtoIndex)
        {
            newtarget[from] = target[to];
            for (int colidx = 0; colidx < inputWidth; colidx++)
                newmatrix[from, colidx] = matrix[to, colidx];
        }
        return (newmatrix, newtarget);
    }

    /// <summary>
    /// Split a matrix containing features and a target variable array
    /// into train and test sets using a ratio between 0 and 1, e.g. 0.7
    /// places 70% into train and keeps 30% for test.
    /// </summary>
    /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length or if ratio parameter is
    /// not between 0 and 1.
    /// </exception>
    public static ((double[,], double[]), (double[,], double[]))
    Split(
        double[,] matrix,
        double[] target,
        double ratio
    )
    {
        if (ratio <= 0 | ratio >= 1)
            throw new ArgumentException("ratio must be between 0 and 1");
        int inputLength = matrix.GetLength(0), inputWidth = matrix.GetLength(1);
        if (inputLength != target.Length)
            throw new ArgumentException(ErrorMessages.E2);
        int[] index = Enumerable.Range(0, inputLength).ToArray();
        double cutPoint = (inputLength - 1) * ratio;
        bool[] filter = index.Select(x => x <= cutPoint).ToArray();
        double[,] mlhs, mrhs;
        double[] tlhs, trhs;
        (mlhs, mrhs) = CsML.Utility.Matrix.Split(matrix, filter);
        (tlhs, trhs) = target.Split(filter);
        return ((mlhs, tlhs), (mrhs, trhs));
    }

    /// <summary>Profile a column.</summary>
    public class ColumnProfile
    {
        /// <summary>The 25th percentile value.</summary>
        public double q25;
        /// <summary>The 50th percentile value.</summary>
        public double q50;
        /// <summary>The 75th percentile value.</summary>
        public double q75;
        /// <summary>The maximum value in the column.</summary>
        public double max;
        /// <summary>The average value in the column.</summary>
        public double mean;
        /// <summary>The minimum value in the column.</summary>
        public double min;
        /// <summary>The standard deviation.</summary>
        public double stdevp;
        /// <summary>The upper outlier boundary.</summary>
        public double outlierUpper;
        /// <summary>The lower outlier boundary.</summary>
        public double outlierLower;

        /// <summary>Create a new column profile from column data.</summary>
        public ColumnProfile(double[] columnData)
        {
            q25 = Statistics.PercentileLinear(columnData, 0.25);
            q50 = Statistics.PercentileLinear(columnData, 0.5);
            q75 = Statistics.PercentileLinear(columnData, 0.75);
            max = columnData.Max();
            mean = columnData.Average();
            stdevp = Statistics.StdevP(columnData);
            min = columnData.Min();
            (outlierLower, outlierUpper) = Statistics.OutlierBounds(columnData);
        }
    }

    /// <summary>
    /// Profile model training data, to monitor for data drift and retain
    /// scaling factors to apply to new data.
    /// </summary>
    public class Profiler
    {
        /// <summary>Column meta data.</summary>
        public List<ColumnProfile> columnData;
        /// <summary>Target column meta data.</summary>
        public ColumnProfile targetData;
        /// <summary>The number of columns profiled.</summary>
        public int minColumns;

        /// <summary>Create a new profiler.</summary>
        public Profiler(double[,] matrix, double[] target)
        {
            minColumns = matrix.GetLength(1);
            columnData = new List<ColumnProfile>();
            Span2D<double> matrixSpan = matrix;
            var columnIndeces = Enumerable.Range(0, minColumns).ToArray();
            foreach (int columnIndex in columnIndeces)
            {
                double[] col = matrixSpan.GetColumn(columnIndex).ToArray();
                columnData.Add(new ColumnProfile(col));
            }
            targetData = new ColumnProfile(target);
        }

        /// <summary>Get indeces of columns that have outliers.</summary>
        public int[] ColumnsWithOutliers(double[,] matrix)
        {
            if (matrix.GetLength(1) != minColumns)
                throw new ArgumentException(ErrorMessages.E4);
            var result = new List<int>();
            Span2D<double> matrixSpan = matrix;
            for (int columnIndex = 0; columnIndex < minColumns; columnIndex++)
            {
                double[] col = matrixSpan.GetColumn(columnIndex).ToArray();
                if (HasOutliers(col, columnIndex))
                    result.Add(columnIndex);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Test if all columns are within outlier boundaries.
        /// </summary>
        public bool HasOutliers(double[,] matrix)
        {
            if (matrix.GetLength(1) != minColumns)
                throw new ArgumentException(ErrorMessages.E4);
            Span2D<double> matrixSpan = matrix;
            for (int columnIndex = 0; columnIndex < minColumns; columnIndex++)
            {
                double[] col = matrixSpan.GetColumn(columnIndex).ToArray();
                if (HasOutliers(col, columnIndex))
                    return true;
            }
            return false;
        }

        /// <summary>Test if column values within outlier boundaries.</summary>
        public bool HasOutliers(double[] column, int columnIndex)
        {
            return column.Any(x =>
            {
                return x < columnData[columnIndex].outlierLower |
                       x > columnData[columnIndex].outlierUpper;
            });
        }

        /// <summary>Get the index of outlier values in a column.</summary>
        public int[] OutlierIndex(double[] column, int columnIndex)
        {
            if (column.Length == 0) return System.Array.Empty<int>();
            var lower = columnData[columnIndex].outlierLower;
            var upper = columnData[columnIndex].outlierUpper;
            var result = new List<int>();
            double rowval;
            for (int ri = 0; ri < column.Length; ri++)
            {
                rowval = column[ri];
                if ((rowval < lower) | (rowval > upper))
                    result.Add(ri);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Scale matrix columns to z-scores using column metrics captured at
        /// instantiation.
        /// </summary>
        /// <param name="matrix">
        /// The matrix to scale. Must be the same number of columns as matrix
        /// used to capture metrics at instantiation.
        /// </param>
        /// <param name="columns">
        /// Columns to scale. Scale all columns if null (default).
        /// </param>
        public double[,] ScaleZScore(double[,] matrix, int[]? columns = null)
        {
            if (matrix.GetLength(1) != minColumns)
                throw new ArgumentException(ErrorMessages.E4);
            var result = new List<double[]>();
            int[] cols = columns ?? Enumerable.Range(0, minColumns).ToArray();
            Span2D<double> matrixSpan = matrix;
            for (int columnIndex = 0; columnIndex < minColumns; columnIndex++)
            {
                var col = matrixSpan.GetColumn(columnIndex).ToArray();
                if (!cols.Contains(columnIndex))
                {
                    result.Add(col);
                    continue;
                }
                result.Add(col.Select(x =>
                {
                    return (x - columnData[columnIndex].mean) /
                            columnData[columnIndex].stdevp;
                }).ToArray());
            }
            return Matrix.FromListColumns(result);
        }
    }
}

/// <summary>CsML model interface.</summary>
public interface IModel
{
    /// <summary>Train the model.</summary>
    public void Train(double[,] matrix, double[] target);
    /// <summary>Infer target values from new data.</summary>
    public double[] Predict(double[,] matrix);
}

/// <summary>
/// A collection of matrix utility functions, using 2D arrays.
/// </summary>
public static class Matrix
{
    /// <summary>
    /// Determine which value to split a matrix (two dimensional array) on to
    /// maximise the weighted gain in purity when the split is applied to a
    /// corresponding target array.
    /// </summary>
    /// <param name="matrix">
    /// Numeric two dimensional array to test split points on.
    /// </param>
    /// <param name="target">
    /// Target array to find best gain in purity on split.
    /// </param>
    /// <param name="purityfn">
    /// Function that calculates purity of an array.
    /// </param>
    /// <param name="randomFeatures">
    /// If randomFeatures is greater than 0, select a random number of
    /// specified columns to include (used to add randomisation to a Random
    /// Forest).
    /// </param>
    /// <returns>
    /// A tuple containing the index of the column with best gain, the split
    /// value and gain calculated by the gain function specified through the
    /// purityfn parameter.
    /// </returns>
    public static (int, double, double)
    BestSplit<T>(
        double[,] matrix,
        T[] target,
        Func<T[], double> purityfn,
        int randomFeatures
    )
        where T : notnull
    {
        Span2D<double> matrixSpan = matrix;
        int[] columnIndeces;
        int columnCount = matrix.GetLength(1);
        columnIndeces = Enumerable.Range(0, columnCount).ToArray();
        if (randomFeatures > 0 & randomFeatures < columnCount)
            columnIndeces = Probability.Sampling
                .ArrayWithoutReplacement(columnIndeces, randomFeatures);
        double bestsplit = 0.0, bestgain = 0.0;
        int bestColumnIndex = 0;
        double split, gain;
        foreach (int columnIndex in columnIndeces)
        {
            double[] columnToCheck = matrixSpan
                                        .GetColumn(columnIndex)
                                        .ToArray();
            (split, gain) = Arrays.BestSplit(columnToCheck, target, purityfn);
            if (gain > bestgain)
            {
                bestgain = gain;
                bestsplit = split;
                bestColumnIndex = columnIndex;
            }
        }
        return (bestColumnIndex, bestsplit, bestgain);
    }

    /// <summary>Compare two matrixes for equality.</summary>
    public static bool Equal(double[,] a, double[,] b)
    {
        return
        // Check the number of dimensions
        a.Rank == b.Rank &&
        // Check if dimensions are the same size 
        Enumerable.Range(0, a.Rank).All(
            dimension => a.GetLength(dimension) == b.GetLength(dimension)
        ) &&
        // Cast to IEnumerable to use SequenceEqual to compare values
        a.Cast<double>().SequenceEqual(b.Cast<double>());
    }

    /// <summary>
    /// Create a two dimensional double array from a CSV file.
    /// </summary>
    /// <param name="inputfile">Path of the CSV input file.</param>
    /// <param name="mapping">
    /// A dictionary used to convert string columns to numeric values of the
    /// format  { {column id , { {string val, numeric val}, {string val,
    /// numeric val} ...}.
    /// </param>
    /// <param name="separator">The field delimeter.</param>
    /// <param name="loadFromRow">
    /// The index of the row to load from, e.g. 1 skips the first row.
    /// </param>
    public static double[,] FromCSV(
        string inputfile,
        Dictionary<int, Dictionary<string, double>>? mapping,
        string separator = ",",
        int loadFromRow = 0)
    {
        var rawdata = File
                        .ReadLines(inputfile)
                        .Select(x => x.Split(separator))
                        .ToArray();
        int rowcount = rawdata.Length, columncount = rawdata[0].Length;
        var result = new double[rowcount - loadFromRow, columncount];
        double cell;
        string rawval;
        for (int rowidx = loadFromRow; rowidx < rowcount; rowidx++)
        {
            for (int colidx = 0; colidx < columncount; colidx++)
            {
                rawval = rawdata[rowidx][colidx];
                if (mapping != null)
                {
                    if (mapping.TryGetValue(colidx, out var value))
                    {
                        cell = value[rawval];
                        result[rowidx - loadFromRow, colidx] = cell;
                        continue;
                    }
                }
                if (!Double.TryParse(rawval, out cell))
                    cell = 0.0;
                result[rowidx - loadFromRow, colidx] = cell;
            }
        }
        return result;
    }

    /// <summary>
    /// Create a two dimensional array from a List of columns.
    /// </summary>
    public static T[,] FromListColumns<T>(List<T[]> matrix)
    {
        int cols = matrix.Count;
        if (cols == 0) return new T[,] { };
        int rows = matrix[0].Length;
        T[,] result = new T[rows, cols];
        for (int ri = 0; ri < rows; ri++)
        {
            for (int ci = 0; ci < cols; ci++)
                result[ri, ci] = matrix[ci][ri];
        }
        return result;
    }

    /// <summary>Create a two dimensional array from a List of rows.</summary>
    public static T[,] FromListRows<T>(List<T[]> matrix)
    {
        int rows = matrix.Count;
        if (rows == 0) return new T[,] { };
        int cols = matrix[0].Length;
        T[,] result = new T[rows, cols];
        for (int ri = 0; ri < rows; ri++)
        {
            for (int ci = 0; ci < cols; ci++)
                result[ri, ci] = matrix[ri][ci];
        }
        return result;
    }

    /// <summary>Extract a column from a 2D array.</summary>
    /// <param name="matrix">The matrix to extract a column from.</param>
    /// <param name="index">The column number to extract.</param>
    /// <param name="useSpan">
    /// Use Span2D to extract the row if true e.g. set to false in 
    /// parallel code if it doesn't accept Span2D.
    /// </param> 
    public static double[] GetColumn(
        double[,] matrix, int index, bool useSpan = true)
    {
        if (useSpan)
        {
            Span2D<double> matrixSpan = matrix;
            return matrixSpan.GetColumn(index).ToArray();
        }
        int length = matrix.GetLength(0);
        var result = new double[length];
        for (int ridx = 0; ridx < length; ridx++)
            result[ridx] = matrix[ridx, index];
        return result;
    }

    /// <summary>Extract a row from a 2D array.</summary>
    /// <param name="matrix">The matrix to extract a row from.</param>
    /// <param name="index">The row number to extract.</param>
    /// <param name="useSpan">
    /// Use Span2D to extract the row if true e.g. set to false in 
    /// parallel code if it doesn't accept Span2D.
    /// </param> 
    public static double[] GetRow(
        double[,] matrix, int index, bool useSpan = true)
    {
        if (useSpan)
        {
            Span2D<double> matrixSpan = matrix;
            return matrixSpan.GetRow(index).ToArray();
        }
        int width = matrix.GetLength(1);
        var result = new double[width];
        for (int cidx = 0; cidx < width; cidx++)
            result[cidx] = matrix[index, cidx];
        return result;
    }

    /// <summary>
    /// Split a matrix (two dimensional double array) using a column
    /// index and split point.
    /// </summary>
    /// <param name="matrix">Numeric two dimensional array to split.</param>
    /// <param name="columnIndex">Column to split the matrix on.</param>
    /// <param name="splitPoint">
    /// Value to split the column on (greater than value splits left;
    /// less than value splits right).
    /// </param>
    /// <returns>
    /// Returns a Tuple containing the left and right splits
    /// and boolean filter array. The filter array can be applied to
    /// other arrays using Split1D.
    /// </returns>
    public static (double[,], double[,], bool[])
    Split(
            double[,] matrix,
            int columnIndex,
            double splitPoint
        )
    {
        List<double[]> lhs = new(), rhs = new();
        Span2D<double> matrixSpan = matrix;
        var filterColumn = matrixSpan.GetColumn(columnIndex).ToArray();
        bool[] filter = filterColumn.Select(x => x > splitPoint).ToArray();
        double[] row;
        for (int rowIndex = 0; rowIndex < matrix.GetLength(0); rowIndex++)
        {
            row = matrixSpan.GetRow(rowIndex).ToArray();
            if (filter[rowIndex])
                lhs.Add(row);
            else
                rhs.Add(row);
        }
        double[,] dlhs, drhs;
        if (lhs.Count != 0) dlhs = FromListRows(lhs);
        else dlhs = new double[,] { };
        if (rhs.Count != 0) drhs = FromListRows(rhs);
        else drhs = new double[,] { };
        return (dlhs, drhs, filter);
    }

    /// <summary>Split a matrix using a boolean filer matrix.</summary>
    /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length.
    /// </exception>
    public static (double[,], double[,])
    Split(
        double[,] matrix,
        bool[] filter
    )
    {
        if (matrix.GetLength(0) != filter.Length)
            throw new ArgumentException(ErrorMessages.E2);
        List<double[]> lhs = new(), rhs = new();
        Span2D<double> matrixSpan = matrix;
        double[] row;
        for (int rowIndex = 0; rowIndex < matrix.GetLength(0); rowIndex++)
        {
            row = matrixSpan.GetRow(rowIndex).ToArray();
            if (filter[rowIndex])
                lhs.Add(row);
            else
                rhs.Add(row);
        }
        double[,] dlhs, drhs;
        if (lhs.Count != 0) dlhs = FromListRows(lhs);
        else dlhs = new double[,] { };
        if (rhs.Count != 0) drhs = FromListRows(rhs);
        else drhs = new double[,] { };
        return (dlhs, drhs);
    }
}

/// <summary>Enumeration of model types e.g. to set model mode.</summary>
public enum ModelType
{
    /// <summary>Make inferences about class labels.</summary>
    Classification,
    /// <summary>Make inferences about values.</summary>
    Regression
}

/// <summary>A collection of statistics utility functions.</summary>
public static class Statistics
{
    /// <summary>
    /// Calculate the factorial of a number. n! = n x (n - 1) x (n - 2) ...
    /// </summary>
    public static int Factorial(int n)
    {
        if (n == 0) return 1;
        int fact = n;
        for (int i = n - 1; i >= 1; i--)
            fact *= i;
        return fact;
    }

    /// <summary>
    /// Calculate the Gini index of a set of discrete values.
    /// </summary>
    public static double Gini<T>(IEnumerable<T> vector) where T : notnull
    {
        int length = 0;
        // Get bin counts
        var counts = new Dictionary<T, int>();
        foreach (T val in vector)
        {
            if (counts.ContainsKey(val)) counts[val] += 1;
            else counts[val] = 1;
            length += 1;
        }
        // Return if empty input
        if (length == 0) return 0.0;
        // Calculate the Gini index
        double result = 1.0;
        double calc;
        foreach (KeyValuePair<T, int> entry in counts)
        {
            calc = entry.Value / (double)length;
            result -= calc * calc;
        }
        return result;
    }

    /// <summary>
    /// Calculate the upper and lower bounds for outliers with upper bound
    /// being 1.5 times Inter Quartile Range (IQR) plus 75th percentile and
    /// lower bound being 25th percentile les 1.5 times IQR.
    /// </summary>
    public static (double, double) OutlierBounds(double[] input)
    {
        double q25 = PercentileLinear(input, 0.25);
        double q75 = PercentileLinear(input, 0.75);
        double iqr = q75 - q25;
        return (q25 - iqr * 1.5, q75 + iqr * 1.5);
    }

    /// <summary>
    /// Calculate a percentile using linear interpolation.
    /// <see> See <seealso
    /// href="https://stackoverflow.com/questions/8137391/percentile-calculation">
    /// Stackoverflow</seealso>.
    /// </see>
    /// </summary>
    public static double PercentileLinear(double[] input, double percentile)
    {
        double[] sequence = input.OrderBy(x => x).ToArray();
        int N = sequence.Length;
        if (N == 0) return 0;
        double n = (N - 1) * percentile + 1;
        if (n == 1d) return sequence[0];
        else if (n == N) return sequence[N - 1];
        else
        {
            int k = (int)n;
            double d = n - k;
            return sequence[k - 1] + d * (sequence[k] - sequence[k - 1]);
        }
    }

    /// <summary>
    /// Calculate the r-squared and adjusted r-squared of an actuals array vs
    /// predictions.
    /// <see> See <seealso
    /// href="https://en.wikipedia.org/wiki/Coefficient_of_determination">
    /// Wikipedia</seealso> Coefficient Of Determination.
    /// </see>
    /// </summary>
    /// <param name="actuals">
    /// Actual values to compare with predicted values.
    /// </param>
    /// <param name="predictions">
    /// Predicted values to compare with actual values.
    /// </param>
    /// <param name="p">
    /// p is the number of explanatory terms used in the regression to
    /// calculated adjusted r-squared (returns 0 for adjusted r-squared if p is
    /// null).
    /// </param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length.
    /// </exception>
    /// <returns>A tuple containing r-squared and adjusted r-squared.</returns>
    public static (double, double)
    RSquared(
        double[] actuals,
        double[] predictions,
        int? p
    )
    {
        if (actuals.Length != predictions.Length)
            throw new ArgumentException(ErrorMessages.E2);
        double mn = actuals.Average();
        double sseVal = SSE(actuals, predictions);
        double sstVal = actuals.Select(x => Math.Pow(x - mn, 2)).Sum();
        double rsq = 1.0 - sseVal / sstVal;
        if (p == null)
            return (rsq, 0.0);
        double n = (double)actuals.Length;
        return (rsq, 1.0 - (1.0 - rsq) * ((n - 1) / (n - (double)p! - 1)));
    }

    /// <summary>
    /// Calculate the sum of the squared difference between two arrays.
    /// </summary>
    /// <exception cref="System.ArgumentException">
    /// Thrown if inputs aren't the same length.
    /// </exception>
    public static double SSE(double[] actuals, double[] predictions)
    {
        if (actuals.Length != predictions.Length)
            throw new ArgumentException(ErrorMessages.E2);
        IEnumerable<(double, double)> zipped = actuals.Zip(predictions);
        return zipped
                .Select(x => Math.Pow((x.Item1 - x.Item2), 2))
                .Sum();
    }

    /// <summary>
    /// Calculate the sum of the squared difference between array values and the
    /// mean value.
    /// </summary>
    public static double SSEvsMean(double[] input)
    {
        if (input.Length == 0) return 0.0;
        double m = input.Average();
        return input.Select(x => Math.Pow(x - m, 2)).Sum();
    }

    /// <summary>
    /// Calculate the population standard deviation from an input array.
    /// </summary>
    public static double StdevP(double[] input)
    {
        return Math.Sqrt(Variance(input));
    }

    /// <summary>
    /// Calculate the variance of a double array (average of the squared
    /// deviations from the mean).
    /// </summary>
    public static double Variance(double[] input)
    {
        double mn = input.Average();
        return input.Select(x => Math.Pow(x - mn, 2)).Average();
    }
}

/// <summary>
/// A class that yields a boolean filter containing train vs test splits for
/// k-fold  cross validation. E.g. a 10 fold iterator will iteratively yield
/// 10 train / 90 test, 10 test / 10 train / 80 test etc in folds.
/// </summary>
public class KFoldIterator : IEnumerable<bool[]>
{
    /// <summary>The number of records to split into folds.</summary>
    private int size;

    /// <summary>The number of folds to iterate over.</summary>
    private int kfolds;

    /// <summary>The start and end index values of each fold.</summary>
    private List<(int, int)> foldIndeces;

    /// <summary>Get the current fold number.</summary>
    public int CurrentFold { get { return _currentFold; } }

    private int _currentFold;

    /// <summary>Create a new k-fold iterator.</summary>
    public KFoldIterator(int size, int k)
    {
        this.size = size;
        this.kfolds = k;
        int foldSize = (int)((double)size / (double)kfolds);
        foldIndeces = new List<(int, int)>();
        int foldStart, foldEnd;
        for (int cntr = 0; cntr < kfolds; cntr++)
        {
            foldStart = cntr * foldSize;
            foldEnd = foldStart + foldSize;
            foldIndeces.Add((foldStart, foldEnd));
        }
        _currentFold = 0;
    }

    /// <summary>Get a string representation of an instance.</summary>
    public override string ToString() =>
            $"KFoldIterator(k:{kfolds}, CurrentFold:{CurrentFold})";

    /// <summary>Get the IEnumerator to iterate over.</summary>
    public IEnumerator<bool[]> GetEnumerator()
    {
        for (int currentFold = 0; currentFold < kfolds; currentFold++)
        {
            _currentFold = currentFold + 1;
            (int, int) currentIndex = foldIndeces[currentFold];
            bool[] result = new bool[size];
            for (int idx = 0; idx < size; idx++)
            {
                if (idx >= currentIndex.Item1 & idx < currentIndex.Item2)
                    result[idx] = false;
                else result[idx] = true;
            }
            yield return result;
        }
    }

    System.Collections.IEnumerator
           System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
