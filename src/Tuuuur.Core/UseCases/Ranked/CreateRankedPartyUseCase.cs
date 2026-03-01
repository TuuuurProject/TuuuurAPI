using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Configuration;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;

namespace Tuuuur.Core.UseCases.Ranked;

/// <summary>
/// Creates a ranked duel party in SQL and registers it in Redis for both players.
/// Called exclusively by the MatchmakingWorker once an Elo-compatible pair is found.
/// Questions are NOT assigned at creation time; they will be loaded when the game starts.
/// </summary>
internal class CreateRankedPartyUseCase(
    IUnitOfWork p_UnitOfWork,
    ILogger<CreateRankedPartyUseCase> p_Logger,
    ICacheService p_CacheService,
    IServiceScopeFactory p_ServiceScopeFactory,
    IRankedNotificationService p_NotificationService,
    RankedConfiguration p_RankedConfiguration)
    : ADbUseCase<CreateRankedPartyRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(CreateRankedPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        User v_Player1 = await m_UnitOfWork.UserRepository.GetUserByIdAsync(p_Request.Player1.Id, p_CancellationToken);
        if (v_Player1 is null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {p_Request.Player1.Id}")]);

        User v_Player2 = await m_UnitOfWork.UserRepository.GetUserByIdAsync(p_Request.Player2.Id, p_CancellationToken);
        if (v_Player2 is null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {p_Request.Player2.Id}")]);

        Party v_Party = new()
        {
            Id = Guid.NewGuid(),
            IdPartyType = (int)PartyTypeType.Ranked,
            IdUserHost = v_Player1.Id,
            Dt = DateTime.UtcNow,
            Active = true,
            Finish = false,
            InProgress = true,
            PartyUsers =
            [
                new PartyUser { IdUser = v_Player1.Id, User =  v_Player1 },
                new PartyUser { IdUser = v_Player2.Id, User =  v_Player2 }
            ],
            PartyQuestions = []
        };

        await p_CacheService.SetAsync(RedisKeys.Ranked.ById(v_Party.Id), v_Party, p_CancellationToken: p_CancellationToken);

        await p_CacheService.SetAsync(
            RedisKeys.Ranked.CurrentQuestionIndex(v_Party.Id),
            0, p_CancellationToken: p_CancellationToken);

        // Store the ranked party ID for each player in Redis so they can retrieve it on reconnect
        await p_CacheService.SetAsync(RedisKeys.User.UserRanked(v_Player1.Id), v_Party.Id, p_CancellationToken: p_CancellationToken);
        await p_CacheService.SetAsync(RedisKeys.User.UserRanked(v_Player2.Id), v_Party.Id, p_CancellationToken: p_CancellationToken);

        // Init score for players
        await p_CacheService.SortedSetAddAsync(RedisKeys.Ranked.Scores(v_Party.Id), v_Player1, p_RankedConfiguration.InitialRankedScore, p_CancellationToken: p_CancellationToken);
        await p_CacheService.SortedSetAddAsync(RedisKeys.Ranked.Scores(v_Party.Id), v_Player2, p_RankedConfiguration.InitialRankedScore, p_CancellationToken: p_CancellationToken);

        m_Logger.LogInformation(
            "Ranked party {PartyId} created for {P1} (elo={E1}) vs {P2} (elo={E2})",
            v_Party.Id, v_Player1.Id, v_Player1.GlobalElo, v_Player2.Id, v_Player2.GlobalElo);

        await p_NotificationService.NotifyMatchFoundAsync(v_Player1, v_Player2, v_Party.Id);

        // Execute on background thread to not block the client
        // Create a new scope to avoid disposed dependencies
        _ = Task.Run(async () =>
        {
            try
            {
                // Add a small delay to ensure the response is sent before starting the game loop
                await Task.Delay(500, CancellationToken.None);

                // Create a new scope for the background task
                using IServiceScope v_Scope = p_ServiceScopeFactory.CreateScope();
                IMediator v_Mediator = v_Scope.ServiceProvider.GetRequiredService<IMediator>();

                // Launch party logic
                RankedLogicRequest v_GroupLogicRequest = new(v_Party.Id);
                _ = await v_Mediator.Send(v_GroupLogicRequest, CancellationToken.None);
            }
            catch (Exception v_Exception)
            {
                p_Logger.LogError(v_Exception, "StartGroupUseCase (Background): Error during party game loop execution for party {PartyId}", v_Party.Id);
            }
        }, CancellationToken.None);

        return new EmptyResponse();
    }
}
