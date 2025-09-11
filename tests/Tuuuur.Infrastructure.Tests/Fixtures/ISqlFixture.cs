using Tuuuur.Infrastructure.Data.EntityFramework;
using Tuuuur.Infrastructure.Tests.Data;

namespace Tuuuur.Infrastructure.Tests.Fixtures
{
    public interface ISqlFixture : IDisposable
    {
        internal const string Collection = "SqlServer-Collection";
        public string ConnectionString { get; }
        public TestContext TestContext { get; }
        public TuuuurContext TuuuurContext { get; }
    }
}