using Tuuuur.Domain.Bo;
using Tuuuur.Infrastructure.Services;

namespace Tuuuur.Infrastructure.Tests.Services;

public class EloServiceTests
{
    private readonly EloConfiguration m_DefaultConfig = new();
    private readonly RankConfiguration m_DefaultRankConfig = new();

    // ── GetKFactor ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0, 40)]
    [InlineData(800, 40)]
    [InlineData(1200, 40)]
    public void GetKFactor_WhenEloUpTo1200_ShouldReturn40(int p_Elo, int p_ExpectedK)
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig, m_DefaultRankConfig);

        // Act
        int v_K = v_Service.GetKFactor(p_Elo);

        // Assert
        v_K.Should().Be(p_ExpectedK);
    }

    [Theory]
    [InlineData(1201, 40)]
    [InlineData(1500, 40)]
    public void GetKFactor_WhenEloBetween1201And1500_ShouldReturn40(int p_Elo, int p_ExpectedK)
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig, m_DefaultRankConfig);

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
        EloService v_Service = new(m_DefaultConfig, m_DefaultRankConfig);

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
        EloService v_Service = new(m_DefaultConfig, m_DefaultRankConfig);

        // Act
        int v_K = v_Service.GetKFactor(p_Elo);

        // Assert
        v_K.Should().Be(p_ExpectedK);
    }

    [Fact]
    public void GetKFactor_DuringPlacement_ShouldReturnPlacementKFactor()
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig, m_DefaultRankConfig);

        // Act: 19 games played < PlacementGames (20)
        int v_K = v_Service.GetKFactor(5000, 19);

        // Assert
        v_K.Should().Be(m_DefaultConfig.PlacementKFactor);
    }

    [Fact]
    public void GetKFactor_AfterPlacement_ShouldNotReturnPlacementKFactor()
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig, m_DefaultRankConfig);

        // Act: exactly PlacementGames (20) → no longer in placement
        int v_K = v_Service.GetKFactor(1000, 20);

        // Assert
        v_K.Should().NotBe(m_DefaultConfig.PlacementKFactor);
        v_K.Should().Be(40); // 1000 ≤ 1500 → K=40
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
        EloService v_Service = new(v_Config, m_DefaultRankConfig);

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
        EloService v_Service = new(m_DefaultConfig, m_DefaultRankConfig);
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
        EloService v_Service = new(m_DefaultConfig, m_DefaultRankConfig);

        // act: underdog wins (elo 800 beats elo 1800 — different K-factors: winner K=40, loser K=25)
        (int v_WinnerDelta, int v_LoserDelta) = v_Service.ComputeEloDelta(800, 1800);

        // Assert: underdog gains more; favourite loses less
        v_WinnerDelta.Should().BeGreaterThan(v_LoserDelta);
    }

    [Fact]
    public void ComputeEloDelta_WhenWinnerHasHigherElo_LoserDeltaShouldBeGreater()
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig, m_DefaultRankConfig);

        // act: favourite wins (elo 1200 vs elo 800)
        (int v_WinnerDelta, int v_LoserDelta) = v_Service.ComputeEloDelta(1200, 800);

        // Assert: favourite gains less; underdog loses more
        v_LoserDelta.Should().BeGreaterThanOrEqualTo(v_WinnerDelta);
    }

    [Fact]
    public void ComputeEloDelta_WinnerDelta_ShouldBeNonNegative()
    {
        // Arrange
        EloService v_Service = new(m_DefaultConfig, m_DefaultRankConfig);

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
        EloService v_Service = new(m_DefaultConfig, m_DefaultRankConfig);

        // Act
        (int v_WinnerDelta, int v_LoserDelta) = v_Service.ComputeEloDelta(p_WinnerElo, p_LoserElo);

        // Assert
        v_WinnerDelta.Should().BeGreaterThanOrEqualTo(0);
        v_LoserDelta.Should().BeGreaterThanOrEqualTo(0);
    }

    // ── GetHighestPool ─────────────────────────────────────────────────────────

    [Fact]
    public void GetHighestPool_WhenBothPlayersAreFer_ShouldReturnFerPool()
    {
        // Arrange: both at 0 elo → Fer 3 → pool = General only, Facile only
        EloService v_Service = new(m_DefaultConfig, m_DefaultRankConfig);

        // Act
        RankedQuestionPool v_Pool = v_Service.GetHighestPool(0, 0);

        // Assert: Fer pool → theme restricted to [1], difficulty [1]
        v_Pool.ThemeIds.Should().BeEquivalentTo(new[] { 1 });
        v_Pool.DifficultyIds.Should().BeEquivalentTo(new[] { 1 });
    }

    [Fact]
    public void GetHighestPool_ShouldUseHigherPlayerRank()
    {
        // Arrange: P1 at Fer (0), P2 at Argent (1200+) → higher = Argent
        EloService v_Service = new(m_DefaultConfig, m_DefaultRankConfig);

        // Act
        RankedQuestionPool v_Pool = v_Service.GetHighestPool(0, 1200);

        // Assert: Argent pool → all themes (null), Facile+Moyen [1,2]
        v_Pool.ThemeIds.Should().BeNull();
        v_Pool.DifficultyIds.Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [Fact]
    public void GetHighestPool_WhenBothPlayersAreChampion_ShouldReturnChampionPool()
    {
        // Arrange: both at Champion tier (3600+)
        EloService v_Service = new(m_DefaultConfig, m_DefaultRankConfig);

        // Act
        RankedQuestionPool v_Pool = v_Service.GetHighestPool(3600, 3600);

        // Assert: Champion pool → all themes (null), Difficile+Extrême [3,4]
        v_Pool.ThemeIds.Should().BeNull();
        v_Pool.DifficultyIds.Should().BeEquivalentTo(new[] { 3, 4 });
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
        EloService v_Service = new(v_Config, m_DefaultRankConfig);

        // Act: elo=2000 exceeds both configured MaxElo values (500 and 1000).
        // The foreach loop finds no matching threshold, so the fallback return
        // returns the K of the threshold with the highest MaxElo.
        int v_K = v_Service.GetKFactor(2000);

        // Assert: highest MaxElo threshold is 1000 → K=20
        v_K.Should().Be(20);
    }
}
