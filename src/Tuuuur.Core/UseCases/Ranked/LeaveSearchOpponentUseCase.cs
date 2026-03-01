using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;

namespace Tuuuur.Core.UseCases.Ranked;

internal class LeaveSearchOpponentUseCase(
    IUnitOfWork p_UnitOfWork,
    ILogger<LeaveSearchOpponentUseCase> p_Logger,
    ICacheService p_CacheService) :
    ADbUseCase<LeaveSeachOpponentRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(LeaveSeachOpponentRequest p_Request, CancellationToken p_CancellationToken)
    {
        User v_User = await m_UnitOfWork.UserRepository.GetUserByIdAsync(p_Request.UserId, p_CancellationToken);
        if (v_User is null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {p_Request.UserId}")]);

        List<(User Value, int Score)> v_UserInMatchmaking = await p_CacheService.SortedSetGetAllWithScoresAsync<User>(RedisKeys.Ranked.MatchmakingList(), true, p_CancellationToken);

        if (v_UserInMatchmaking.All(p_Tuple => p_Tuple.Value.Id != p_Request.UserId))
        {
            return new EmptyResponse();
        }

        await p_CacheService.SortedSetRemoveAsync(RedisKeys.Ranked.MatchmakingList(), v_User, p_CancellationToken);

        // Remove join timestamp
        await p_CacheService.HashDeleteAsync(
            RedisKeys.Ranked.MatchmakingJoinedAt(),
            p_Request.UserId.ToString(),
            p_CancellationToken);

        return new EmptyResponse();
    }
}