using Microsoft.Toolkit.HighPerformance;

namespace CsML.Util;

public class Array
{
    /// <summary>
    /// Determine which value to split an array on to maximise the weighted gain 
    /// in purity when the split is applied to a corresponding target array.
    /// </summary>
    /// <param name="vector">Numeric array to test split points on.</param>
    /// <param name="target">Target array to find best gain in purity on split.
    /// </param>
    /// <param name="purityfn">Function that calculates purity of an array.</param>
    /// <returns>
    /// A tuple containing the best split value and the gain calculated by the gain
    /// function specified through the purityfn parameter.
    /// </returns>
    public static (double, double) BestSplit<T>(
        double[] vector,
        T[] target,
        Func<T[], double> purityfn)
        where T : notnull
    {
        double bestsplit = 0.0, bestgain = 0.0;
        int lenVals = vector.Length;
        (double, T)[] zipped = vector.Zip(target).OrderBy(x => x.First).ToArray();
        List<T> lhs = new List<T>(), rhs = new List<T>(target);
        // target.CopyTo(rhs, 0);
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
                var midpoint = (zipped[idx].Item1 + zipped[idx + 1].Item1) / 2.0;
                bestgain = gain;
                bestsplit = midpoint;
            }
        }
        if (allSame)
            return (zipped[0].Item1 - 1.0, 0.0);
        return (bestsplit, bestgain);
    }

    /// <summary>
    /// Calculate classification accuracy from a predictions array compared to an
    /// actuals array.
    /// </summary>
    public static double ClassificationAccuracy<T>(
        T[] actuals, T[] predictions) where T : IComparable<T>
    {
        int lenActuals = actuals.Length, lenPredictions = predictions.Length;
        if (lenActuals != lenPredictions)
            throw new ArgumentException("Inputs must be same length");
        double tptn = 0;
        for (int idx = 0; idx < lenActuals; idx++)
        {
            if (actuals[idx].CompareTo(predictions[idx]) == 0) tptn += 1;
        }
        return tptn / lenActuals;
    }

    /// <summary>
    /// Calculate classification error from a predictions array compared to an
    /// actuals array.
    /// </summary>
    public static double ClassificationError<T>(
        T[] actuals, T[] predictions) where T : IComparable<T>
    {
        return 1.0 - ClassificationAccuracy(actuals, predictions);
    }

    /// <summary>
    /// Calculate Precision (proportion of positives predictived correctly)
    /// and Recall (proportion of true positives found).
    /// </summary>
    public static Dictionary<T, (double, double)> ClassificationMetrics<T>(
        T[] actuals, T[] predictions) where T : IComparable<T>
    {
        int lenActuals = actuals.Length, lenPredictions = predictions.Length;
        if (lenActuals != lenPredictions)
            throw new ArgumentException("Inputs must be same length");
        (T, T)[] zipped = actuals.Zip(predictions).ToArray();
        Dictionary<T, double[]> counts = new Dictionary<T, double[]> { };
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
        Dictionary<T, (double, double)> result = new Dictionary<T, (double, double)> { };
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
    /// Count of ocurrences of each array element.
    /// </summary>
    /// <returns>
    /// A dictionary containing array elements as keys and  element counts as values.
    /// </returns>
    public static Dictionary<T, int> ElementCounts<T>(T[] input) where T : notnull
    {
        Dictionary<T, int> counts = new Dictionary<T, int>();
        foreach (T item in input)
        {
            if (counts.ContainsKey(item)) counts[item] += 1;
            else counts[item] = 1;
        }
        return counts;
    }

    /// <summary>
    /// Split a double array using a boolean filter array, with equivalent true values 
    /// going to the left and false going to the right.
    /// </summary>
    public static (T[], T[]) Split<T>(
        T[] input,
        bool[] filter)
    {
        List<T> lhs = new List<T>(), rhs = new List<T>();
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

/// <summary>
/// A collection of functions that do work on model training inputs, comprising a matrix of
/// features and an a target variable array.
/// </summary>
public class Features
{
    /// <summary>
    /// Shuffle a matrix containing features and a target array maintaining the
    /// relationship between matrix rows and array items.
    /// </summary>
    public static (double[,], double[]) Shuffle(double[,] matrix, double[] target)
    {
        int inputLength = matrix.GetLength(0), inputWidth = matrix.GetLength(1);
        if (inputLength != target.Length)
            throw new ArgumentException("Inputs must be same length");
        int[] startingIndex = Enumerable.Range(0, inputLength).ToArray();
        Random random = new Random();
        int[] shuffledIndex = ((int[])startingIndex.Clone()).OrderBy(x => random.Next()).ToArray();
        var fromtoIndex = startingIndex.Zip(shuffledIndex);
        double[,] newmatrix = new double[inputLength, inputWidth];
        double[] newtarget = new double[inputLength];
        foreach (var fromto in fromtoIndex)
        {
            newtarget[fromto.First] = target[fromto.Second];
            for (int colidx = 0; colidx < inputWidth; colidx++)
            {
                newmatrix[fromto.First, colidx] = matrix[fromto.Second, colidx];
            }
        }
        return (newmatrix, newtarget);
    }

    /// <summary>
    /// Split a matrix containing features and a target variable array
    /// into train and test sets using a ratio between 0 and 1, e.g. 0.7
    /// places 70% into train and keeps 30% for test.
    /// </summary>
    public static ((double[,], double[]), (double[,], double[])) Split(
        double[,] matrix, double[] target, double ratio
    )
    {
        if (ratio <= 0 | ratio >= 1)
            throw new ArgumentException("Splitting ratio must be between 0 and 1");
        int inputLength = matrix.GetLength(0), inputWidth = matrix.GetLength(1);
        if (inputLength != target.Length)
            throw new ArgumentException("Inputs must be same length");
        int[] index = Enumerable.Range(0, inputLength).ToArray();
        double cutPoint = (inputLength - 1) * ratio;
        bool[] filter = index.Select(x => x <= cutPoint).ToArray();
        double[,] mlhs, mrhs;
        double[] tlhs, trhs;
        (mlhs, mrhs) = CsML.Util.Matrix.Split(matrix, filter);
        (tlhs, trhs) = CsML.Util.Array.Split(target, filter);
        return ((mlhs, tlhs), (mrhs, trhs));
    }
}

public class Matrix
{
    /// <summary>
    /// Determine which value to split a matrix (two dimensional c# array) on to
    /// maximise the weighted gain in purity when the split is applied to a
    /// corresponding target array.
    /// </summary>
    /// <param name="matrix">Numeric two dimensional array to test split points on.</param>
    /// <param name="target">Target array to find best gain in purity on split.</param>
    /// <param name="purityfn">Function that calculates purity of an array.</param>
    /// <param name="randomFeatures">
    /// If randomFeatures is greater than 0, select a random number of specified columns
    /// to include (used to add randomisation to a Random Forest).
    /// </param>
    /// <returns>
    /// A tuple containing the index of the column with best gain and a tuple
    /// containing the split value and gain calculated by the gain
    /// function specified through the purityfn parameter.
    /// </returns>
    public static (int, (double, double)) BestSplit<T>(
        double[,] matrix,
        T[] target,
        Func<T[], double> purityfn,
        int randomFeatures)
        where T : notnull
    {
        Span2D<double> matrixSpan = matrix;
        int[] columnIndeces;
        int columnCount = matrix.GetLength(1);
        columnIndeces = Enumerable.Range(0, columnCount).ToArray();
        if (randomFeatures > 0 & randomFeatures < columnCount)
        {
            Random random = new Random();
            columnIndeces = columnIndeces.OrderBy(x => random.Next()).ToArray();
            columnIndeces = columnIndeces[0..randomFeatures];
        }
        double bestsplit = 0.0, bestgain = 0.0;
        int bestColumnIndex = 0;
        double split, gain;
        foreach (int columnIndex in columnIndeces)
        {
            double[] columnToCheck = matrixSpan.GetColumn(columnIndex).ToArray();
            (split, gain) = Array.BestSplit(columnToCheck, target, purityfn);
            if (gain > bestgain)
            {
                bestgain = gain;
                bestsplit = split;
                bestColumnIndex = columnIndex;
            }
        }
        return (bestColumnIndex, (bestsplit, bestgain));
    }

    /// <summary>
    /// Compare two matrixes for equality.
    /// </summary>
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
    /// <param name="inputfile">A Path object point to the CSV file.</param>
    /// <param name="mapping">
    /// A dictionary used to convert string columns to numeric values of the format
    /// { {column id , { {string val, numeric val}, {string val, numeric val} ...}.
    /// </param>
    public static double[,] FromCSV(
        string inputfile,
        Dictionary<int, Dictionary<string, double>>? mapping,
        string separator = ",",
        int loadFromRow = 0)
    {
        var rawdata = File.ReadLines(inputfile).Select(x => x.Split(separator)).ToArray();
        int rowcount = rawdata.Length, columncount = rawdata[0].Length;
        double[,] result = new double[rowcount - loadFromRow, columncount];
        double cell;
        string rawval;
        for (int rowidx = loadFromRow; rowidx < rowcount; rowidx++)
        {
            for (int colidx = 0; colidx < columncount; colidx++)
            {
                rawval = rawdata[rowidx][colidx];
                if (mapping != null)
                {
                    if (mapping.ContainsKey(colidx))
                    {
                        cell = mapping[colidx][rawval];
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
    /// Create a two dimensional array from a List.
    /// </summary>
    public static T[,] FromList2D<T>(List<T[]> matrix)
    {
        int rows = matrix.Count, cols = matrix[0].Length;
        T[,] result = new T[rows, cols];
        for (int ri = 0; ri < rows; ri++)
        {
            for (int ci = 0; ci < cols; ci++)
                result[ri, ci] = matrix[ri][ci];
        }
        return result;
    }

    public static double[] GetRow(double[,] matrix, int index, bool useSpan = true)
    {
        if (useSpan)
        {
            Span2D<double> matrixSpan = matrix;
            return matrixSpan.GetRow(index).ToArray();
        }
        int width = matrix.GetLength(1);
        double[] result = new double[width];
        for (int cidx = 0; cidx < width; cidx++)
        {
            result[cidx] = matrix[index, cidx];
        }
        return result;
    }

    /// <summary>
    /// Split a matrix (c# two dimensional double array) using a column
    /// index and split point.
    /// </summary>
    /// <param name="matrix">Numeric two dimensional array to split.</param>
    /// <param name="columnIndex">Column to split the matrix on.</param>
    /// <param name="splitPoint">
    /// Value to split the column on (greater than value splits left;
    /// less than value splits right).
    /// </param>
    /// <returns>
    /// Returns a Tuple containing a Tuple with left and right splits
    /// and boolean filter array. The filter array can be applied to
    /// other arrays using Split1D.
    /// </returns>
    public static ((double[,], double[,]), bool[])
        Split(
            double[,] matrix,
            int columnIndex,
            double splitPoint
        )
    {
        List<double[]> lhs = new List<double[]>(), rhs = new List<double[]>();
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
        if (lhs.Count != 0) dlhs = FromList2D(lhs);
        else dlhs = new double[,] { };
        if (rhs.Count != 0) drhs = FromList2D(rhs);
        else drhs = new double[,] { };
        return ((dlhs, drhs), filter);
    }

    /// <summary>
    /// Split a matrix using a boolean filer matrix.
    /// </summary>
    public static (double[,], double[,]) Split(double[,] matrix, bool[] filter)
    {
        List<double[]> lhs = new List<double[]>(), rhs = new List<double[]>();
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
        if (lhs.Count != 0) dlhs = FromList2D(lhs);
        else dlhs = new double[,] { };
        if (rhs.Count != 0) drhs = FromList2D(rhs);
        else drhs = new double[,] { };
        return (dlhs, drhs);
    }
}

public class Statistics
{
    /// <summary>
    /// Calculate the Gini index of a set of discrete values.
    /// </summary>
    public static double Gini<T>(IEnumerable<T> vector) where T : notnull
    {
        int length = 0;
        // Get bin counts
        Dictionary<T, int> counts = new Dictionary<T, int>();
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
    /// Calculate the r-squared and adjusted r-squared of an actuals array vs predictions.
    /// <see> See
    /// <seealso href=" https://en.wikipedia.org/wiki/Coefficient_of_determination">Wikipedia</seealso>
    /// Coefficient Of Determination.
    /// </see>
    /// </summary>
    /// <param name="p">
    /// p is the number of explanatory terms used in the regression to calculated adjusted r-squared
    /// (returns 0 for adjusted r-squared if p is null).
    /// </param>
    /// <returns>A tuple containing r-squared and adjusted r-squared.</returns>
    public static (double, double) RSquared(double[] actuals, double[] predictions, int? p)
    {
        double mn = actuals.Average();
        double sseVal = SSE(actuals, predictions);
        double sstVal = actuals.Select(x => Math.Pow(x - mn, 2)).Sum();
        double rsq = 1.0 - sseVal / sstVal;
        if (p == null)
        {
            return (rsq, 0.0);
        }
        double n = (double)actuals.Length;
        return (rsq, 1.0 - (1.0 - rsq) * ((n - 1) / (n - (double)p! - 1)));
    }

    /// <summary>
    /// Calculate the sum of the squared difference between two arrays.
    /// </summary>
    public static double SSE(double[] actuals, double[] predictions)
    {
        IEnumerable<(double, double)> zipped = actuals.Zip(predictions);
        return zipped.Select(x => Math.Pow((x.Item1 - x.Item2), 2)).ToArray().Sum();
    }

    /// <summary>
    /// Calculate the population standard deviation from an input array.
    /// </summary>
    public static double StdevP(double[] input)
    {
        double mn = input.Average();
        double dev = input.Select(x => Math.Pow(x - mn, 2)).Sum();
        return Math.Sqrt(dev / input.Length);
    }
}

/// <summary>
/// A class that yields a boolean filter containing train vs test splits for k-fold
/// cross validation. E.g. a 10 fold iterator will iteratively yield 10 train / 90
/// test, 10 test / 10 train / 80 test etc in folds.
/// </summary>
public class KFoldIterator : IEnumerable<bool[]>
{
    public int size;
    public int kfolds;
    public List<(int, int)> foldIndeces;

    public KFoldIterator(int size, int kfolds)
    {
        this.size = size;
        this.kfolds = kfolds;
        int foldSize = (int)((double)size / (double)kfolds);
        foldIndeces = new List<(int, int)>();
        int foldStart, foldEnd;
        for (int cntr = 0; cntr < kfolds; cntr++)
        {
            foldStart = cntr * foldSize;
            foldEnd = foldStart + foldSize;
            foldIndeces.Add((foldStart, foldEnd));
        }
    }

    public IEnumerator<bool[]> GetEnumerator()
    {
        for (int currentFold = 0; currentFold < kfolds; currentFold++)
        {
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

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}