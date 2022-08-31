using System.Data;
using TaskSchedulerScraping.Infrastructure.UoW;

namespace TaskSchedulerScraping.Infrastructure.Repositories;

public abstract class RepositoryBase
{
    private readonly IUnitOfWorkRepository _unitOfWorkRepository;
    protected IDbConnection _connection => _unitOfWorkRepository.Connection;
    protected IDbTransaction _transaction => _unitOfWorkRepository.Transaction;

    public RepositoryBase(IUnitOfWorkRepository unitOfWorkRepository)
    {
        _unitOfWorkRepository = unitOfWorkRepository;
    }
}