using System.Data.Common;

namespace TaskSchedulerScraping.Infrastructure.Connection;

/// <summary>
/// <see cref="DbConnection"/> Factory
/// </summary>
public interface IConnectionFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="DbConnection"/>
    /// </summary>
    DbConnection CreateConn();
}
