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
    public class DifficultyRepositoryTests : ADatabaseTests
    {
        private readonly Mock<ILogger<DifficultyRepository>> m_MockLogger;

        public DifficultyRepositoryTests(LocalDbFixture p_SqlServerFixture) : base(p_SqlServerFixture)
        {
            m_MockLogger = m_MockRepository.Create<ILogger<DifficultyRepository>>();
        }

        private DifficultyRepository CreateRepository()
        {
            return new DifficultyRepository(m_SqlServerFixture.TestContext, m_Mapper, m_MockLogger.Object);
        }

        [Fact]
        public void GetAllAsync_ExpectedBehavior()
        {
            try
            {
                // Arrange
                DifficultyRepository v_DifficultyRepository = CreateRepository();
                List<DifficultyDft> v_Entity = EfFactory.CreateDifficulty().Generate(5);

                Check.ThatCode(async () =>
                {
                    await m_SqlServerFixture.TestContext.DifficultyDft.AddRangeAsync(v_Entity);
                    _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();
                }).DoesNotThrow();

                // Assert
                Check.ThatCode(() => v_DifficultyRepository.GetAllDifficultiesAsync(CancellationToken.None))
                    .WhichResult().Considering().Properties.Excluding(nameof(List<DifficultyDft>.Capacity)).IsEqualTo(v_Entity);
            }
            finally
            {
                ClearData(m_SqlServerFixture.TestContext);
            }
        }
    }
}