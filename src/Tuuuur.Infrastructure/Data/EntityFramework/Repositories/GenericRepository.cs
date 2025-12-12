using Ardalis.GuardClauses;
using AutoMapper;
using AutoMapper.Internal;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Infrastructure.Data.Mapping;
using Tuuuur.Infrastructure.Tools;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Reflection;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Repositories;

/// <summary>
/// Generic repository for each entity of the DbContext
/// </summary>
/// <typeparam name="T">Entity of type <see cref="IEntity"/></typeparam>
internal class GenericRepository<T> : AMappingClass, IGenericRepository<T> where T : class, IEntity
{
    private const string Entitycannotbenull = "Entity cannot be null";
    private const string Entitycannotbenullorempty = "Entity cannot be null or empty";
    private readonly DbContext m_DbContext;
    private readonly DbSet<T> m_DbSet;

    protected GenericRepository(DbContext p_DbContext, IMapper p_Mapper, ILogger p_Logger) : base(p_Mapper)
    {
        m_DbContext = p_DbContext ?? throw new ArgumentNullException(nameof(p_DbContext), "DbContext cannot be null");
        m_DbSet = m_DbContext.Set<T>();
        Logger = p_Logger;
    }

    protected ILogger Logger { get; set; }

    /// <summary>
    /// Get the current <see cref="DbContext"/>
    /// </summary>
    protected DbContext DbContext => m_DbContext;

    public Task AddAsync(T p_Entity, CancellationToken p_CancellationToken = default)
    {
        if (p_Entity == null)
            throw new ArgumentNullException(nameof(p_Entity), Entitycannotbenull);

        return AddInternalAsync(p_Entity, p_CancellationToken);
    }

    public Task AddAsync(IEnumerable<T> p_Entities, CancellationToken p_CancellationToken = default)
    {
        if (p_Entities?.Any() != true)
            throw new ArgumentNullException(nameof(p_Entities), Entitycannotbenullorempty);

        return AddInternalAsync(p_Entities, p_CancellationToken);
    }

    public async Task<long> CountAsync(Expression<Func<T, bool>> p_Predicate = null, CancellationToken p_CancellationToken = default)
    {
        return p_Predicate == null ? await m_DbSet.LongCountAsync(cancellationToken: p_CancellationToken) : await m_DbSet.LongCountAsync(p_Predicate, cancellationToken: p_CancellationToken);
    }

    public Task DeleteAsync(object p_Id, CancellationToken p_CancellationToken = default)
    {
        if (p_Id == null)
            throw new ArgumentNullException(nameof(p_Id), "Id cannot be null");

        return DeleteInternalAsync(p_Id, p_CancellationToken);
    }

    public Task DeleteAsync(T p_Entity)
    {
        if (p_Entity == null)
            throw new ArgumentNullException(nameof(p_Entity), Entitycannotbenull);

        return DeleteInternal(p_Entity);
    }

