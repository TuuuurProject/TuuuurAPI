using Tuuuur.Infrastructure.Services;

namespace Tuuuur.Infrastructure.Tests.Services;

public class EloServiceTests
{
    private readonly EloConfiguration m_DefaultConfig = new();

    // ── GetKFactor ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0, 40)]
    [InlineData(800, 40)]
    [InlineData(1500, 40)]
    public void GetKFactor_WhenEloUpTo1500_ShouldReturn40(int p_Elo, int p_ExpectedK)
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig);

        // Act
        int v_K = v_Service.GetKFactor(p_Elo);

        // Assert
        v_K.Should().Be(p_ExpectedK);
    }

    [Theory]
    [InlineData(1501, 25)]
    [InlineData(2000, 25)]
    [InlineData(2500, 25)]
    public void GetKFactor_WhenEloBetween1501And2500_ShouldReturn25(int p_Elo, int p_ExpectedK)
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig);

        // Act
        int v_K = v_Service.GetKFactor(p_Elo);

        // Assert
        v_K.Should().Be(p_ExpectedK);
    }

    [Theory]
    [InlineData(2501, 18)]
    [InlineData(3000, 18)]
    [InlineData(int.MaxValue, 18)]
    public void GetKFactor_WhenEloAbove2500_ShouldReturn18(int p_Elo, int p_ExpectedK)
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

        // act: underdog wins (elo 1400 beats elo 1600 — different K-factors: winner K=40, loser K=25)
        (int v_WinnerDelta, int v_LoserDelta) = v_Service.ComputeEloDelta(1400, 1600);

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

    // ── GetKFactor fallback path ───────────────────────────────────────────────

    [Fact]
    public void GetKFactor_WhenEloExceedsAllDefinedThresholds_ShouldReturnKOfHighestThreshold()
    {
        // Arrange: config with no catch-all (no int.MaxValue threshold).
        // Any elo above 1000 exhausts all thresholds and hits the fallback return.
        EloConfiguration v_Config = new()
        {
            KFactorThresholds =
            [
                new KFactorThreshold { MaxElo = 500, K = 40 },
                new KFactorThreshold { MaxElo = 1000, K = 20 }
            ]
        };
        EloService v_Service = new(v_Config);

        // Act: elo=2000 exceeds both configured MaxElo values (500 and 1000).
        // The foreach loop finds no matching threshold, so the fallback return
        // returns the K of the threshold with the highest MaxElo.
        int v_K = v_Service.GetKFactor(2000);

        // Assert: highest MaxElo threshold is 1000 → K=20
        v_K.Should().Be(20);
    }
}
