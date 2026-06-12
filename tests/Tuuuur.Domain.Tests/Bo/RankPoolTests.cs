using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Tests.Bo;

public class RankPoolTests
{
    // ── RankPoolConfiguration ───────────────────────────────────────────────

    [Fact]
    public void RankPoolConfiguration_DefaultValues_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        RankPoolConfiguration v_Pool = new();

        // Assert
        v_Pool.Tier.Should().Be(0);
        v_Pool.DifficultyIds.Should().BeEmpty();
        v_Pool.ThemeIds.Should().BeNull();
    }

    [Fact]
    public void RankPoolConfiguration_ShouldStoreProperties()
    {
        // Arrange & Act
        RankPoolConfiguration v_Pool = new()
        {
            Tier = 3,
            DifficultyIds = [1, 2, 3],
            ThemeIds = [10, 20]
        };

        // Assert
        v_Pool.Tier.Should().Be(3);
        v_Pool.DifficultyIds.Should().BeEquivalentTo([1, 2, 3]);
        v_Pool.ThemeIds.Should().BeEquivalentTo([10, 20]);
    }

    // ── RankThresholdConfiguration ──────────────────────────────────────────

    [Fact]
    public void RankThresholdConfiguration_DefaultValues_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        RankThresholdConfiguration v_Threshold = new();

        // Assert
        v_Threshold.MinElo.Should().Be(0);
        v_Threshold.Tier.Should().Be(0);
        v_Threshold.Division.Should().Be(0);
    }

    [Fact]
    public void RankThresholdConfiguration_ShouldStoreProperties()
    {
        // Arrange & Act
        RankThresholdConfiguration v_Threshold = new()
        {
            MinElo = 1200,
            Tier = 3,
            Division = 1
        };

        // Assert
        v_Threshold.MinElo.Should().Be(1200);
        v_Threshold.Tier.Should().Be(3);
        v_Threshold.Division.Should().Be(1);
    }
}
