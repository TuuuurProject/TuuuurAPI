using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;

namespace Tuuuur.Core.UseCases.Ranked;

internal class JoinSearchOpponentUseCase(
    IUnitOfWork p_UnitOfWork,
    ILogger<JoinSearchOpponentUseCase> p_Logger,
    ICacheService p_CacheService) :
    ADbUseCase<JoinSearchOpponentRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(JoinSearchOpponentRequest p_Request, CancellationToken p_CancellationToken)
    {
        User v_User = await m_UnitOfWork.UserRepository.GetUserByIdAsync(p_Request.UserId, p_CancellationToken);
        if (v_User is null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {p_Request.UserId}")]);

        string v_PartyId = await p_CacheService.GetAsync<string>(RedisKeys.User.UserRanked(p_Request.UserId), p_CancellationToken) ?? string.Empty;

        List<(User Value, int Score)> v_UserInMatchmaking = await p_CacheService.SortedSetGetAllWithScoresAsync<User>(RedisKeys.Ranked.MatchmakingList(), true, p_CancellationToken);

        // If the user is not already in the queue and has no active ranked party, add them
        if (v_PartyId != string.Empty && v_UserInMatchmaking.Any(p_Tuple => p_Tuple.Value.Id == p_Request.UserId))
        {
            // TODO : User already in matchmaking or party
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {p_Request.UserId}")]);
        }

        await p_CacheService.SortedSetAddAsync(RedisKeys.Ranked.MatchmakingList(), v_User, v_User.GlobalElo, TimeSpan.FromHours(24), p_CancellationToken);

        // Record join timestamp so the worker can progressively widen the Elo tolerance
        await p_CacheService.HashSetAsync(
            RedisKeys.Ranked.MatchmakingJoinedAt(),
            p_Request.UserId.ToString(),
            DateTime.UtcNow.Ticks,
            TimeSpan.FromHours(24),
            p_CancellationToken);

        return new EmptyResponse();
    }
}
