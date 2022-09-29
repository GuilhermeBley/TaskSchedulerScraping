namespace TaskSchedulerScraping.Scraper.DependencyInjection.Attributes;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class ScraperObjSharedAttribute : Attribute
{
    public ScraperObjSharedAttribute()
    {
        throw new ArgumentException();
    }
}