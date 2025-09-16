using AutoMapper;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Repositories;
using Tuuuur.Infrastructure.Data.EntityFramework.Repositories;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Tuuuur.Infrastructure.Data;

internal class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext m_DbContext;
    private readonly IMapper m_Mapper;
    private readonly ILoggerFactory m_LoggerFactory;
    private readonly Lazy<IUserRepository> m_UserRepository;
    private readonly Lazy<IUserAuthRepository> m_UserAuthRepository;

    public UnitOfWork(TContext p_DbContext, IMapper p_Mapper, ILoggerFactory p_LoggerFactory)
    {
        //Infrastructure layer instance
        m_DbContext = p_DbContext ?? throw new ArgumentNullException(nameof(p_DbContext));
        m_Mapper = p_Mapper ?? throw new ArgumentNullException(nameof(p_Mapper));
        m_LoggerFactory = p_LoggerFactory ?? throw new ArgumentNullException(nameof(p_LoggerFactory));
        m_UserRepository = CreateLazy<IUserRepository, UserRepository>();
        m_UserAuthRepository = CreateLazy<IUserAuthRepository, UserAuthRepository>();
    }
    public T ExecutionStrategy<T>(Func<T> p_Func)
    {
        IExecutionStrategy v_Strategy = m_DbContext.Database.CreateExecutionStrategy();
        return v_Strategy.Execute(() => p_Func());
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public IUserRepository UserRepository => m_UserRepository.Value;
    public IUserAuthRepository UserAuthRepository => m_UserAuthRepository.Value;

    public int Save()
    {
        //Detaching unchanged entities
        m_DbContext?.ChangeTracker?.Entries().Where(p_E => p_E.State == EntityState.Unchanged).ForEach(p_E => p_E.State = EntityState.Detached);
        return m_DbContext?.SaveChanges() ?? 0;
    }

    public void BeginTransaction() => m_DbContext?.Database.BeginTransaction();

    public void CommitTransaction() => m_DbContext?.Database.CommitTransaction();

    public void RollbackTransaction() => m_DbContext?.Database.RollbackTransaction();

    protected virtual void Dispose(bool p_Disposing)
    {
        if (p_Disposing)
            m_DbContext?.Dispose();
    }

    private Lazy<TInterface> CreateLazy<TInterface, TImplem>()
        where TInterface : IGenericRepository
        where TImplem : class, IGenericRepository
    {
        return new Lazy<TInterface>(() => (TInterface)Activator.CreateInstance(typeof(TImplem), m_DbContext, m_Mapper, m_LoggerFactory.CreateLogger<TImplem>()));
    }
}