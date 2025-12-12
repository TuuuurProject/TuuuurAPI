using Microsoft.Extensions.Logging;
using Tuuuur.Infrastructure.Services;

namespace Tuuuur.Infrastructure.Tests.Services
{
    public class CalculServiceTests
    {
        private readonly Mock<ILogger<CalculService>> m_LoggerMock;
        private readonly CalculConfiguration m_CalculConfiguration;
        private readonly CalculService m_CalculService;

        public CalculServiceTests()
        {
            m_LoggerMock = new Mock<ILogger<CalculService>>();
            m_CalculConfiguration = new CalculConfiguration
            {
                MaxDurationInSeconds = 30,
                MaxScore = 1000
            };

            m_CalculService = new CalculService(m_CalculConfiguration);
        }

        [Fact]
        public void CalculateScore_ShouldThrow_WhenAnsweredAtIsNull()
        {
            DateTime v_PresentedAt = DateTime.Now;

            Assert.Throws<NotImplementedException>(() =>
                m_CalculService.CalculateScore(v_PresentedAt, null));
        }

        [Fact]
        public void CalculateScore_ShouldReturnZero_WhenDurationExceedsMax()
        {
            DateTime v_PresentedAt = DateTime.Now;
            DateTime v_AnsweredAt = v_PresentedAt.AddSeconds(m_CalculConfiguration.MaxDurationInSeconds + 1);

            int v_Score = m_CalculService.CalculateScore(v_PresentedAt, v_AnsweredAt);

            Assert.Equal(0, v_Score);
        }

        [Fact]
        public void CalculateScore_ShouldReturnMaxScore_WhenDurationIsZeroOrNegative()
        {
            DateTime v_PresentedAt = DateTime.Now;
            DateTime v_AnsweredAt = v_PresentedAt;

            int v_CalculateScore = m_CalculService.CalculateScore(v_PresentedAt, v_AnsweredAt);

            Assert.Equal(m_CalculConfiguration.MaxScore, v_CalculateScore);
        }

        [Fact]
        public void CalculateScore_ShouldReturnProportionalScore_WhenDurationIsPositiveAndLessThanMax()
        {
            DateTime v_PresentedAt = DateTime.Now;
            DateTime v_AnsweredAt = v_PresentedAt.AddSeconds(10);

            int v_ExpectedScore = (int)Math.Round(
                m_CalculConfiguration.MaxScore * (1 - (10.0 / m_CalculConfiguration.MaxDurationInSeconds))
            );

            int v_CalculateScore = m_CalculService.CalculateScore(v_PresentedAt, v_AnsweredAt);

            Assert.Equal(v_ExpectedScore, v_CalculateScore);
        }
    }
}
