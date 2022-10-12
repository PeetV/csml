using Microsoft.Toolkit.HighPerformance;

namespace CsML.Util;

public class Array
{
    /// <summary>
    /// Determine which value to split an array on to maximise the weighted gain 
    /// in purity when the split is applied to a corresponding target array.
    /// </summary>
    /// <remarks>
    /// To do: test if a List is faster than an an array for append
    /// operations.
    /// </remarks>
    /// <param name="vector">Numeric array to test split points on.</param>
    /// <param name="target">Target array to find best gain in purity on split.
    /// </param>
    /// <param name="purityfn">Function that calculates purity of an array.</param>
    /// <returns>
    /// A tuple containing the best split value and the gain calculated by the gain
    /// function specified through the purityfn parameter.
    /// </returns>
    public static Tuple<double, double> BestSplit<T>(
        double[] vector,
        T[] target,
        Func<T[], double> purityfn)
        where T : notnull
    {
        double bestsplit = 0.0, bestgain = 0.0;
        int lenVals = vector.Length;
        (double, T)[] zipped = vector.Zip(target).OrderBy(x => x.First).ToArray();
        T[] lhs = { }, rhs = new T[target.Length];
        target.CopyTo(rhs, 0);
        double purityPreSplit = purityfn(target);
        bool allSame = true;
        // Iterate through the sorted arrays
        for (int idx = 0; idx < lenVals - 1; idx++)
        {
            T targetval = zipped[idx].Item2;
            int targetvalIdx = System.Array.IndexOf(rhs, targetval);
            // Add to the left
            lhs = lhs.Append(targetval).ToArray();
            // Remove from the right
            rhs = rhs.Where((val, i) => i != targetvalIdx).ToArray();
            // Ignore this split potential if not unique
            if (zipped[idx].Item1 == zipped[idx + 1].Item1) continue;
            allSame = false;
            // Ignore this split, if not enough data for averages
            if (lhs.Length == 0 || rhs.Length == 0) continue;
            // Calculate Gini index
            double slice1Purity = purityfn(lhs) * lhs.Length / lenVals;
            double slice2Purity = purityfn(rhs) * rhs.Length / lenVals;
            double gain = purityPreSplit - slice1Purity - slice2Purity;
            if (gain > bestgain)
            {
                var midpoint = (zipped[idx].Item1 + zipped[idx + 1].Item1) / 2.0;
                bestgain = gain;
                bestsplit = midpoint;
            }
        }
        if (allSame)
            return Tuple.Create(zipped[0].Item1 - 1.0, 0.0);
        return Tuple.Create(bestsplit, bestgain);
    }

    /// <summary>
    /// Split a double array using a boolean filter array.
    /// </summary>
    /// <remarks>
    /// To do: test if a List is faster than an an array for append
    /// operations.
    /// </remarks>
    public static Tuple<T[], T[]> Split<T>(
        T[] input,
        bool[] filter)
    {
        T[] lhs = new T[] { }, rhs = new T[] { };
        for (int index = 0; index < input.Length; index++)
        {
            if (filter[index])
                lhs = lhs.Append(input[index]).ToArray();
            else
                rhs = rhs.Append(input[index]).ToArray();
        }
        return Tuple.Create(lhs, rhs);
    }

    /// <summary>
    /// Convert an array to a dictionary containing the count of ocurrences of each
    /// array element.
    /// </summary>
    public static Dictionary<T, int> ToElementCounts<T>(T[] input) where T: notnull
    {
        Dictionary<T, int> counts = new Dictionary<T, int>();
        foreach(T item in input)
        {
            if (counts.ContainsKey(item)) counts[item] += 1;
            else counts[item] = 1;
        }
        return counts;
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
    /// A function to randomly sample integers from an array without replacement.
    /// </summary>
    /// <remarks>
    /// See <see href="https://en.wikipedia.org/wiki/Reservoir_sampling">Wikipedia: Resevoir Sampling.</see>
    /// </remarks>
    public static int[] SampleTake(int[] input, int count)
    {
        int index;
        int[] result = new int[count];
        for (index = 0; index < count; index++)
            result[index] = input[index];
        Random r = new Random();
        for (; index < input.Length; index++)
        {
            int j = r.Next(index + 1);
            if (j < count)
                result[j] = input[index];
        }
        return result;
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
    public static Tuple<int, Tuple<double, double>> BestSplit<T>(
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
            columnIndeces = Statistics.SampleTake(columnIndeces, randomFeatures);
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
        return Tuple.Create(bestColumnIndex, Tuple.Create(bestsplit, bestgain));
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
        Dictionary<int, Dictionary<string, double>> mapping,
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
                if (mapping.ContainsKey(colidx))
                    cell = mapping[colidx][rawval];
                else if (!Double.TryParse(rawval, out cell))
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
    public static Tuple<Tuple<double[,], double[,]>, bool[]>
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
        else dlhs = new double[,]{};
        if (rhs.Count != 0) drhs = FromList2D(rhs);
        else drhs = new double[,]{};
        return Tuple.Create(Tuple.Create(dlhs, drhs), filter);
    }
}

public class Features
{
    public static Tuple<double[,], double[]> Shuffle(double[,] matrix, double[] target)
    {
        int inputLength = matrix.GetLength(0), inputWidth = matrix.GetLength(1);
        int[] startingIndex = Enumerable.Range(0, inputLength).ToArray();
        int[] shuffledIndex = Util.Statistics.SampleTake(startingIndex, inputLength);
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
        return Tuple.Create(newmatrix, newtarget);
    }
}