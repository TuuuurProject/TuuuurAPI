using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Notifications;

namespace Tuuuur.API.Workers;

/// <summary>
/// Background service that runs the matchmaking loop.
///
/// Distributed leader election:
///   Many API replicas can run simultaneously. Only ONE instance at a time
///   performs matchmaking, enforced by a Redis distributed lock.
///   If the leader crashes, the lock expires (TTL = LockExpirySeconds) and
///   another replica takes over automatically — no human intervention needed.
///
/// Elo matching:
///   Players are stored in a Redis sorted set with their GlobalElo as the score.
///   The worker iterates pairs in ascending Elo order and matches the first two
///   players whose Elo difference is within the tolerance window.
///   Tolerance starts at <see cref="BaseEloTolerance"/> and grows by
///   <see cref="EloExpansionStep"/> for every interval spent in the queue,
///   capped at <see cref="MaxEloTolerance"/>.
/// </summary>
public class MatchmakingWorker(
    ICacheService p_CacheService,
    IRankedNotificationService p_NotificationService,
    IServiceScopeFactory p_ScopeFactory,
    IConfiguration p_Configuration,
    ILogger<MatchmakingWorker> p_Logger)
    : BackgroundService
{
    // ── Configuration (overridable via appsettings.json "MatchmakingWorker" section) ──

    /// <summary>Base Elo difference a freshly queued player accepts.</summary>
    private int BaseEloTolerance => p_Configuration.GetValue("MatchmakingWorker:BaseEloTolerance", 150);

    /// <summary>Additional Elo tolerance gained per step.</summary>
    private int EloExpansionStep => p_Configuration.GetValue("MatchmakingWorker:EloExpansionStep", 25);

    /// <summary>Interval in seconds for each Elo expansion step.</summary>
    private int EloExpansionIntervalSeconds => p_Configuration.GetValue("MatchmakingWorker:EloExpansionIntervalSeconds", 5);

    /// <summary>Maximum Elo tolerance regardless of wait time.</summary>
    private int MaxEloTolerance => p_Configuration.GetValue("MatchmakingWorker:MaxEloTolerance", 750);

    /// <summary>How long it takes for the lock to expire if the leader crashes (seconds).</summary>
    private int LockExpirySeconds => p_Configuration.GetValue("MatchmakingWorker:LockExpirySeconds", 15);

    /// <summary>Delay between matchmaking ticks when leader (milliseconds).</summary>
    private int TickIntervalMs => p_Configuration.GetValue("MatchmakingWorker:TickIntervalMs", 500);

    /// <summary>Delay between lock-acquisition retries when standby (milliseconds).</summary>
    private int StandbyIntervalMs => p_Configuration.GetValue("MatchmakingWorker:StandbyIntervalMs", 1000);

    // ── Instance identity ──────────────────────────────────────────────────────────────

    /// <summary>Unique ID that identifies this process as the lock owner.</summary>
    private readonly string m_InstanceId = Guid.NewGuid().ToString("N");

    // ──────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Match making worder
    /// </summary>
    /// <param name="p_StoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken p_StoppingToken)
    {
        p_Logger.LogInformation(
            "MatchmakingWorker started (instanceId={InstanceId})", m_InstanceId);

        while (!p_StoppingToken.IsCancellationRequested)
        {
            try
            {
                bool v_Acquired = await p_CacheService.AcquireLockAsync(
                    RedisKeys.Ranked.MatchmakingLock(),
                    m_InstanceId,
                    TimeSpan.FromSeconds(LockExpirySeconds),
                    p_StoppingToken);

                if (v_Acquired)
                {
                    p_Logger.LogInformation(
                        "MatchmakingWorker acquired leader lock (instanceId={InstanceId})", m_InstanceId);

                    await RunLeaderLoopAsync(p_StoppingToken);

                    p_Logger.LogWarning(
                        "MatchmakingWorker lost leader lock, entering standby (instanceId={InstanceId})", m_InstanceId);
                }
            }
            catch (Exception v_Ex) when (v_Ex is not OperationCanceledException)
            {
                p_Logger.LogError(v_Ex, "MatchmakingWorker outer loop error (instanceId={InstanceId})", m_InstanceId);
            }

            // Standby: wait before the next acquisition attempt
            await Task.Delay(StandbyIntervalMs, p_StoppingToken);
        }

        // On graceful shutdown, release the lock immediately so another replica can take over
        await p_CacheService.ReleaseLockAsync(
            RedisKeys.Ranked.MatchmakingLock(),
            m_InstanceId, p_StoppingToken);

        p_Logger.LogInformation(
            "MatchmakingWorker stopped (instanceId={InstanceId})", m_InstanceId);
    }

    /// <summary>
    /// Inner loop that runs while this instance holds the leader lock.
    /// Exits when the lock can no longer be refreshed (crash-failover scenario).
    /// </summary>
    private async Task RunLeaderLoopAsync(CancellationToken p_StoppingToken)
    {
        while (!p_StoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunMatchmakingTickAsync(p_StoppingToken);
            }
            catch (Exception v_Ex) when (v_Ex is not OperationCanceledException)
            {
                p_Logger.LogError(v_Ex, "Matchmaking tick error (instanceId={InstanceId})", m_InstanceId);
            }

            // Refresh the lock AFTER each tick —
            // if refresh fails it means the key expired (very slow tick) or was evicted.
            bool v_Refreshed = await p_CacheService.RefreshLockAsync(
                RedisKeys.Ranked.MatchmakingLock(),
                m_InstanceId,
                TimeSpan.FromSeconds(LockExpirySeconds),
                p_StoppingToken);

            if (!v_Refreshed)
                return; // Lock lost — fall back to standby

            await Task.Delay(TickIntervalMs, p_StoppingToken);
        }
    }

    /// <summary>
    /// One matchmaking iteration: reads the queue, pairs compatible players,
    /// creates ranked parties and notifies players.
    /// </summary>
    private async Task RunMatchmakingTickAsync(CancellationToken p_CancellationToken)
    {
        // Read players ordered by Elo (ascending) with their scores
        List<(User Value, int Score)> v_Players =
            await p_CacheService.SortedSetGetAllWithScoresAsync<User>(
                RedisKeys.Ranked.MatchmakingList(),
                p_Descending: false,
                p_CancellationToken);

        if (v_Players.Count < 2)
            return;

        // Read join timestamps for progressive Elo widening
        Dictionary<string, long> v_JoinTimes =
            await p_CacheService.HashGetAllAsync<long>(
                RedisKeys.Ranked.MatchmakingJoinedAt(),
                p_CancellationToken);

        HashSet<int> v_Matched = [];

        for (int v_I = 0; v_I < v_Players.Count - 1; v_I++)
        {
            if (v_Matched.Contains(v_I))
                continue;

            (User v_Player1, int v_Elo1) = v_Players[v_I];
            int v_Tolerance1 = GetEloTolerance(v_Player1, v_JoinTimes);

            for (int v_J = v_I + 1; v_J < v_Players.Count; v_J++)
            {
                if (v_Matched.Contains(v_J))
                    continue;

                (User v_Player2, int v_Elo2) = v_Players[v_J];

                // The sorted set is sorted ascending, so all subsequent players have higher elo.
                // If the gap already exceeds the maximum possible tolerance, stop searching.
                int v_EloDiff = Math.Abs(v_Elo1 - v_Elo2);
                if (v_EloDiff > MaxEloTolerance)
                    break;

                // Use the broader of the two players' tolerances (fairest approach)
                int v_Tolerance2 = GetEloTolerance(v_Player2, v_JoinTimes);
                int v_EffectiveTolerance = Math.Max(v_Tolerance1, v_Tolerance2);

                if (v_EloDiff > v_EffectiveTolerance)
                {
                    continue;
                }

                await ProcessMatchAsync(v_Player1, v_Player2, p_CancellationToken);
                v_Matched.Add(v_I);
                v_Matched.Add(v_J);
                break;
            }
        }
    }

    /// <summary>
    /// Finalizes a match: removes both players from the queue, creates the party,
    /// and notifies both players via SignalR.
    /// </summary>
    private async Task ProcessMatchAsync(User p_Player1, User p_Player2, CancellationToken p_CancellationToken)
    {
        p_Logger.LogInformation(
            "Matchmaking: pairing {P1} (elo={E1}) with {P2} (elo={E2})",
            p_Player1.Id, p_Player1.GlobalElo, p_Player2.Id, p_Player2.GlobalElo);

        // Remove from queue (worker holds the lock — no concurrent worker can be doing the same)
        await Task.WhenAll(
            p_CacheService.SortedSetRemoveAsync(RedisKeys.Ranked.MatchmakingList(), p_Player1, p_CancellationToken),
            p_CacheService.SortedSetRemoveAsync(RedisKeys.Ranked.MatchmakingList(), p_Player2, p_CancellationToken),
            p_CacheService.HashDeleteAsync(RedisKeys.Ranked.MatchmakingJoinedAt(), p_Player1.Id.ToString(), p_CancellationToken),
            p_CacheService.HashDeleteAsync(RedisKeys.Ranked.MatchmakingJoinedAt(), p_Player2.Id.ToString(), p_CancellationToken)
        );

        // Create ranked party in SQL + Redis (scoped services)
        using IServiceScope v_Scope = p_ScopeFactory.CreateScope();
        IMediator v_Mediator = v_Scope.ServiceProvider.GetRequiredService<IMediator>();
        EmptyResponse v_Response = await v_Mediator.Send(
            new CreateRankedPartyRequest(p_Player1, p_Player2),
            p_CancellationToken);

        if (!v_Response.Success)
        {
            p_Logger.LogError(
                "Failed to create ranked party for {P1} vs {P2}: {Errors}",
                p_Player1.Id, p_Player2.Id,
                string.Join(", ", v_Response.Errors.Select(p_ErrorDto => p_ErrorDto.Description)));
        }
    }

    /// <summary>
    /// Computes the effective Elo tolerance for a player based on how long they have waited.
    /// </summary>
    private int GetEloTolerance(User p_Player, Dictionary<string, long> p_JoinTimes)
    {
        if (!p_JoinTimes.TryGetValue(p_Player.Id.ToString(), out long v_Ticks))
        {
            return BaseEloTolerance;
        }

        TimeSpan v_WaitTime = DateTime.UtcNow - new DateTime(v_Ticks, DateTimeKind.Utc);
        int v_Intervals = (int)(v_WaitTime.TotalSeconds / EloExpansionIntervalSeconds);
        int v_Expansion = v_Intervals * EloExpansionStep;
        return Math.Min(BaseEloTolerance + v_Expansion, MaxEloTolerance);
    }
}
