using AutoMapper;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Tests.Data.Mapping;
using Tuuuur.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Tuuuur.Infrastructure.Tests.Data
{
    /// <summary>
    /// Abstract class for tests around database
    /// </summary>
    [Collection(ISqlFixture.Collection)]
    public abstract class ADatabaseTests
    {
        protected readonly MockRepository m_MockRepository;
        protected readonly IMapper m_Mapper;
        protected IEnumerable<string> m_TypeManufacture;

        /// <summary>
        /// Use docker fixture for these tests
        /// </summary>
        protected readonly ISqlFixture m_SqlServerFixture;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="p_SqlServerFixture">docker fixture</param>
        protected ADatabaseTests(LocalDbFixture p_SqlServerFixture)
        {
            m_SqlServerFixture = p_SqlServerFixture;
            m_MockRepository = new MockRepository(MockBehavior.Strict);
            m_Mapper = InfrastructureProfileTests.InitializeAutoMapper().CreateMapper();
        }

        /// <summary>
        /// Factoring logger init
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_Logger"></param>
        protected static void InitLogger<T>(Mock<ILogger<T>> p_Logger)
        {
            p_Logger.Setup(p_X => p_X.Log<It.IsAnyType>(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        /// <summary>
        /// Delete All record in one table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_DbContext"></param>
        protected static void CleanTable<T>(DbContext p_DbContext)
        where T : class, IEntity
        {
            p_DbContext.Set<T>().RemoveRange(p_DbContext.Set<T>());
        }

        protected void CleanTable<T, TT>(DbContext p_DbContext, IEnumerable<TT> p_Ts)
        where T : class, IEntity
        where TT : class, IBOEntity
        {
            CleanTable(p_DbContext, m_Mapper.Map<IEnumerable<T>>(p_Ts));
        }

        protected static void CleanTable<T>(DbContext p_DbContext, IEnumerable<T> p_Ts)
        where T : class, IEntity
        {
            p_DbContext.Set<T>().RemoveRange(p_Ts);
            p_DbContext.SaveChanges();
            p_DbContext.ChangeTracker.Clear();
        }

        /// <summary>
        /// Add Entities in database
        /// </summary>
        /// <typeparam name="T">Class BO</typeparam>
        /// <typeparam name="TT">Class Entity</typeparam>
        /// <param name="p_Ts">DbSet</param>
        /// <param name="p_Entities">Data faker</param>
        /// <returns>Data list with  identity key</returns>
        protected async Task<IEnumerable<T>> GetDataAsync<T, TT>(DbContext p_DbContext, DbSet<TT> p_Ts, IEnumerable<T> p_Entities)
            where T : class, IBOEntity
            where TT : class, IEntity
        {
            IEnumerable<TT> v_TTs = m_Mapper.Map<IEnumerable<TT>>(p_Entities);
            await p_Ts.AddRangeAsync(v_TTs);
            p_DbContext.SaveChanges();
            p_DbContext.ChangeTracker.Clear();
            return m_Mapper.Map<IEnumerable<T>>(v_TTs);
        }

        protected static async Task<IEnumerable<T>> GetDataAsync<T>(DbContext p_DbContext, DbSet<T> p_Ts, IEnumerable<T> p_Entities)
            where T : class, IEntity
        {
            await p_Ts.AddRangeAsync(p_Entities);
            p_DbContext.SaveChanges();
            p_DbContext.ChangeTracker.Clear();
            return p_Entities;
        }

        protected static void ClearData(DbContext p_DbContext)
        {
            // Add here tables to clean
            CleanTable<RefreshTokenRtk>(p_DbContext);
            CleanTable<UserAuthUat>(p_DbContext);
            CleanTable<UserUsr>(p_DbContext);

            p_DbContext.SaveChanges();
            p_DbContext.ChangeTracker.Clear();
        }

        /// <summary>
        /// PickRandomOnce
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_Enumerator"></param>
        /// <returns></returns>
        public static T TakeNext<T>(IEnumerator<T> p_Enumerator)
        {
            if (!p_Enumerator.MoveNext())
                throw new IndexOutOfRangeException();
            return p_Enumerator.Current;
        }
    }
}