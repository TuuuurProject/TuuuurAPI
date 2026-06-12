using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Infrastructure.Services;

namespace Tuuuur.Infrastructure.Tests.Services;

public class RankServiceTests
{
    private static RankConfiguration CreateConfig()
    {
        return new RankConfiguration
        {
            Thresholds =
            [
                new RankThresholdConfiguration { MinElo = 0,    Tier = 1, Division = 3 },
                new RankThresholdConfiguration { MinElo = 400,  Tier = 1, Division = 2 },
                new RankThresholdConfiguration { MinElo = 650,  Tier = 1, Division = 1 },
                new RankThresholdConfiguration { MinElo = 850,  Tier = 2, Division = 3 },
                new RankThresholdConfiguration { MinElo = 1050, Tier = 2, Division = 1 },
                new RankThresholdConfiguration { MinElo = 2700, Tier = 7, Division = 1 },
                new RankThresholdConfiguration { MinElo = 3200, Tier = 8, Division = 0 }
            ],
            Pools =
            [
                new RankPoolConfiguration { Tier = 1, DifficultyIds = [1] },
                new RankPoolConfiguration { Tier = 2, DifficultyIds = [1, 2] },
                new RankPoolConfiguration { Tier = 7, DifficultyIds = [3, 4] },
                new RankPoolConfiguration { Tier = 8, DifficultyIds = [4, 5] }
            ]
        };
    }

    // ── GetRankForElo ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(0,    1, 3)]
    [InlineData(399,  1, 3)]
    [InlineData(400,  1, 2)]
    [InlineData(650,  1, 1)]
    [InlineData(850,  2, 3)]
    [InlineData(1050, 2, 1)]
    [InlineData(3200, 8, 0)]
    [InlineData(5000, 8, 0)]
    public void GetRankForElo_ShouldReturnExpectedTierAndDivision(int p_Elo, int p_ExpectedTier, int p_ExpectedDivision)
    {
        // Arrange
        RankService v_Service = new(CreateConfig());

        // Act
        (int v_Tier, int v_Division) = v_Service.GetRankForElo(p_Elo);

        // Assert
        v_Tier.Should().Be(p_ExpectedTier);
        v_Division.Should().Be(p_ExpectedDivision);
    }

    // ── GetPoolForTier ──────────────────────────────────────────────────────

    [Fact]
    public void GetPoolForTier_WhenTierExists_ShouldReturnMatchingPool()
    {
        // Arrange
        RankService v_Service = new(CreateConfig());

        // Act
        RankPoolConfiguration v_Pool = v_Service.GetPoolForTier(2);

        // Assert
        v_Pool.Should().NotBeNull();
        v_Pool.Tier.Should().Be(2);
        v_Pool.DifficultyIds.Should().BeEquivalentTo([1, 2]);
    }

    [Fact]
    public void GetPoolForTier_WhenTierDoesNotExist_ShouldFallbackToLowestTier()
    {
        // Arrange
        RankService v_Service = new(CreateConfig());

        // Act — tier 5 does not exist in config
        RankPoolConfiguration v_Pool = v_Service.GetPoolForTier(5);

        // Assert — falls back to lowest tier pool (Tier 1)
        v_Pool.Should().NotBeNull();
        v_Pool.Tier.Should().Be(1);
    }

    // ── GetAverageTier ──────────────────────────────────────────────────────

    [Fact]
    public void GetAverageTier_WhenSameTier_ShouldReturnThatTier()
    {
        // Arrange — both elos in Tier 1 Div 3 (elo 0–399)
        RankService v_Service = new(CreateConfig());

        // Act
        int v_Avg = v_Service.GetAverageTier(100, 200);

        // Assert
        v_Avg.Should().Be(1);
    }

    [Fact]
    public void GetAverageTier_WhenDifferentTiers_ShouldReturnRoundedAverage()
    {
        // Arrange — elo 0 => Tier 1, elo 850 => Tier 2
        RankService v_Service = new(CreateConfig());

        // Act — average of (1+2)/2 = 1.5 → rounds to 2
        int v_Avg = v_Service.GetAverageTier(0, 850);

        // Assert
        v_Avg.Should().Be(2);
    }

    [Fact]
    public void GetAverageTier_ShouldClampToMaxTier()
    {
        // Arrange — both elos at Tier 8
        RankService v_Service = new(CreateConfig());

        // Act
        int v_Avg = v_Service.GetAverageTier(5000, 5000);

        // Assert — clamped to max pool tier (8)
        v_Avg.Should().Be(8);
    }

    [Fact]
    public void GetAverageTier_ShouldClampToMinOne()
    {
        // Arrange
        RankService v_Service = new(CreateConfig());

        // Act — both at lowest
        int v_Avg = v_Service.GetAverageTier(0, 0);

        // Assert
        v_Avg.Should().Be(1);
    }
}
