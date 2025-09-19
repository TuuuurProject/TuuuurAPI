using Tuuuur.Domain.Interfaces.Data.Repositories;

namespace Tuuuur.Domain.Interfaces.Data;

public interface IUnitOfWork : IDisposable
{
    IUserRepository UserRepository { get; }
    IUserAuthRepository UserAuthRepository { get; }
    IThemeRepository ThemeRepository { get; }
    IDifficultyRepository DifficultyRepository { get; }

    int Save();

    void BeginTransaction();

    void CommitTransaction();

    void RollbackTransaction();
    T ExecutionStrategy<T>(Func<T> p_Func);
}