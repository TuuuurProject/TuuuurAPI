using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Factory.Tests;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Data.EntityFramework.Repositories;
using Tuuuur.Infrastructure.Tests.Fixtures;

namespace Tuuuur.Infrastructure.Tests.Data.EntityFramework.Repositories
{
    public class PartyRepositoryTests : ADatabaseTests
    {
        private readonly Mock<ILogger<PartyRepository>> m_MockLogger;

        public PartyRepositoryTests(LocalDbFixture p_SqlServerFixture) : base(p_SqlServerFixture)
        {
            m_MockLogger = m_MockRepository.Create<ILogger<PartyRepository>>();
        }

        private PartyRepository CreateRepository()
        {
            return new PartyRepository(m_SqlServerFixture.TestContext, m_Mapper, m_MockLogger.Object);
        }

        [Fact]
        public void CreateParty_ExpectedBehavior()
        {
            try
            {
                // Arrange
                PartyRepository v_Repository = CreateRepository();
                Party v_Party = BoFactory.CreateParty().Generate();

                Check.ThatCode(async () =>
                {
                    await v_Repository.CreatePartyAsync(v_Party);
                    _ = await m_SqlServerFixture.TestContext.SaveChangesAsync();
                }).DoesNotThrow();

                // Assert
                Check.ThatCode(() =>
                        m_SqlServerFixture.TestContext.PartyPty.FirstOrDefault())
                    .WhichResult()
                    .Considering()
                    .Properties
                    .Excluding(
                        nameof(PartyPty.PartyUserPus),
                        nameof(PartyPty.PartyQuestionPqt),
                        nameof(PartyPty.IdUserHostNavigation),
                        nameof(PartyPty.IdPartyTypeNavigation),
                        nameof(PartyPty.PartyDifficultyPdf),
                        nameof(PartyPty.PartyThemePth),
                        nameof(Party.PartyUsers),
                        nameof(Party.PartyQuestions),
                        nameof(Party.PartyType),
                        nameof(Party.IdPartyType),
                        nameof(Party.Score),
                        nameof(Party.InProgress),
                        nameof(Party.NbQuestions),
                        nameof(Party.PartyTheme),
                        nameof(Party.PartyDifficulty),
                        nameof(Party.User),
                        nameof(Party.Percent),
                        nameof(Party.Time)
                        )
                    .IsEqualTo(v_Party);
            }
            finally
            {
                ClearData(m_SqlServerFixture.TestContext);
            }
        }
    }
}