namespace CsML.Probability;

public class Sample
{
    public static int[] WithoutReplacement(int[] input, int count)
    {
        int[] working = (int[])input.Clone();
        Random random = new Random();
        working = working.OrderBy(x => random.Next()).ToArray();
        return working[0..count];
    }

    public static int[] WithReplacement(int minValue, int maxValue, int count)
    {
        Random random = new Random();
        return Enumerable.Range(0, count)
                         .Select(_ => random.Next(minValue, maxValue))
                         .ToArray();
    }
}
public class Shuffle
{
    public static int[] Ints(int[] input, bool inPlace = false)
    {
        Random random = new Random();
        int[] result = inPlace ? input : (int[])input.Clone();
        return result.OrderBy(x => random.Next()).ToArray();
    }
}