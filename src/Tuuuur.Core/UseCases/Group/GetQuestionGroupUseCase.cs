using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Services;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Group;

internal class GetQuestionGroupUseCase(
    IUnitOfWork p_UnitOfWork,
    IUserRoleService p_UserRoleService,
    ICacheService p_CacheService,
    IGroupNotificationService p_GroupNotificationService,
    ICalculService p_CalculService,
    IMediator p_Mediator,
    ILogger<GetQuestionGroupUseCase> p_Logger)
    : ADbUseCase<GetQuestionGroupRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(GetQuestionGroupRequest p_PartyRequest, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);

        if (v_User == null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);

        Guid? v_Parties = await p_CacheService.GetAsync<Guid?>(RedisKeys.User.UserParty(v_User.Id), p_CancellationToken);

        if (v_Parties is null)
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        }

        GroupParty v_Party = await p_CacheService.GetAsync<GroupParty>(RedisKeys.Party.ById(v_Parties.Value), p_CancellationToken);
        Guid v_CurrentParty = await p_CacheService.GetAsync<Guid>(RedisKeys.User.UserParty(v_User.Id), p_CancellationToken: p_CancellationToken);

        // If user is not in the party and party is not in progress
        if (v_CurrentParty != v_Party.Id || v_Party.IdUserHost != v_User.Id || !v_Party.InProgress)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);

        // Get the index of question to get
        int v_CurrentIndex = await p_CacheService.GetAsync<int>(RedisKeys.Party.CurrentQuestionIndex(v_Party.Id), p_CancellationToken);

        // Get the question
        GroupQuestion v_CurrentQuestion = await p_CacheService.SortedSetGetByIndexAsync<GroupQuestion>(
            RedisKeys.Party.Questions(v_Party.Id),
            p_Index: v_CurrentIndex, p_CancellationToken: p_CancellationToken);

        GroupQuestion v_Question = await m_UnitOfWork.QuestionRepository.GetQuestionByIdWithAnswerAsync(v_CurrentQuestion.Id, p_CancellationToken);
        v_Question.ClearAnswer();

        // Send countdown (3, 2, 1)
        for (int v_I = 3; v_I > 0; v_I--)
        {
            await p_GroupNotificationService.NotifyCountdownAsync(
                v_Party.Id,
                v_I
            );
            await Task.Delay(1000, p_CancellationToken);
        }

        List<int> v_UserIds = await p_CacheService.SetMembersAsync<int>(
            RedisKeys.Party.Users(v_Party.Id),
            CancellationToken.None
        );

        // Update question state for each user in parallel
        IEnumerable<Task> v_UpdateQuestionTasks = v_UserIds.Select(async p_UserId =>
        {
            UserPartyQuestion v_UserPartyQuestion = new()
            {
                IdUser = p_UserId,
                Correct = null,
                DtPresentedAt = DateTime.Now,
                AnswersOrder = Guid.NewGuid()
            };

            // Create a shuffled copy of answers for this user
            int v_Seed = v_UserPartyQuestion.AnswersOrder.GetHashCode();
            Random v_Random = new(v_Seed);
            v_Question.Answer = v_Question.Answer.OrderBy(_ => v_Random.Next()).ToList();

            await p_CacheService.SetAsync(
                RedisKeys.Party.PartyQuestionUserAnswer(v_Party.Id, v_Question.Id, p_UserId),
                v_UserPartyQuestion,
                p_CancellationToken: p_CancellationToken
            );
            
            v_Question.CurrentIndex = v_CurrentIndex + 1;

            // Send the question to the specific user
            await p_GroupNotificationService.NotifyPartyQuestionSend(
                p_UserId,
                v_Question
            );
        });

        // Send question for all users in the same time
        await Task.WhenAll(v_UpdateQuestionTasks);

        // Wait for players to respond 
        await Task.Delay(TimeSpan.FromSeconds(15), p_CancellationToken);

        // Fetch current scores from Redis
        List<(User User, int Score)> v_CurrentScores = await p_CacheService.SortedSetGetAllWithScoresAsync<User>(
            RedisKeys.Party.Scores(v_Party.Id),
            p_Descending: true,
            p_CancellationToken: p_CancellationToken
        );

        v_Question = await m_UnitOfWork.QuestionRepository.GetQuestionByIdWithAnswerAsync(v_CurrentQuestion.Id, p_CancellationToken);

        // Update scores for all users in parallel
        IEnumerable<Task<UserScore>> v_UpdateScoreTasks = v_UserIds.Select(async p_UserId =>
        {
            UserPartyQuestion v_UserPartyQuestion = await p_CacheService.GetAsync<UserPartyQuestion>(
                RedisKeys.Party.PartyQuestionUserAnswer(v_Party.Id, v_Question.Id, p_UserId),
                p_CancellationToken: p_CancellationToken
            );

            if (v_UserPartyQuestion != null)
            {
                // Answer can be null if the request was sent without response
                Answer v_Answer = v_Question.Answer.FirstOrDefault(p_P => p_P.Id == v_UserPartyQuestion.IdAnswer);
                
                int v_Score = p_CalculService.CalculateScore(v_UserPartyQuestion.DtPresentedAt, v_UserPartyQuestion.DtAnsweredAt);

                if (v_Answer is null)
                {
                    v_UserPartyQuestion.IdAnswer = null;
                    v_UserPartyQuestion.Correct = false;
                    v_UserPartyQuestion.Score = 0;
                }
                else
                {
                    v_UserPartyQuestion.IdAnswer = v_Answer.Id;
                    if (v_Answer.Valid.HasValue && v_Answer.Valid.Value && v_Score > 0)
                    {
                        v_UserPartyQuestion.Correct = true;
                        v_UserPartyQuestion.Score = v_Score;
                    }
                    else
                    {
                        v_UserPartyQuestion.Score = 0;
                        v_UserPartyQuestion.Correct = false;
                    }
                }

                // Find existing score
                (User User, int Score) v_ExistingScore = v_CurrentScores.FirstOrDefault(p_S => p_S.User.Id == p_UserId);
                int v_TotalScore = v_ExistingScore.Score + v_UserPartyQuestion.Score;

                // Update party user
                await p_CacheService.SetAsync(RedisKeys.Party.PartyQuestionUserAnswer(v_Party.Id, v_Question.Id, p_UserId), v_UserPartyQuestion, p_CancellationToken: p_CancellationToken);

                // Update score directly in Redis
                if (v_ExistingScore.User != null)
                {
                    await p_CacheService.SortedSetAddAsync(
                        RedisKeys.Party.Scores(v_Party.Id),
                        v_ExistingScore.User,
                        v_TotalScore,
                        p_CancellationToken
                    );
                }
                
                int v_Seed = v_UserPartyQuestion.AnswersOrder.GetHashCode();
                Random v_Random = new(v_Seed);
                v_Question.Answer = v_Question.Answer.OrderBy(_ => v_Random.Next()).ToList();
                
                v_Question.CurrentIndex = v_CurrentIndex + 1;
                v_Question.Score = v_Score;

                // Send the question with correct answer after countdown
                await p_GroupNotificationService.NotifyPartyQuestionAnswerSend(
                    p_UserId,
                    v_Question
                );

                return new UserScore
                {
                    User = v_ExistingScore.User,
                    Score = v_TotalScore
                };
            }

            return null;
        });

        // Update all scores in parallel, get results and order it
        UserScore[] v_AllScores = await Task.WhenAll(v_UpdateScoreTasks);
        List<UserScore> v_ScoresList = v_AllScores
            .Where(p_S => p_S != null)
            .OrderByDescending(p_S => p_S.Score)
            .ToList();

        // Put time to let users see the correct answer
        await Task.Delay(TimeSpan.FromSeconds(5), p_CancellationToken);
        
        // Send score each round if its configured
        if (v_Party.ScoreEachRound)
        {
            await p_GroupNotificationService.NotifyPartyScoresAsync(
                v_Party.Id,
                v_ScoresList
            );
            
            await Task.Delay(TimeSpan.FromSeconds(5), p_CancellationToken);
        }
        
        // If the party is finished or not
        if (v_Party.NbQuestions != v_CurrentIndex + 1)
        {
            // Start the next question
            await p_CacheService.SetAsync(
                RedisKeys.Party.CurrentQuestionIndex(v_Party.Id),
                v_CurrentIndex + 1, p_CancellationToken: p_CancellationToken);

            // Loop to others questions
            await p_Mediator.Send(p_PartyRequest, p_CancellationToken);
        }
        else
        {
            await p_GroupNotificationService.NotifyPartyScoresAsync(
                v_Party.Id,
                v_ScoresList
            );
            
            await Task.Delay(TimeSpan.FromSeconds(5), p_CancellationToken);
            
            // TODO : Enregistrer tout dans la base de données
            // TODO : Reset la partie dans REDIS, supprimer toutes les clés qui ont été crées après
        }

        return new EmptyResponse();
    }
}
