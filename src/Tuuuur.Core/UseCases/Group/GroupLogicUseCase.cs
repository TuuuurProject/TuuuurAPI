using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Services;
using Tuuuur.Domain.Notifications;

namespace Tuuuur.Core.UseCases.Group;

internal class GroupLogicUseCase(
    IUnitOfWork p_UnitOfWork,
    ICacheService p_CacheService,
    IGroupNotificationService p_GroupNotificationService,
    ICalculService p_CalculService,
    IMediator p_Mediator,
    ILogger<GroupLogicUseCase> p_Logger)
    : ADbUseCase<GroupLogicRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(GroupLogicRequest p_PartyLogicRequest, CancellationToken p_CancellationToken)
    {
        GroupParty v_Party = await p_CacheService.GetAsync<GroupParty>(RedisKeys.Party.ByCode(p_PartyLogicRequest.PartyCode), p_CancellationToken);

        // Get the index of question to get
        int v_CurrentIndex = await p_CacheService.GetAsync<int>(RedisKeys.Party.CurrentQuestionIndex(v_Party.Code), p_CancellationToken);

        // Get the question
        Question v_CurrentQuestion = await p_CacheService.SortedSetGetByIndexAsync<Question>(
            RedisKeys.Party.Questions(v_Party.Code),
            p_Index: v_CurrentIndex, p_CancellationToken: p_CancellationToken);

        Question v_Question = await m_UnitOfWork.QuestionRepository.GetQuestionByIdWithAnswerAsync(v_CurrentQuestion.Id, p_CancellationToken);
        v_Question.ClearAnswer();

        // Send countdown (3, 2, 1)
        for (int v_I = 3; v_I > 0; v_I--)
        {
            await p_GroupNotificationService.NotifyCountdownAsync(
                v_Party.Code,
                v_I
            );
            await Task.Delay(1000, p_CancellationToken);
        }

        List<int> v_UserIds = await p_CacheService.SetMembersAsync<int>(
            RedisKeys.Party.Users(v_Party.Code),
            CancellationToken.None
        );

        // Initialize the answered set for this question
        string v_AnsweredSetKey = RedisKeys.Party.PartyQuestionAnswered(v_Party.Code, v_CurrentQuestion.Id);
        await p_CacheService.RemoveAsync(v_AnsweredSetKey, p_CancellationToken);

        // Update question state for each user in parallel
        Question v_QuestionCopied = v_Question;
        IEnumerable<Task> v_UpdateQuestionTasks = v_UserIds.Select(async p_UserId =>
        {
            Question v_LocalQuestion = v_QuestionCopied.Copy();
            UserPartyQuestion v_UserPartyQuestion = new()
            {
                IdUser = p_UserId,
                Correct = null,
                DtPresentedAt = DateTime.Now,
                AnswersOrder = Guid.NewGuid()
            };

            await p_CacheService.SetAsync(
                RedisKeys.Party.PartyQuestionUserAnswer(v_Party.Code, v_LocalQuestion.Id, p_UserId),
                v_UserPartyQuestion,
                p_CancellationToken: p_CancellationToken
            );

            // Create a shuffled copy of answers for this user
            int v_Seed = v_UserPartyQuestion.AnswersOrder.GetHashCode();
            Random v_Random = new(v_Seed);
            v_LocalQuestion.Answer = v_LocalQuestion.Answer.OrderBy(_ => v_Random.Next()).ToList();

            GroupQuestion v_GroupQuestion = new()
            {
                Question = v_LocalQuestion,
                CurrentIndex = v_CurrentIndex,
            };

            // Send the question to the specific user
            await p_GroupNotificationService.NotifyPartyQuestionSend(
                p_UserId,
                v_GroupQuestion
            );
        });

        // Send question for all users in the same time
        await Task.WhenAll(v_UpdateQuestionTasks);

        // Wait for players to respond with instant Pub/Sub notification
        await WaitForAllPlayersOrTimeoutAsync(v_Party.Code, v_CurrentQuestion.Id, p_CancellationToken);

        // Fetch current scores from Redis
        List<(User User, int Score)> v_CurrentScores = await p_CacheService.SortedSetGetAllWithScoresAsync<User>(
            RedisKeys.Party.Scores(v_Party.Code),
            p_Descending: true,
            p_CancellationToken: p_CancellationToken
        );

        v_Question = await m_UnitOfWork.QuestionRepository.GetQuestionByIdWithAnswerAsync(v_CurrentQuestion.Id, p_CancellationToken);

        List<UserAnswered> v_UserAnswereds = [];

        // Update scores for all users in parallel
        IEnumerable<Task<UserScore>> v_UpdateScoreTasks = v_UserIds.Select(async p_UserId =>
        {
            Question v_LocalQuestion = v_Question.Copy();
            UserPartyQuestion v_UserPartyQuestion = await p_CacheService.GetAsync<UserPartyQuestion>(
                RedisKeys.Party.PartyQuestionUserAnswer(v_Party.Code, v_LocalQuestion.Id, p_UserId),
                p_CancellationToken: p_CancellationToken
            );

            if (v_UserPartyQuestion != null)
            {
                // Answer can be null if the request was sent without response
                Answer v_Answer = v_LocalQuestion.Answer.FirstOrDefault(p_P => p_P.Id == v_UserPartyQuestion.IdAnswer);

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
                await p_CacheService.SetAsync(RedisKeys.Party.PartyQuestionUserAnswer(v_Party.Code, v_Question.Id, p_UserId), v_UserPartyQuestion, p_CancellationToken: p_CancellationToken);

                // Update score directly in Redis
                if (v_ExistingScore.User != null)
                {
                    await p_CacheService.SortedSetAddAsync(
                        RedisKeys.Party.Scores(v_Party.Code),
                        v_ExistingScore.User,
                        v_TotalScore,
                        TimeSpan.Zero,
                        p_CancellationToken
                    );
                }

                int v_Seed = v_UserPartyQuestion.AnswersOrder.GetHashCode();
                Random v_Random = new(v_Seed);
                v_LocalQuestion.Answer = v_LocalQuestion.Answer.OrderBy(_ => v_Random.Next()).ToList();

                GroupQuestion v_GroupQuestion = new()
                {
                    Question = v_LocalQuestion,
                    CurrentIndex = v_CurrentIndex,
                    Score = v_UserPartyQuestion.Score,
                };

                // Send the question with correct answer after countdown
                await p_GroupNotificationService.NotifyPartyQuestionAnswerSend(
                    p_UserId,
                    v_GroupQuestion
                );
                
                v_UserAnswereds.Add(new UserAnswered(){ Correct = v_UserPartyQuestion.Correct ?? false , User = v_ExistingScore.User });

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
        
        await p_GroupNotificationService.NotifyAllPlayerAnswered(v_Party.Code, v_UserAnswereds);
        
        List<UserScore> v_ScoresList = v_AllScores
            .Where(p_S => p_S != null)
            .OrderByDescending(p_S => p_S.Score)
            .ToList();
        
        // Put time to let users see the correct answer
        await Task.Delay(TimeSpan.FromSeconds(5), p_CancellationToken);

        // Send score each round if its configured
        if (v_Party.ScoreEachRound && v_Party.NbQuestions != v_CurrentIndex + 1)
        {
            await p_GroupNotificationService.NotifyPartyScoresAsync(
                v_Party.Code,
                v_ScoresList
            );

            await Task.Delay(TimeSpan.FromSeconds(5), p_CancellationToken);
        }

        // If the party is finished or not
        if (v_Party.NbQuestions != v_CurrentIndex + 1)
        {
            // Start the next question
            await p_CacheService.SetAsync(
                RedisKeys.Party.CurrentQuestionIndex(v_Party.Code),
                v_CurrentIndex + 1, p_CancellationToken: p_CancellationToken);

            // Loop to others questions
            await p_Mediator.Send(p_PartyLogicRequest, p_CancellationToken);
        }
        else
        {
            await p_GroupNotificationService.NotifyPartyFinishedAsync(
                v_Party.Code,
                v_ScoresList
            );

            v_Party.InProgress = false;
            await p_CacheService.SetAsync(RedisKeys.Party.ByCode(v_Party.Code), v_Party, p_CancellationToken: p_CancellationToken);
            
            // Sauvegarder l'ID avant de le réinitialiser
            List<Question> v_Questions = await p_CacheService.SortedSetRangeByRankAsync<Question>(RedisKeys.Party.Questions(v_Party.Code), p_CancellationToken: p_CancellationToken);

            v_Party.Id = Guid.Empty;
            v_Party.Finish = true;

            v_Party.PartyQuestions.AddRange(v_Questions.Select(p_P => new PartyQuestion { IdQuestion = p_P.Id }));

            IMappingAddEntity<PartyBase, IEntity> v_MappingAddEntity = await m_UnitOfWork.PartyRepository.CreatePartyAsync(v_Party, p_CancellationToken);
            m_UnitOfWork.Save();

            try
            {
                foreach (PartyQuestion v_PartyQuestion in v_MappingAddEntity.MapBoEntity.PartyQuestions)
                {
                    foreach (int v_UserId in v_UserIds)
                    {
                        UserPartyQuestion v_UserPartyQuestion = await p_CacheService.GetAsync<UserPartyQuestion>(
                            RedisKeys.Party.PartyQuestionUserAnswer(v_Party.Code, v_PartyQuestion.IdQuestion, v_UserId), p_CancellationToken);
                        v_UserPartyQuestion.IdPartyQuestion = v_PartyQuestion.Id;

                        _ = await m_UnitOfWork.UserPartyQuestionRepository.CreateUserPartyQuestionAsync(v_UserPartyQuestion, p_CancellationToken);
                        m_UnitOfWork.Save();
                    }
                }
            }
            finally
            {
                await p_CacheService.RemoveByPatternAsync(RedisKeys.Party.ByCode(v_Party.Code) + ":*", [RedisKeys.Party.Users(v_Party.Code)], p_CancellationToken);
            }
        }

        return new EmptyResponse();
    }

    /// <summary>
    /// Waits for all players to answer or until timeout (15 seconds).
    /// Uses Redis Pub/Sub for instant notification when all players answer - zero polling overhead.
    /// Scalable to thousands of simultaneous groups.
    /// </summary>
    private async Task WaitForAllPlayersOrTimeoutAsync(
        string p_PartyCode,
        int p_QuestionId,
        CancellationToken p_CancellationToken)
    {
        string v_Channel = RedisKeys.Party.PartyQuestionAllAnsweredChannel(p_PartyCode, p_QuestionId);

        try
        {
            // Subscribe and wait for Pub/Sub notification or timeout (15s)
            // This is instant when all players answer - no polling!
            await p_CacheService.SubscribeAndWaitAsync<bool>(v_Channel, TimeSpan.FromSeconds(15), p_CancellationToken);
        }
        catch (TaskCanceledException)
        {
            // Timeout reached - continue with scoring
        }
        finally
        {
            // Cleanup the answered set
            await p_CacheService.RemoveAsync(RedisKeys.Party.PartyQuestionAnswered(p_PartyCode, p_QuestionId), p_CancellationToken);
        }
    }
}
