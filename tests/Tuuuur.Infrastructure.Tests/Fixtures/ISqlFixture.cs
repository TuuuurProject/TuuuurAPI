using Tuuuur.Infrastructure.Data.EntityFramework;
using Tuuuur.Infrastructure.Tests.Data;

namespace Tuuuur.Infrastructure.Tests.Fixtures
{
    public interface ISqlFixture : IDisposable
    {
        internal const string Collection = "SqlServer-Collection";
        public TestContext TestContext { get; }
    }
}