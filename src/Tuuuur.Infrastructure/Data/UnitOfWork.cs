using AutoMapper;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Repositories;
using Tuuuur.Infrastructure.Data.EntityFramework.Repositories;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Tuuuur.Infrastructure.Data;

internal class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext m_DbContext;
    private readonly IMapper m_Mapper;
    private readonly ILoggerFactory m_LoggerFactory;
    //private readonly Lazy<IRepository> m_Repository;

    public UnitOfWork(TContext p_DbContext, IMapper p_Mapper, ILoggerFactory p_LoggerFactory)
    {
        //Infrastructure layer instance
        m_DbContext = p_DbContext ?? throw new ArgumentNullException(nameof(p_DbContext));
        m_Mapper = p_Mapper ?? throw new ArgumentNullException(nameof(p_Mapper));
        m_LoggerFactory = p_LoggerFactory ?? throw new ArgumentNullException(nameof(p_LoggerFactory));

        //m_Repository = CreateLazy<IRepository, ProjectRepository>();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    //public IRepository Repository => m_Repository.Value;

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