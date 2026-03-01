using Tuuuur.Infrastructure.Services;

namespace Tuuuur.Infrastructure.Tests.Services;

public class EloServiceTests
{
    private readonly EloConfiguration m_DefaultConfig = new();

    // ── GetKFactor ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0, 40)]
    [InlineData(800, 40)]
    [InlineData(1200, 40)]
    public void GetKFactor_WhenEloUpTo1200_ShouldReturn40(int p_Elo, int p_ExpectedK)
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig);

        // Act
        int v_K = v_Service.GetKFactor(p_Elo);

        // Assert
        v_K.Should().Be(p_ExpectedK);
    }

    [Theory]
    [InlineData(1201, 20)]
    [InlineData(1600, 20)]
    [InlineData(2000, 20)]
    public void GetKFactor_WhenEloBetween1201And2000_ShouldReturn20(int p_Elo, int p_ExpectedK)
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig);

        // Act
        int v_K = v_Service.GetKFactor(p_Elo);

        // Assert
        v_K.Should().Be(p_ExpectedK);
    }

    [Theory]
    [InlineData(2001, 10)]
    [InlineData(3000, 10)]
    [InlineData(int.MaxValue, 10)]
    public void GetKFactor_WhenEloAbove2000_ShouldReturn10(int p_Elo, int p_ExpectedK)
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig);

        // Act
        int v_K = v_Service.GetKFactor(p_Elo);

        // Assert
        v_K.Should().Be(p_ExpectedK);
    }

    [Fact]
    public void GetKFactor_WithCustomSingleThreshold_ShouldUseItAsCatchAll()
    {
        // Arrange
        EloConfiguration v_Config = new()
        {
            KFactorThresholds =
            [
                new KFactorThreshold { MaxElo = int.MaxValue, K = 25 }
            ]
        };
        EloService v_Service = new(v_Config);

        // Act
        int v_K = v_Service.GetKFactor(5000);

        // Assert
        v_K.Should().Be(25);
    }

    // ── ComputeEloDelta ───────────────────────────────────────────────────────

    [Fact]
    public void ComputeEloDelta_WhenEqualElo_BothDeltasShouldBePositiveAndEqual()
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig);
        int v_Elo = 1000;

        // Act
        (int v_WinnerDelta, int v_LoserDelta) = v_Service.ComputeEloDelta(v_Elo, v_Elo);

        // Assert
        v_WinnerDelta.Should().BeGreaterThan(0);
        v_LoserDelta.Should().BeGreaterThan(0);
        v_WinnerDelta.Should().Be(v_LoserDelta);
    }

    [Fact]
    public void ComputeEloDelta_WhenWinnerHasLowerElo_WinnerDeltaShouldBeGreater()
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig);

        // act: underdog wins (elo 800 beats elo 1300 — different K-factors: winner K=40, loser K=20)
        (int v_WinnerDelta, int v_LoserDelta) = v_Service.ComputeEloDelta(800, 1300);

        // Assert: underdog gains more; favourite loses less
        v_WinnerDelta.Should().BeGreaterThan(v_LoserDelta);
    }

    [Fact]
    public void ComputeEloDelta_WhenWinnerHasHigherElo_LoserDeltaShouldBeGreater()
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig);

        // act: favourite wins (elo 1200 vs elo 800)
        (int v_WinnerDelta, int v_LoserDelta) = v_Service.ComputeEloDelta(1200, 800);

        // Assert: favourite gains less; underdog loses more
        v_LoserDelta.Should().BeGreaterThanOrEqualTo(v_WinnerDelta);
    }

    [Fact]
    public void ComputeEloDelta_WinnerDelta_ShouldBeNonNegative()
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig);

        (int v_WinnerDelta, int v_LoserDelta) = v_Service.ComputeEloDelta(2500, 100);

        v_WinnerDelta.Should().BeGreaterThanOrEqualTo(0);
        v_LoserDelta.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(1000, 1000)]
    [InlineData(500, 1500)]
    [InlineData(1500, 500)]
    public void ComputeEloDelta_BothDeltas_ShouldAlwaysBePositive(int p_WinnerElo, int p_LoserElo)
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig);

        // Act
        (int v_WinnerDelta, int v_LoserDelta) = v_Service.ComputeEloDelta(p_WinnerElo, p_LoserElo);

        // Assert
        v_WinnerDelta.Should().BeGreaterThanOrEqualTo(0);
        v_LoserDelta.Should().BeGreaterThanOrEqualTo(0);
    }
}
