using Tuuuur.Infrastructure.Services;

namespace Tuuuur.Infrastructure.Tests.Services;

public class EloConfigurationTests
{
    [Fact]
    public void GetSectionName_ShouldReturnCorrectSectionName()
    {
        // Arrange
        EloConfiguration v_Config = new();

        // Act
        string v_SectionName = v_Config.GetSectionName();

        // Assert
        v_SectionName.Should().Be("EloConfiguration");
    }

    [Fact]
    public void DefaultKFactorThresholds_ShouldHaveThreeEntries()
    {
        // Arrange
        EloConfiguration v_Config = new();

        // Assert
        v_Config.KFactorThresholds.Should().HaveCount(3);
    }

    [Fact]
    public void DefaultKFactorThresholds_FirstEntry_ShouldBeK40UpTo1500()
    {
        // Arrange
        EloConfiguration v_Config = new();

        // Act
        KFactorThreshold v_First = v_Config.KFactorThresholds[0];

        // Assert
        v_First.MaxElo.Should().Be(1500);
        v_First.K.Should().Be(40);
    }

    [Fact]
    public void DefaultKFactorThresholds_SecondEntry_ShouldBeK25UpTo2500()
    {
        // Arrange
        EloConfiguration v_Config = new();

        // Act
        KFactorThreshold v_Second = v_Config.KFactorThresholds[1];

        // Assert
        v_Second.MaxElo.Should().Be(2500);
        v_Second.K.Should().Be(25);
    }

    [Fact]
    public void DefaultKFactorThresholds_ThirdEntry_ShouldBeK18AtMaxValue()
    {
        // Arrange
        EloConfiguration v_Config = new();

        // Act
        KFactorThreshold v_Third = v_Config.KFactorThresholds[2];

        // Assert
        v_Third.MaxElo.Should().Be(int.MaxValue);
        v_Third.K.Should().Be(18);
    }

    [Fact]
    public void KFactorThreshold_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        KFactorThreshold v_Threshold = new()
        {
            MaxElo = 1500,
            K = 30
        };

        // Assert
        v_Threshold.MaxElo.Should().Be(1500);
        v_Threshold.K.Should().Be(30);
    }
}
