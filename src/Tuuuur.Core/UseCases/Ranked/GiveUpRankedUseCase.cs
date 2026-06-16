using Microsoft.Extensions.Logging;
using Tuuuur.Core.Configuration;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;

namespace Tuuuur.Core.UseCases.Ranked;

internal class GiveUpRankedUseCase(
    IUnitOfWork p_UnitOfWork,
    ILogger<GiveUpRankedUseCase> p_Logger,
    ICacheService p_CacheService,
    IRankedNotificationService p_RankedNotificationService,
    RankedConfiguration p_RankedConfiguration) :
    ADbUseCase<GiveUpRankedRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(GiveUpRankedRequest p_Request, CancellationToken p_CancellationToken)
    {
        // Get the abandoning user
        User v_QuittingUser = await m_UnitOfWork.UserRepository.GetUserByIdAsync(p_Request.UserId, p_CancellationToken);
        if (v_QuittingUser is null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {p_Request.UserId}")]);

        // Get party ID from cache
        string v_GetPartyId = await p_CacheService.GetAsync<string>(RedisKeys.User.UserRanked(p_Request.UserId), p_CancellationToken);
        Guid v_PartyId;
        try
        {
            v_PartyId = Guid.Parse(v_GetPartyId);
        }
        catch (Exception)
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        }

        // Get the party from cache
        Party v_Party = await p_CacheService.GetAsync<Party>(RedisKeys.Ranked.ById(v_PartyId), p_CancellationToken);
        if (v_Party is null || v_PartyId != v_Party.Id)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        
        // Get the index of question to get
        int v_CurrentIndex = await p_CacheService.GetAsync<int>(RedisKeys.Ranked.CurrentQuestionIndex(v_Party.Id), p_CancellationToken);
        
        Question v_Question = await p_CacheService.SortedSetGetByIndexAsync<Question>(
            RedisKeys.Ranked.Questions(v_Party.Id),
            p_Index: v_CurrentIndex, p_CancellationToken: p_CancellationToken);

        // Mark the player as forfeited first so the game loop detects it even if no question is loaded yet
        await p_CacheService.SetAsync(RedisKeys.Ranked.PlayerForfeited(v_PartyId), v_QuittingUser, p_RankedConfiguration.PartyTtl, p_CancellationToken);

        // Only unblock the current round's wait if a question is already in progress
        if (v_Question is not null)
        {
            await p_CacheService.PublishAsync(
                RedisKeys.Ranked.PartyQuestionAllAnsweredChannel(v_Party.Id, v_Question.Id),
                true,
                p_CancellationToken
            );
        }

        User v_OtherUser = v_Party.PartyUsers.FirstOrDefault(p_P => p_P.IdUser != v_QuittingUser.Id)?.User;

        if (v_OtherUser != null)
        {
            await p_RankedNotificationService.NotifyPlayerForfeited(v_OtherUser.Id, v_QuittingUser);
        }

        return new EmptyResponse();
    }
}