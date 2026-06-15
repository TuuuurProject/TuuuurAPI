using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tuuuur.API.Workers;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Notifications;

namespace Tuuuur.API.Tests.Workers;

public class MatchmakingWorkerTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<ICacheService> m_CacheServiceMock;
    private readonly Mock<IRankedNotificationService> m_NotificationServiceMock;
    private readonly Mock<IServiceScopeFactory> m_ScopeFactoryMock;
    private readonly IConfiguration m_Configuration;
    private readonly Mock<ILogger<MatchmakingWorker>> m_LoggerMock;

    public MatchmakingWorkerTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();
        m_NotificationServiceMock = m_MockRepository.Create<IRankedNotificationService>();
        m_ScopeFactoryMock = m_MockRepository.Create<IServiceScopeFactory>();
        m_LoggerMock = m_MockRepository.Create<ILogger<MatchmakingWorker>>(MockBehavior.Loose);

        // Use in-memory configuration with fast intervals for tests
        m_Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["MatchmakingWorker:BaseEloTolerance"] = "100",
                ["MatchmakingWorker:EloExpansionStep"] = "25",
                ["MatchmakingWorker:EloExpansionIntervalSeconds"] = "5",
                ["MatchmakingWorker:MaxEloTolerance"] = "500",
                ["MatchmakingWorker:LockExpirySeconds"] = "5",
                ["MatchmakingWorker:TickIntervalMs"] = "50",
                ["MatchmakingWorker:StandbyIntervalMs"] = "50"
            }!)
            .Build();
    }

    private MatchmakingWorker CreateWorker() =>
        new(
            m_CacheServiceMock.Object,
            m_ScopeFactoryMock.Object,
            m_Configuration,
            m_LoggerMock.Object);

    // ── Lock not acquired: standby path ──────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenLockNotAcquired_ShouldWaitAndRetry()
    {
        // Arrange
        MatchmakingWorker v_Worker = CreateWorker();
        using CancellationTokenSource v_Cts = new(TimeSpan.FromMilliseconds(200));

        // Lock never acquired, and release on stop should be called
        m_CacheServiceMock
            .Setup(p_C => p_C.AcquireLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        m_CacheServiceMock
            .Setup(p_C => p_C.ReleaseLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await v_Worker.StartAsync(v_Cts.Token);
        await Task.Delay(150);
        await v_Worker.StopAsync(CancellationToken.None);

        // Assert: lock acquisition attempted at least once, release called on shutdown
        m_CacheServiceMock.Verify(
            p_C => p_C.AcquireLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    // ── Tick: empty queue ─────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenQueueEmpty_ShouldNotCreateParty()
    {
        // Arrange
        MatchmakingWorker v_Worker = CreateWorker();
        using CancellationTokenSource v_Cts = new(TimeSpan.FromMilliseconds(250));
        int v_LockCallCount = 0;

        m_CacheServiceMock
            .Setup(p_C => p_C.AcquireLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                // Acquire once, then fail so it enters standby
                v_LockCallCount++;
                return v_LockCallCount == 1;
            });

        // Empty queue
        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetGetAllWithScoresAsync<User>(RedisKeys.Ranked.MatchmakingList(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(User, int)>());

        m_CacheServiceMock
            .Setup(p_C => p_C.HashGetAllAsync<long>(RedisKeys.Ranked.MatchmakingJoinedAt(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, long>());

        m_CacheServiceMock
            .Setup(p_C => p_C.RefreshLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // forces exit from leader loop after first tick

        m_CacheServiceMock
            .Setup(p_C => p_C.ReleaseLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await v_Worker.StartAsync(v_Cts.Token);
        await Task.Delay(150);
        await v_Worker.StopAsync(CancellationToken.None);

        // Assert: never tried to create a party
        m_ScopeFactoryMock.Verify(p_F => p_F.CreateScope(), Times.Never);
    }

    // ── Tick: two compatible players ─────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenTwoCompatiblePlayersInQueue_ShouldCreateParty()
    {
        // Arrange
        MatchmakingWorker v_Worker = CreateWorker();
        using CancellationTokenSource v_Cts = new(TimeSpan.FromMilliseconds(500));
        int v_LockCallCount = 0;

        User v_P1 = new() { Id = Guid.NewGuid(), Elo = [new Domain.Bo.Elo { IdTheme = 1, Value = 1000 }] };
        User v_P2 = new() { Id = Guid.NewGuid(), Elo = [new Domain.Bo.Elo { IdTheme = 1, Value = 1050 }] };

        m_CacheServiceMock
            .Setup(p_C => p_C.AcquireLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                v_LockCallCount++;
                return v_LockCallCount == 1;
            });

        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetGetAllWithScoresAsync<User>(RedisKeys.Ranked.MatchmakingList(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(User, int)> { (v_P1, 1000), (v_P2, 1050) });

        m_CacheServiceMock
            .Setup(p_C => p_C.HashGetAllAsync<long>(RedisKeys.Ranked.MatchmakingJoinedAt(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, long>());

        // Remove both from queue
        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetRemoveAsync(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        m_CacheServiceMock
            .Setup(p_C => p_C.HashDeleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Scope for CreateRankedPartyRequest
        Mock<IServiceScope> v_ScopeMock = new();
        Mock<IServiceProvider> v_ServiceProviderMock = new();
        Mock<IMediator> v_MediatorMock = new();
        v_MediatorMock
            .Setup(p_M => p_M.Send(It.IsAny<CreateRankedPartyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmptyResponse());
        v_ServiceProviderMock
            .Setup(p_Sp => p_Sp.GetService(typeof(IMediator)))
            .Returns(v_MediatorMock.Object);
        v_ScopeMock.Setup(p_S => p_S.ServiceProvider).Returns(v_ServiceProviderMock.Object);
        m_ScopeFactoryMock
            .Setup(p_F => p_F.CreateScope())
            .Returns(v_ScopeMock.Object);

        m_CacheServiceMock
            .Setup(p_C => p_C.RefreshLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        m_CacheServiceMock
            .Setup(p_C => p_C.ReleaseLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await v_Worker.StartAsync(v_Cts.Token);
        await Task.Delay(300);
        await v_Worker.StopAsync(CancellationToken.None);

        // Assert: scope was created (party creation was attempted)
        m_ScopeFactoryMock.Verify(p_F => p_F.CreateScope(), Times.AtLeastOnce);
        v_MediatorMock.Verify(p_M => p_M.Send(It.IsAny<CreateRankedPartyRequest>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    // ── Tick: two players with elo diff exceeding tolerance ──────────────────

    [Fact]
    public async Task ExecuteAsync_WhenEloDiffExceedsMaxTolerance_ShouldNotMatch()
    {
        // Arrange
        MatchmakingWorker v_Worker = CreateWorker();
        using CancellationTokenSource v_Cts = new(TimeSpan.FromMilliseconds(300));
        int v_LockCallCount = 0;

        User v_P1 = new() { Id = Guid.NewGuid(), Elo = [new Domain.Bo.Elo { IdTheme = 1, Value = 1000 }] };
        User v_P2 = new() { Id = Guid.NewGuid(), Elo = [new Domain.Bo.Elo { IdTheme = 1, Value = 2000 }] }; // diff=1000 > MaxTolerance=500

        m_CacheServiceMock
            .Setup(p_C => p_C.AcquireLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                v_LockCallCount++;
                return v_LockCallCount == 1;
            });

        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetGetAllWithScoresAsync<User>(RedisKeys.Ranked.MatchmakingList(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(User, int)> { (v_P1, 1000), (v_P2, 2000) });

        m_CacheServiceMock
            .Setup(p_C => p_C.HashGetAllAsync<long>(RedisKeys.Ranked.MatchmakingJoinedAt(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, long>());

        m_CacheServiceMock
            .Setup(p_C => p_C.RefreshLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        m_CacheServiceMock
            .Setup(p_C => p_C.ReleaseLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await v_Worker.StartAsync(v_Cts.Token);
        await Task.Delay(200);
        await v_Worker.StopAsync(CancellationToken.None);

        // Assert: no match created since elo diff exceeds max tolerance
        m_ScopeFactoryMock.Verify(p_F => p_F.CreateScope(), Times.Never);
    }

    // ── Tick: elo tolerance expands with wait time ────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenPlayerWaitedLong_EloToleranceExpandsToAllowMatch()
    {
        // Arrange
        // P1 elo=1000, P2 elo=1150 → diff=150 > BaseEloTolerance(100)
        // After P1 waiting 3 minutes: tolerance = 100 + (3 * 25) = 175 > 150 → match allowed
        MatchmakingWorker v_Worker = CreateWorker();
        using CancellationTokenSource v_Cts = new(TimeSpan.FromMilliseconds(500));
        int v_LockCallCount = 0;

        User v_P1 = new() { Id = Guid.NewGuid(), Elo = [new Domain.Bo.Elo { IdTheme = 1, Value = 1000 }] };
        User v_P2 = new() { Id = Guid.NewGuid(), Elo = [new Domain.Bo.Elo { IdTheme = 1, Value = 1150 }] };

        // P1 joined 3 minutes ago — forces GetEloTolerance to take the expansion path
        long v_ThreeMinutesAgoTicks = DateTime.UtcNow.AddMinutes(-3).Ticks;

        m_CacheServiceMock
            .Setup(p_C => p_C.AcquireLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                v_LockCallCount++;
                return v_LockCallCount == 1;
            });

        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetGetAllWithScoresAsync<User>(RedisKeys.Ranked.MatchmakingList(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(User, int)> { (v_P1, 1000), (v_P2, 1150) });

        m_CacheServiceMock
            .Setup(p_C => p_C.HashGetAllAsync<long>(RedisKeys.Ranked.MatchmakingJoinedAt(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, long> { [v_P1.Id.ToString()] = v_ThreeMinutesAgoTicks });

        // Both players removed from queue
        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetRemoveAsync(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        m_CacheServiceMock
            .Setup(p_C => p_C.HashDeleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Scope + mediator for CreateRankedPartyRequest
        Mock<IServiceScope> v_ScopeMock = new();
        Mock<IServiceProvider> v_ServiceProviderMock = new();
        Mock<IMediator> v_MediatorMock = new();
        v_MediatorMock
            .Setup(p_M => p_M.Send(It.IsAny<CreateRankedPartyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmptyResponse());
        v_ServiceProviderMock
            .Setup(p_Sp => p_Sp.GetService(typeof(IMediator)))
            .Returns(v_MediatorMock.Object);
        v_ScopeMock.Setup(p_S => p_S.ServiceProvider).Returns(v_ServiceProviderMock.Object);
        m_ScopeFactoryMock
            .Setup(p_F => p_F.CreateScope())
            .Returns(v_ScopeMock.Object);

        m_CacheServiceMock
            .Setup(p_C => p_C.RefreshLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        m_CacheServiceMock
            .Setup(p_C => p_C.ReleaseLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await v_Worker.StartAsync(v_Cts.Token);
        await Task.Delay(300);
        await v_Worker.StopAsync(CancellationToken.None);

        // Assert: expanded tolerance allows the match — scope was created
        m_ScopeFactoryMock.Verify(p_F => p_F.CreateScope(), Times.AtLeastOnce);
        v_MediatorMock.Verify(
            p_M => p_M.Send(It.IsAny<CreateRankedPartyRequest>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }
}
