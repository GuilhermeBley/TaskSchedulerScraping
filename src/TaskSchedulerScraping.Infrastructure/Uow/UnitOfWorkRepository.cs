using System.Data;
using System.Data.Common;
using TaskSchedulerScraping.Application.UoW;
using TaskSchedulerScraping.Infrastructure.Connection;

namespace TaskSchedulerScraping.Infrastructure.UoW;

internal class UnitOfWorkRepository : IUnitOfWork, IUnitOfWorkRepository
{
    private DbConnection? _connection { get; set; }
    private DbTransaction? _transaction { get; set; }

    public IDbConnection Connection => _connection ?? throw new DataException("Connection is closed.");

    public IDbTransaction Transaction => _transaction!;

    public Guid Identifier { get; } = Guid.NewGuid();

    private readonly IConnectionFactory _connectionFactory;

    public UnitOfWorkRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IUnitOfWork> BeginTransactionAsync()
    {
        await OpenConnectionAsync();

        if (_transaction is null && _connection is not null)
            _transaction = await _connection.BeginTransactionAsync();
            
        return this;
    }

    public async Task SaveChangesAsync()
    {
        try
        {
            if (_transaction is not null)
                await _transaction.CommitAsync();
        }
        catch
        {
            if (_transaction is not null)
                await _transaction.RollbackAsync();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null!;
        }
    }

    public void Dispose()
    {
        if (_transaction is not null)
        {
            _transaction.Dispose();
            _transaction = null;
        }

        if (_connection is not null)
        {
            _connection.Dispose();
            _connection = null;
        }
    }

    public async Task<IUnitOfWork> OpenConnectionAsync()
    {
        if (_connection is null)
        {
            _connection = _connectionFactory.CreateConn();
            await _connection.OpenAsync();
        }

        return this;
    }
}