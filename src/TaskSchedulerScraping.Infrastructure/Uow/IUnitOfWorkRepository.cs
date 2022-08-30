using System.Data;

namespace TaskSchedulerScraping.Infrastructure.UoW;

public interface IUnitOfWorkRepository
{
    IDbConnection Connection { get; }
    IDbTransaction Transaction { get; }
}