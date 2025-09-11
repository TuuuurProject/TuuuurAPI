using Tuuuur.Domain.Interfaces.Data.Repositories;

namespace Tuuuur.Domain.Interfaces.Data;

public interface IUnitOfWork : IDisposable
{
    //IRepository Repository { get; }

    int Save();

    void BeginTransaction();

    void CommitTransaction();

    void RollbackTransaction();
}