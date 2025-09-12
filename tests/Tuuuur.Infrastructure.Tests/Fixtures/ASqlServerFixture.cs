using Microsoft.EntityFrameworkCore;
using Tuuuur.Infrastructure.Data.EntityFramework;
using Tuuuur.Infrastructure.Tests.Data;
using Microsoft.SqlServer.Dac;

namespace Tuuuur.Infrastructure.Tests.Fixtures
{
    public abstract class ASqlServerFixture : ISqlFixture
    {
        #region Constants

        protected const string Tudbname = "TUUUUR_TU";

        #endregion Constants

        protected ASqlServerFixture()
        {
            //TestContext = DbContextFactory<TestContext>.InitContext();
            
            DbContextOptions<TestContext> v_DbContextOptions = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: Tudbname)
                .Options;
            TestContext = new TestContext(v_DbContextOptions);
        }

        public TestContext TestContext { get; }

        /// <summary>
        /// Dispose the fixture
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool p_Disposing);
    }
}