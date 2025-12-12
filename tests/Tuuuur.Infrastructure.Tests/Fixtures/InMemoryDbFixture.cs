using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac;
using Polly;

namespace Tuuuur.Infrastructure.Tests.Fixtures
{
    public class LocalDbFixture : ASqlServerFixture
    {
        public LocalDbFixture() : base()
        {
        }

        protected override void Dispose(bool p_Disposing)
        {
            TestContext?.Dispose();
        }
    }
}