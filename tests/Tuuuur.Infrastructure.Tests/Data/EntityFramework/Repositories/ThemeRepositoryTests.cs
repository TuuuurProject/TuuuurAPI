using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Factory.Tests;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Data.EntityFramework.Repositories;
using Tuuuur.Infrastructure.Tests.Fixtures;

namespace Tuuuur.Infrastructure.Tests.Data.EntityFramework.Repositories
{
    public class ThemeRepositoryTests : ADatabaseTests
    {
        private readonly Mock<ILogger<ThemeRepository>> m_MockLogger;

        public ThemeRepositoryTests(LocalDbFixture p_SqlServerFixture) : base(p_SqlServerFixture)
        {
            m_MockLogger = m_MockRepository.Create<ILogger<ThemeRepository>>();
        }

        private ThemeRepository CreateRepository()
        {
            return new ThemeRepository(m_SqlServerFixture.TestContext, m_Mapper, m_MockLogger.Object);
        }

        [Fact]
        public void GetAllAsync_ExpectedBehavior()
        {
            try
            {
                // Arrange
                ThemeRepository v_ThemeRepository = CreateRepository();
                List<ThemeThm> v_Entity = EfFactory.CreateTheme().Generate(5);

                Check.ThatCode(async () =>
                {
                    await m_SqlServerFixture.TestContext.ThemeThm.AddRangeAsync(v_Entity);
                    _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();
                }).DoesNotThrow();

                // Assert
                Check.ThatCode(() => v_ThemeRepository.GetAllThemesAsync(CancellationToken.None))
                    .WhichResult().Considering().Properties.Excluding(nameof(List<ThemeThm>.Capacity)).IsEqualTo(v_Entity);
            }
            finally
            {
                ClearData(m_SqlServerFixture.TestContext);
            }
        }
    }
}