    public Task DeleteAsync(IEnumerable<T> p_Entities)
    {
        if (p_Entities == null)
            throw new ArgumentNullException(nameof(p_Entities), Entitycannotbenull);
        return DeleteInternal(p_Entities);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool p_Disposing)
    {
        // Cleanup
    }

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> p_Predicate, CancellationToken p_CancellationToken = default)
    {
        if (p_Predicate == null)
            throw new ArgumentNullException(nameof(p_Predicate), "Predicate cannot be null or empty");

        return ExistsInternalAsync(p_Predicate, p_CancellationToken);
    }

    public IQueryable<T> FindBy(Expression<Func<T, bool>> p_Predicate, Func<IQueryable<T>, IOrderedQueryable<T>> p_OrderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> p_Include = null, int? p_Skip = null, int? p_Take = null, bool p_DisableTracking = true)
    {
        IQueryable<T> v_Query = m_DbSet;
        if (p_DisableTracking)
            v_Query = v_Query.AsNoTracking();

        if (p_Include != null)
            v_Query = p_Include(v_Query);

        if (p_Predicate != null)
            v_Query = v_Query.Where(p_Predicate);

        if (p_OrderBy != null)
            v_Query = p_OrderBy(v_Query);

        if (p_Skip.HasValue)
            v_Query = v_Query.Skip(p_Skip.Value);

        if (p_Take.HasValue)
            v_Query = v_Query.Take(p_Take.Value);

        return v_Query;
    }

    public async Task<IEnumerable<T>> GetAllAsync(Func<IQueryable<T>, IIncludableQueryable<T, object>> p_Include = null, Func<IQueryable<T>, IOrderedQueryable<T>> p_OrderBy = null, int? p_Skip = null, int? p_Take = null, bool p_DisableTracking = true, CancellationToken p_CancellationToken = default)
    {
        return await GetAllInternal(p_Include, p_OrderBy, p_Skip, p_Take, p_DisableTracking).ToListAsync(cancellationToken: p_CancellationToken);
    }

    public Task<T> GetAsync(object p_Id, CancellationToken p_CancellationToken = default)
    {
        if (p_Id == null)
            throw new ArgumentNullException(nameof(p_Id), "Id cannot be null");

        return GetInternalAsync(p_Id, p_CancellationToken);
    }

    public Task UpdateAsync(T p_Entity)
    {
        if (p_Entity == null)
            throw new ArgumentNullException(nameof(p_Entity), Entitycannotbenull);

        return UpdateInternal(p_Entity);
    }

    public Task UpdateAsync(IEnumerable<T> p_Entities)
    {
        if (p_Entities?.Any() != true)
            throw new ArgumentNullException(nameof(p_Entities), Entitycannotbenullorempty);

        return UpdateInternal(p_Entities);
    }

    /// <summary>
    /// Map BOEntity to EF Core Entity (prevent tracking exception)
    /// </summary>
    /// <typeparam name="TT">BOEntity type</typeparam>
    /// <param name="p_BoEntity">instance of BOEntity</param>
    /// <returns>instance of EF Core Entity</returns>
    internal T MapEntity<TT>(TT p_BoEntity) where TT : class
    {
        Guard.Against.Null(p_BoEntity);

        IEnumerable<string> v_KeyName = m_DbContext.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties.Select(p_X => p_X.Name); //get primary keys
        TypeMap v_TypeMap = Mapper.ConfigurationProvider.Internal().FindTypeMapFor<T, TT>(); //get mapping for primary keys
        Dictionary<string, string> v_PropertiesMap = v_TypeMap?.PropertyMaps?.Where(p_Pm => v_KeyName.Contains(p_Pm.SourceMember.Name)).ToDictionary(p_C => p_C.SourceMember.Name, p_C => p_C.DestinationMember.Name);

        if (v_PropertiesMap != null)
        {
            //build query with primary keys
            Expression<Func<T, bool>> v_Expression = PredicateBuilder.New<T>(true);
            foreach (KeyValuePair<string, string> v_Item in v_PropertiesMap)
            {
                PropertyInfo v_TargetPropertyInfo = typeof(TT).GetProperty(v_Item.Value);
                object v_TargetValue = v_TargetPropertyInfo.GetValue(p_BoEntity);

                if (v_TargetValue.Equals(v_TargetPropertyInfo.PropertyType.GetDefaultValue())) //if it is en unsaved object
                    return Mapper.Map<T>(p_BoEntity);

                v_Expression = v_Expression.And(p_C => typeof(T).GetProperty(v_Item.Key).GetValue(p_C).Equals(v_TargetValue));
            }

            T v_LocalEntity = m_DbSet.Local.SingleOrDefault(v_Expression.Compile());
            if (v_LocalEntity != null) //entity in local already tracked
            {
                m_DbContext.Entry(v_LocalEntity).State = EntityState.Detached;
                return Mapper.Map(p_BoEntity, v_LocalEntity);
            }
        }

        //if no tracking we used new instance
        return Mapper.Map<T>(p_BoEntity);
    }

    private async Task AddInternalAsync(T p_Entity, CancellationToken p_CancellationToken)
    {
        await m_DbSet.AddAsync(p_Entity, p_CancellationToken);
    }

    private async Task AddInternalAsync(IEnumerable<T> p_Entities, CancellationToken p_CancellationToken)
    {
        await m_DbSet.AddRangeAsync(p_Entities, p_CancellationToken);
    }

    private async Task DeleteInternalAsync(object p_Id, CancellationToken p_CancellationToken)
    {
        T v_Entity = await m_DbSet.FindAsync(new[] { p_Id }, cancellationToken: p_CancellationToken);
        await DeleteAsync(v_Entity);
    }

    private Task DeleteInternal(T p_Entity)
    {
        if (m_DbContext.Entry(p_Entity).State == EntityState.Detached)
        {
            m_DbSet.Attach(p_Entity);
        }
        m_DbSet.Remove(p_Entity);
        return Task.CompletedTask;
    }

    private Task DeleteInternal(IEnumerable<T> p_Entities)
    {
        m_DbSet.RemoveRange(p_Entities);
        return Task.CompletedTask;
    }

    private async Task<bool> ExistsInternalAsync(Expression<Func<T, bool>> p_Predicate, CancellationToken p_CancellationToken)
    {
        return await m_DbSet.AnyAsync(p_Predicate, cancellationToken: p_CancellationToken);
    }

    private IQueryable<T> GetAllInternal(Func<IQueryable<T>, IIncludableQueryable<T, object>> p_Include = null, Func<IQueryable<T>, IOrderedQueryable<T>> p_OrderBy = null, int? p_Skip = null, int? p_Take = null, bool p_DisableTracking = true)
    {
        return FindBy(null, p_OrderBy, p_Include, p_Skip, p_Take, p_DisableTracking);
    }

    private async Task<T> GetInternalAsync(object p_Id, CancellationToken p_CancellationToken)
    {
        T v_Entity = await m_DbSet.FindAsync(new[] { p_Id }, cancellationToken: p_CancellationToken);
        m_DbContext.Entry(v_Entity).State = EntityState.Detached;
        return v_Entity;
    }

    private Task UpdateInternal(T p_Entity)
    {
        if (((int)m_DbContext.Entry(p_Entity).State) < 2)
            m_DbContext.Entry(p_Entity).State = EntityState.Modified;
        m_DbSet.Update(p_Entity);
        return Task.CompletedTask;
    }

    private Task UpdateInternal(IEnumerable<T> p_Entities)
    {
        m_DbSet.UpdateRange(p_Entities);
        return Task.CompletedTask;
    }
}