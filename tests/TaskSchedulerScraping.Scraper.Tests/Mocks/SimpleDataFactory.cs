namespace TaskSchedulerScraping.Scraper.Tests.Mocks;

internal static class SimpleDataFactory
{
    public const int MaxData = 10;
    public static IEnumerable<SimpleData> GetData()
    {
        return GetData(MaxData);
    }

    public static IEnumerable<SimpleData> GetData(int length)
    {
        if (length < 0)
            throw new IndexOutOfRangeException($"{nameof(length)} should be more than '0'.");

        var list = new List<SimpleData>();
        for (int i = 0; i < length; i++)
        {
            list.Add(new SimpleData());
        }

        return list;
    }
}