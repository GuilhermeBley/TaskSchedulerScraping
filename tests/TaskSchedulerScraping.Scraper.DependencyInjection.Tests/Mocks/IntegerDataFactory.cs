namespace TaskSchedulerScraping.Scraper.DependencyInjection.Tests.Mocks;

internal static class IntegerDataFactory
{
    /// <summary>
    /// Get ordered data, '0' to <see cref="lenght"/>, initializes in '1'
    /// <exception cref="IndexOutOfRangeException"></exception>
    public static IEnumerable<IntegerData> GetData(int length)
    {
        if (length < 1)
            throw new IndexOutOfRangeException($"{nameof(length)} should be more than '0'.");

        var list = new List<IntegerData>();
        for (int i = 1; i <= length; i++)
        {
            list.Add(new IntegerData(i));
        }

        return list;
    }
}