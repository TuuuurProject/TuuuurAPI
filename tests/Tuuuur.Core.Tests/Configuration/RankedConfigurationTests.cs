using Tuuuur.Core.Configuration;

namespace Tuuuur.Core.Tests.Configuration;

public class RankedConfigurationTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Act
        RankedConfiguration v_Config = new();

        // Assert
        v_Config.ThresholdRound.Should().Be(5);
        v_Config.MultiplierIncrement.Should().Be(0.5);
        v_Config.InitialRankedScore.Should().Be(5000);
        v_Config.DefaultElo.Should().Be(1000);
    }

    [Fact]
    public void GetSectionName_ShouldReturnRankedConfiguration()
    {
        // Arrange
        RankedConfiguration v_Config = new();

        // Act
        string v_SectionName = v_Config.GetSectionName();

        // Assert
        v_SectionName.Should().Be("RankedConfiguration");
    }

    [Theory]
    [InlineData(3, 0.25, 3000, 800)]
    [InlineData(10, 1.0, 10000, 1200)]
    public void Properties_CanBeSet(int p_ThresholdRound, double p_Multiplier, int p_InitialScore, int p_DefaultElo)
    {
        // Act
        RankedConfiguration v_Config = new()
        {
            ThresholdRound = p_ThresholdRound,
            MultiplierIncrement = p_Multiplier,
            InitialRankedScore = p_InitialScore,
            DefaultElo = p_DefaultElo
        };

        // Assert
        v_Config.ThresholdRound.Should().Be(p_ThresholdRound);
        v_Config.MultiplierIncrement.Should().Be(p_Multiplier);
        v_Config.InitialRankedScore.Should().Be(p_InitialScore);
        v_Config.DefaultElo.Should().Be(p_DefaultElo);
    }
}
