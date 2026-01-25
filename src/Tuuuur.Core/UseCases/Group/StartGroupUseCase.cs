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
        m_Logger.LogInformation("StartGroupUseCase: Starting a group party for request: {Request}", p_PartyRequest);

        string v_UserEmail = p_PartyRequest.UserEmail;
        m_Logger.LogInformation("StartGroupUseCase: Current user email: {UserEmail}", v_UserEmail);

        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);

        if (v_User == null)
        {
            m_Logger.LogWarning("StartGroupUseCase: User not found for email: {UserEmail}", v_UserEmail);
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
        }
        m_Logger.LogInformation("StartGroupUseCase: User found: {UserId}", v_User.Id);

        string v_PartyCode = await p_CacheService.GetAsync<string>(RedisKeys.User.UserParty(v_User.Id), p_CancellationToken);
        m_Logger.LogInformation("StartGroupUseCase: Party code from cache for user {UserId}: {PartyCode}", v_User.Id, v_PartyCode);

        if (v_PartyCode is null)
        {
            m_Logger.LogWarning("StartGroupUseCase: Party code not found in cache for user {UserId}", v_User.Id);
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        }

        GroupParty v_Party = await p_CacheService.GetAsync<GroupParty>(RedisKeys.Party.ByCode(v_PartyCode), p_CancellationToken);
        m_Logger.LogInformation("StartGroupUseCase: Party object retrieved from cache for code {PartyCode}", v_PartyCode);

        // If user is not in the party and user is not user host
        if (v_PartyCode != v_Party.Code || v_Party.UserHost.Id != v_User.Id)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(GroupParty)} was not found")]);

        if (v_Party.InProgress)
        {
            m_Logger.LogWarning("StartGroupUseCase: Party {PartyCode} is already in progress.", v_PartyCode);
            return new EmptyResponse([new ErrorDto(DomainErrors.Party.InProgress, $"You can't start a party already started")]);
        }

        if (v_Party.NbQuestions is not (5 or 10 or 15 or 20) || v_Party.Difficulties.Count == 0 ||
            v_Party.Themes.Count == 0)
        {
            m_Logger.LogWarning("StartGroupUseCase: Party {PartyCode} has invalid settings. Questions: {NbQuestions}, Difficulties: {Difficulties}, Themes: {Themes}", v_PartyCode, v_Party.NbQuestions, v_Party.PartyDifficulty.Count, v_Party.PartyTheme.Count);
            return new EmptyResponse([new ErrorDto(DomainErrors.Party.InvalidSettings, $"Party settings are invalid")]);
        }
        m_Logger.LogInformation("StartGroupUseCase: Party {PartyCode} settings are valid.", v_PartyCode);

        m_Logger.LogInformation("StartGroupUseCase: Fetching {NbQuestions} questions for party {PartyCode}.", v_Party.NbQuestions, v_PartyCode);
        IEnumerable<Question> v_Questions = await m_UnitOfWork.QuestionRepository
            .GetQuestionsByThemesIdsAndDifficultiesIdsAndNumberOfQuestionsAsync(v_Party.Themes.Select(p_P => p_P.Id),
                v_Party.Difficulties.Select(p_P => p_P.Id), v_Party.NbQuestions, p_CancellationToken);

        List<Question> v_Enumerable = v_Questions.ToList();
        m_Logger.LogInformation("StartGroupUseCase: Fetched {Count} questions for party {PartyCode}.", v_Enumerable.Count, v_PartyCode);

        m_Logger.LogInformation("StartGroupUseCase: Adding questions to cache for party {PartyCode}.", v_PartyCode);
        for (int v_Index = 0; v_Index < v_Enumerable.Count; v_Index++)
        {
            m_Logger.LogDebug("StartGroupUseCase: Adding question {QuestionId} with index {Index} to cache for party {PartyCode}.", v_Enumerable[v_Index].Id, v_Index, v_PartyCode);
            await p_CacheService.SortedSetAddAsync(
                RedisKeys.Party.Questions(v_Party.Code),
                v_Enumerable[v_Index],
                p_Score: v_Index, p_CancellationToken: p_CancellationToken);
        }
        m_Logger.LogInformation("StartGroupUseCase: Finished adding questions to cache for party {PartyCode}.", v_PartyCode);


        // Put the question index to 0
        m_Logger.LogInformation("StartGroupUseCase: Setting current question index to 0 for party {PartyCode}.", v_PartyCode);
        await p_CacheService.SetAsync(
            RedisKeys.Party.CurrentQuestionIndex(v_Party.Code),
            0, p_CancellationToken: p_CancellationToken);

        v_Party.InProgress = true;
        m_Logger.LogInformation("StartGroupUseCase: Setting party {PartyCode} to InProgress.", v_PartyCode);
        await p_CacheService.SetAsync(RedisKeys.Party.ByCode(v_Party.Code), v_Party, p_CancellationToken: p_CancellationToken);

        List<int> v_UserIds = await p_CacheService.SetMembersAsync<int>(
            RedisKeys.Party.Users(v_Party.Code),
            CancellationToken.None
        );
        m_Logger.LogInformation("StartGroupUseCase: Fetched {UserCount} user IDs from cache for party {PartyCode}: {UserIds}", v_UserIds.Count, v_PartyCode, string.Join(", ", v_UserIds));

        // Get all users in a single database query to avoid DbContext concurrency issues
        m_Logger.LogInformation("StartGroupUseCase: Fetching user details from repository for party {PartyCode}.", v_PartyCode);
        List<User> v_Users = await m_UnitOfWork.UserRepository.GetUsersByIdsAsync(v_UserIds, p_CancellationToken);
        m_Logger.LogInformation("StartGroupUseCase: Fetched {UserCount} user objects for party {PartyCode}.", v_Users.Count, v_PartyCode);

        // Initialize scores for all users in parallel
        m_Logger.LogInformation("StartGroupUseCase: Initializing scores for {UserCount} users in party {PartyCode}.", v_Users.Count, v_PartyCode);
        IEnumerable<Task> v_InitScoreTasks = v_Users.Select(async p_User =>
        {
            m_Logger.LogDebug("StartGroupUseCase: Initializing score for user {UserId} in party {PartyCode}.", p_User.Id, v_PartyCode);
            await p_CacheService.SortedSetAddAsync(
                RedisKeys.Party.Scores(v_Party.Code),
                p_User,
                0,
                p_CancellationToken
            );
        });

        await Task.WhenAll(v_InitScoreTasks);
        m_Logger.LogInformation("StartGroupUseCase: Finished initializing scores for party {PartyCode}.", v_PartyCode);

        // Notify all players via WebSocket that party is updated
        m_Logger.LogInformation("StartGroupUseCase: Notifying clients that party {PartyCode} has started.", v_PartyCode);
        await p_GroupNotificationService.NotifyPartyStartedAsync(
            v_Party.Code,
            v_Party
        );
        m_Logger.LogInformation("StartGroupUseCase: Notification sent for party {PartyCode}.", v_PartyCode);

        // Execute on background thread to not block the client
        // Create a new scope to avoid disposed dependencies
        m_Logger.LogInformation("StartGroupUseCase: Starting background task for game loop for party {PartyCode}.", v_PartyCode);
        _ = Task.Run(async () =>
        {
            try
            {
                m_Logger.LogInformation("StartGroupUseCase (Background): Game loop task started for party {PartyCode}.", v_Party.Code);
                // Add a small delay to ensure the response is sent before starting the game loop
                await Task.Delay(500, CancellationToken.None);

                // Create a new scope for the background task
                using IServiceScope v_Scope = p_ServiceScopeFactory.CreateScope();
                IMediator v_Mediator = v_Scope.ServiceProvider.GetRequiredService<IMediator>();

                // Launch party logic
                GroupLogicRequest v_GroupLogicRequest = new(v_Party.Code);
                m_Logger.LogInformation("StartGroupUseCase (Background): Sending GroupLogicRequest for party {PartyCode}.", v_Party.Code);
                _ = await v_Mediator.Send(v_GroupLogicRequest, CancellationToken.None);
                m_Logger.LogInformation("StartGroupUseCase (Background): GroupLogicRequest sent for party {PartyCode}.", v_Party.Code);
            }
            catch (Exception v_Exception)
            {
                p_Logger.LogError(v_Exception, "StartGroupUseCase (Background): Error during party game loop execution for party {PartyCode}", v_Party.Code);
            }
        }, CancellationToken.None);

        m_Logger.LogInformation("StartGroupUseCase: Finished request for party {PartyCode}.", v_PartyCode);
        return new EmptyResponse();
    }
}
