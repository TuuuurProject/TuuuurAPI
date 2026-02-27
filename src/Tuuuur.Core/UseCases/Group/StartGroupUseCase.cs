using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;

namespace Tuuuur.Core.UseCases.Group;

internal class StartGroupUseCase(
    IUnitOfWork p_UnitOfWork,
    ILogger<StartGroupUseCase> p_Logger,
    ICacheService p_CacheService,
    IGroupNotificationService p_GroupNotificationService,
    IServiceScopeFactory p_ServiceScopeFactory)
    : ADbUseCase<StartGroupPartyRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(StartGroupPartyRequest p_PartyRequest, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_PartyRequest.UserEmail;

        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);

        if (v_User == null)
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
        }

        string v_PartyCode = await p_CacheService.GetAsync<string>(RedisKeys.User.UserParty(v_User.Id), p_CancellationToken);

        if (v_PartyCode is null)
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        }

        GroupParty v_Party = await p_CacheService.GetAsync<GroupParty>(RedisKeys.Party.ByCode(v_PartyCode), p_CancellationToken);

        // If user is not in the party and user is not user host
        if (v_PartyCode != v_Party.Code || v_Party.IdUserHost != v_User.Id)
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        }

        if (v_Party.InProgress)
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Party.InProgress, $"You can't start a party already started")]);
        }

        if (v_Party.NbQuestions is not (5 or 10 or 15 or 20) || v_Party.PartyDifficulty.Count == 0 ||
            v_Party.PartyTheme.Count == 0)
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Party.InvalidSettings, $"Party settings are invalid")]);
        }

        IEnumerable<Question> v_Questions = await m_UnitOfWork.QuestionRepository
            .GetQuestionsByThemesIdsAndDifficultiesIdsAndNumberOfQuestionsAsync(v_Party.PartyTheme.Select(p_P => p_P.IdTheme),
                v_Party.PartyDifficulty.Select(p_P => p_P.IdDifficulty), v_Party.NbQuestions, p_CancellationToken);

        List<Question> v_Enumerable = v_Questions.ToList();

        for (int v_Index = 0; v_Index < v_Enumerable.Count; v_Index++)
        {
            await p_CacheService.SortedSetAddAsync(
                RedisKeys.Party.Questions(v_Party.Code),
                v_Enumerable[v_Index],
                p_Score: v_Index, p_CancellationToken: p_CancellationToken);
        }

        // Put the question index to 0
        await p_CacheService.SetAsync(
            RedisKeys.Party.CurrentQuestionIndex(v_Party.Code),
            0, p_CancellationToken: p_CancellationToken);

        v_Party.InProgress = true;
        await p_CacheService.SetAsync(RedisKeys.Party.ByCode(v_Party.Code), v_Party, p_CancellationToken: p_CancellationToken);

        List<User> v_Users = await p_CacheService.SetMembersAsync<User>(
            RedisKeys.Party.Users(v_Party.Code),
            CancellationToken.None
        );

        // Initialize scores for all users in parallel
        IEnumerable<Task> v_InitScoreTasks = v_Users.Select(async p_User =>
        {
            await p_CacheService.SortedSetAddAsync(
                RedisKeys.Party.Scores(v_Party.Code),
                p_User,
                0,
                TimeSpan.Zero,
                p_CancellationToken
            );
        });

        await Task.WhenAll(v_InitScoreTasks);

        v_Party.PartyUsers.AddRange(v_Users.Select(p_P => new PartyUser { User = p_P }));

        // Notify all players via WebSocket that party is updated
        await p_GroupNotificationService.NotifyPartyStartedAsync(
            v_Party.Code,
            v_Party
        );

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
                GroupLogicRequest v_GroupLogicRequest = new(v_Party.Code);
                _ = await v_Mediator.Send(v_GroupLogicRequest, CancellationToken.None);
            }
            catch (Exception v_Exception)
            {
                p_Logger.LogError(v_Exception, "StartGroupUseCase (Background): Error during party game loop execution for party {PartyCode}", v_Party.Code);
            }
        }, CancellationToken.None);

        return new EmptyResponse();
    }
}
