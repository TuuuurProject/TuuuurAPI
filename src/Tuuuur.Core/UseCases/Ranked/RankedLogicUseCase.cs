using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Configuration;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Services;
using Tuuuur.Domain.Notifications;

namespace Tuuuur.Core.UseCases.Ranked;

internal class RankedLogicUseCase(
    IUnitOfWork p_UnitOfWork,
    ILogger<RankedLogicUseCase> p_Logger,
    IRankedNotificationService p_NotificationService,
    ICalculService p_CalculService,
    IEloService p_EloService,
    ICacheService p_CacheService,
    IMediator p_Mediator,
    RankedConfiguration p_RankedConfiguration)
    : ADbUseCase<RankedLogicRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(RankedLogicRequest p_Request, CancellationToken p_CancellationToken)
    {
        Party v_Party = await p_CacheService.GetAsync<Party>(RedisKeys.Ranked.ById(p_Request.PartyId), p_CancellationToken);

        // Get the index of question to get
        int v_CurrentIndex = await p_CacheService.GetAsync<int>(RedisKeys.Ranked.CurrentQuestionIndex(v_Party.Id), p_CancellationToken);

        User v_Player1 = await m_UnitOfWork.UserRepository.GetUserByIdAsync(v_Party.PartyUsers.FirstOrDefault()!.IdUser, p_CancellationToken);
        if (v_Player1 is null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_Party.PartyUsers.FirstOrDefault()!.IdUser}")]);

        User v_Player2 = await m_UnitOfWork.UserRepository.GetUserByIdAsync(v_Party.PartyUsers.LastOrDefault()!.IdUser, p_CancellationToken);
        if (v_Player2 is null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_Party.PartyUsers.LastOrDefault()!.IdUser}")]);

        List<(Question Value, int Score)> v_ExistingQuestions =
            await p_CacheService.SortedSetGetAllWithScoresAsync<Question>(RedisKeys.Ranked.Questions(v_Party.Id), p_CancellationToken: p_CancellationToken);

        // Get random question excluding question already send
        Question v_CurrentQuestion = await m_UnitOfWork.QuestionRepository.GetRandomQuestionExcludingAsync(
            v_ExistingQuestions.Select(p_P => p_P.Value.Id).ToList(),
            p_CancellationToken);

        _ = await p_CacheService.SortedSetAddAsync(RedisKeys.Ranked.Questions(v_Party.Id), v_CurrentQuestion, p_Score: v_CurrentIndex, p_CancellationToken: p_CancellationToken);

        v_CurrentQuestion.ClearAnswer();

        // Send countdown (3, 2, 1)
        for (int v_Seconds = 3; v_Seconds > 0; v_Seconds--)
        {
            await p_NotificationService.NotifyCountdownAsync(
                v_Party.Id,
                v_Seconds
            );
            await Task.Delay(1000, p_CancellationToken);
        }

        double v_DamageMultiplier = GetDamageMultiplier(v_CurrentIndex);
        Question v_QuestionCopied = v_CurrentQuestion;
        IEnumerable<Task> v_UpdateQuestionTasks = v_Party.PartyUsers.Select(p_P => p_P.User).Select(async p_User =>
        {
            Question v_LocalQuestion = v_QuestionCopied.Copy();
            UserPartyQuestion v_UserPartyQuestion = new()
            {
                IdUser = p_User.Id,
                Correct = null,
                DtPresentedAt = DateTime.Now,
                AnswersOrder = Guid.NewGuid()
            };

            await p_CacheService.SetAsync(
                RedisKeys.Ranked.QuestionUserAnswer(v_Party.Id, v_LocalQuestion.Id, p_User.Id),
                v_UserPartyQuestion,
                p_CancellationToken: p_CancellationToken
            );

            // Create a shuffled copy of answers for this user
            int v_Seed = v_UserPartyQuestion.AnswersOrder.GetHashCode();
            Random v_Random = new(v_Seed);
            v_LocalQuestion.Answer = v_LocalQuestion.Answer.OrderBy(_ => v_Random.Next()).ToList();

            RankedQuestion v_GroupQuestion = new()
            {
                Question = v_LocalQuestion,
                CurrentIndex = v_CurrentIndex,
                Multiplier = v_DamageMultiplier
            };

            // Send the question to the specific user
            await p_NotificationService.NotifyPartyQuestionSend(
                p_User.Id,
                v_GroupQuestion
            );
        });

        // Send question for all users in the same time
        await Task.WhenAll(v_UpdateQuestionTasks);

        // Wait for players to respond with instant Pub/Sub notification
        await WaitForAllPlayersOrTimeoutAsync(v_Party.Id, v_CurrentQuestion.Id, p_CancellationToken);

        UserPartyQuestion v_Upq1 = await p_CacheService.GetAsync<UserPartyQuestion>(
            RedisKeys.Ranked.QuestionUserAnswer(v_Party.Id, v_CurrentQuestion.Id, v_Player1.Id), p_CancellationToken);
        UserPartyQuestion v_Upq2 = await p_CacheService.GetAsync<UserPartyQuestion>(
            RedisKeys.Ranked.QuestionUserAnswer(v_Party.Id, v_CurrentQuestion.Id, v_Player2.Id), p_CancellationToken);

        // Determine correct/incorrect
        v_CurrentQuestion = await m_UnitOfWork.QuestionRepository.GetQuestionByIdWithAnswerAsync(v_CurrentQuestion.Id, p_CancellationToken);

        int v_RawScore1 = ComputeRawScore(v_Upq1, v_CurrentQuestion, p_CalculService);
        int v_RawScore2 = ComputeRawScore(v_Upq2, v_CurrentQuestion, p_CalculService);

        bool v_P1Correct = v_RawScore1 > 0;
        bool v_P2Correct = v_RawScore2 > 0;

        if (v_Upq1 != null) v_Upq1.Correct = v_P1Correct;
        if (v_Upq2 != null) v_Upq2.Correct = v_P2Correct;

        // Delta rule
        // - One correct, one wrong → winner keeps their score (delta = 0), loser loses winner's score × multiplier
        // - Both correct or both wrong → no change (delta = 0)

        int v_Delta1 = (!v_P1Correct && v_P2Correct)
            ? -(int)(v_RawScore2 * v_DamageMultiplier) // loser loses winner's score
            : 0;                                       // winner or draw: no change

        int v_Delta2 = (!v_P2Correct && v_P1Correct)
            ? -(int)(v_RawScore1 * v_DamageMultiplier) // loser loses winner's score
            : 0;                                       // winner or draw: no change

        // Reshuffle answers copies
        Question v_Q1 = v_CurrentQuestion.Copy();
        Question v_Q2 = v_CurrentQuestion.Copy();

        if (v_Upq1 != null)
        {
            Random v_Rnd1 = new(v_Upq1.AnswersOrder.GetHashCode());
            v_Q1.Answer = v_Q1.Answer.OrderBy(_ => v_Rnd1.Next()).ToList();
        }
        if (v_Upq2 != null)
        {
            Random v_Rnd2 = new(v_Upq2.AnswersOrder.GetHashCode());
            v_Q2.Answer = v_Q2.Answer.OrderBy(_ => v_Rnd2.Next()).ToList();
        }

        // Send answers to users
        await Task.WhenAll(
            p_NotificationService.NotifyQuestionAnswerSend(v_Player1.Id, new RankedQuestion
            {
                Question = v_Q1,
                CurrentIndex = v_CurrentIndex,
                Score = v_Delta1,
                Multiplier = v_DamageMultiplier
            }),
            p_NotificationService.NotifyQuestionAnswerSend(v_Player2.Id, new RankedQuestion
            {
                Question = v_Q2,
                CurrentIndex = v_CurrentIndex,
                Score = v_Delta2,
                Multiplier = v_DamageMultiplier
            })
        );

        List<UserAnswered> v_UserAnswers =
        [
            new() { Correct = v_P1Correct, User = v_Player1 },
            new() { Correct = v_P2Correct, User = v_Player2 },
        ];

        await p_NotificationService.NotifyAllPlayerAnswered(v_Party.Id, v_UserAnswers);

        // Put time to let users see the correct answer
        await Task.Delay(TimeSpan.FromSeconds(5), p_CancellationToken);

        List<(User User, int Score)> v_CurrentScores = await p_CacheService.SortedSetGetAllWithScoresAsync<User>(
            RedisKeys.Ranked.Scores(v_Party.Id),
            p_Descending: true,
            p_CancellationToken: p_CancellationToken
        );

        // Update scores
        (User User, int Score) v_Existing1 = v_CurrentScores.FirstOrDefault(p_S => p_S.User.Id == v_Player1.Id);
        (User User, int Score) v_Existing2 = v_CurrentScores.FirstOrDefault(p_S => p_S.User.Id == v_Player2.Id);

        int v_Total1 = Math.Max(0, v_Existing1.Score + v_Delta1);
        int v_Total2 = Math.Max(0, v_Existing2.Score + v_Delta2);

        if (v_Upq1 != null) v_Upq1.Score = v_RawScore1;
        if (v_Upq2 != null) v_Upq2.Score = v_RawScore2;

        // ── 8. Persist UPQs + update sorted-set scores — all in parallel ──────────
        await Task.WhenAll(
            // Player 1
            v_Upq1 != null
                ? p_CacheService.SetAsync(RedisKeys.Ranked.QuestionUserAnswer(v_Party.Id, v_CurrentQuestion.Id, v_Player1.Id), v_Upq1, p_CancellationToken: p_CancellationToken)
                : Task.CompletedTask,
            v_Existing1.User != null
                ? p_CacheService.SortedSetAddAsync(RedisKeys.Ranked.Scores(v_Party.Id), v_Existing1.User, v_Total1, p_CancellationToken: p_CancellationToken)
                : Task.FromResult(false),
            // Player 2
            v_Upq2 != null
                ? p_CacheService.SetAsync(RedisKeys.Ranked.QuestionUserAnswer(v_Party.Id, v_CurrentQuestion.Id, v_Player2.Id), v_Upq2, p_CancellationToken: p_CancellationToken)
                : Task.CompletedTask,
            v_Existing2.User != null
                ? p_CacheService.SortedSetAddAsync(RedisKeys.Ranked.Scores(v_Party.Id), v_Existing2.User, v_Total2, p_CancellationToken: p_CancellationToken)
                : Task.FromResult(false)
        );

        // Send updated scores
        List<UserScore> v_ScoresList =
        [
            new() { User = v_Player1, Score = v_Total1 },
            new() { User = v_Player2, Score = v_Total2 },
        ];

        await p_NotificationService.NotifyPartyScoresAsync(
            v_Party.Id,
            v_ScoresList.OrderByDescending(p_S => p_S.User.Id).ToList()
        );

        await Task.Delay(TimeSpan.FromSeconds(2), p_CancellationToken);

        // If the party is not finished (no player has reached 0 or below)
        if (v_ScoresList.All(p_P => p_P.Score > 0))
        {
            // Start the next question
            await p_CacheService.SetAsync(
                RedisKeys.Ranked.CurrentQuestionIndex(v_Party.Id),
                v_CurrentIndex + 1, p_CancellationToken: p_CancellationToken);

            // Loop to others questions
            await p_Mediator.Send(p_Request, p_CancellationToken);
        }
        else
        {
            User v_Winner = v_ScoresList.FirstOrDefault(p_P => p_P.Score > 0)?.User;
            User v_Looser = v_ScoresList.FirstOrDefault(p_P => p_P.Score <= 0)?.User;

            // ── Elo update per theme
            if (v_Winner != null && v_Looser != null)
            {
                IEnumerable<Theme> v_Themes = await m_UnitOfWork.ThemeRepository.GetAllThemesAsync(p_CancellationToken);
                int v_TotalWinnerDelta = 0;
                int v_TotalLoserDelta = 0;
                IEnumerable<Theme> v_ThemesArray = v_Themes as Theme[] ?? v_Themes.ToArray();
                foreach (Theme v_Theme in v_ThemesArray)
                {
                    int v_WinnerCurrentElo = v_Winner.Elo.FirstOrDefault(p_E => p_E.IdTheme == v_Theme.Id)?.Value ?? 1000; // 1_000 is defaut elo value
                    int v_LoserCurrentElo = v_Looser.Elo.FirstOrDefault(p_E => p_E.IdTheme == v_Theme.Id)?.Value ?? 1000; // 1_000 is defaut elo value

                    (int v_WinnerDelta, int v_LoserDelta) = p_EloService.ComputeEloDelta(v_WinnerCurrentElo, v_LoserCurrentElo);

                    await m_UnitOfWork.EloRepository.UpdateValueAsync(v_Winner.Id, v_Theme.Id, v_WinnerCurrentElo + v_WinnerDelta, p_CancellationToken);
                    await m_UnitOfWork.EloRepository.UpdateValueAsync(v_Looser.Id, v_Theme.Id, v_LoserCurrentElo - v_LoserDelta, p_CancellationToken);
                    m_UnitOfWork.Save();

                    v_TotalWinnerDelta += v_WinnerDelta;
                    v_TotalLoserDelta += v_LoserDelta;
                }

                int v_EloDeltaWinner = v_ThemesArray.Any() ? v_TotalWinnerDelta / v_ThemesArray.Count() : 0;
                int v_EloDeltaLoser = v_ThemesArray.Any() ? v_TotalLoserDelta / v_ThemesArray.Count() : 0;

                await p_NotificationService.NotifyUserWinAsync(v_Winner.Id, +v_EloDeltaWinner);
                await p_NotificationService.NotifyUserLooseAsync(v_Looser.Id, -v_EloDeltaLoser);
            }

            v_Party.InProgress = false;
            await p_CacheService.SetAsync(RedisKeys.Ranked.ById(v_Party.Id), v_Party, p_CancellationToken: p_CancellationToken);

            // Sauvegarder l'ID avant de le réinitialiser
            List<Question> v_Questions = await p_CacheService.SortedSetRangeByRankAsync<Question>(RedisKeys.Ranked.Questions(v_Party.Id), p_CancellationToken: p_CancellationToken);

            v_Party.Id = Guid.Empty;
            v_Party.Finish = true;

            v_Party.PartyQuestions.AddRange(v_Questions.Select(p_P => new PartyQuestion { IdQuestion = p_P.Id }));

            // Remove user to avoid duplicated added users
            v_Party.PartyUsers.ForEach(p_P => p_P.User = null);

            IMappingAddEntity<PartyBase, IEntity> v_MappingAddEntity = await m_UnitOfWork.PartyRepository.CreatePartyAsync(v_Party, p_CancellationToken);
            m_UnitOfWork.Save();

            try
            {
                foreach (PartyQuestion v_PartyQuestion in v_MappingAddEntity.MapBoEntity.PartyQuestions)
                {
                    foreach (User v_User in new[] { v_Player1, v_Player2 })
                    {
                        UserPartyQuestion v_UserPartyQuestion = await p_CacheService.GetAsync<UserPartyQuestion>(
                            RedisKeys.Ranked.QuestionUserAnswer(v_Party.Id, v_PartyQuestion.IdQuestion, v_User.Id), p_CancellationToken);
                        if (v_UserPartyQuestion != null)
                        {
                            v_UserPartyQuestion.IdPartyQuestion = v_PartyQuestion.Id;
                            v_UserPartyQuestion.IdUser = v_User.Id;
                            v_UserPartyQuestion.IdGuest = null;
                            v_UserPartyQuestion.GuestNickname = null;
                            _ = await m_UnitOfWork.UserPartyQuestionRepository.CreateUserPartyQuestionAsync(v_UserPartyQuestion, p_CancellationToken);
                            m_UnitOfWork.Save();
                        }
                    }
                }
            }
            finally
            {
                await p_CacheService.RemoveByPatternAsync(RedisKeys.Ranked.ById(v_Party.Id) + ":*", p_CancellationToken: p_CancellationToken);
            }
        }

        return new EmptyResponse();
    }

    /// <summary>
    /// Returns the damage multiplier for the given (0-based) round index.
    /// Rounds 1 to <see cref="RankedConfiguration.ThresholdRound"/> use a multiplier of 1.
    /// Each subsequent round increases the multiplier by <see cref="RankedConfiguration.MultiplierIncrement"/>.
    /// </summary>
    private double GetDamageMultiplier(int p_RoundIndex)
    {
        int v_CurrentRound = p_RoundIndex + 1; // convert to 1-based
        if (v_CurrentRound <= p_RankedConfiguration.ThresholdRound)
            return 1.0;

        return 1.0 + (v_CurrentRound - p_RankedConfiguration.ThresholdRound) * p_RankedConfiguration.MultiplierIncrement;
    }

    /// <summary>
    /// Computes a player's raw score for the current question:
    /// returns the time-based score if they answered correctly, 0 otherwise.
    /// </summary>
    private static int ComputeRawScore(UserPartyQuestion p_Upq, Question p_Question, ICalculService p_CalculService)
    {
        if (p_Upq is null) return 0;

        Answer v_Answer = p_Question.Answer.FirstOrDefault(p_A => p_A.Id == p_Upq.IdAnswer);
        if (v_Answer is null)
        {
            p_Upq.IdAnswer = null;
            return 0;
        }

        int v_Score = p_CalculService.CalculateScore(p_Upq.DtPresentedAt, p_Upq.DtAnsweredAt);
        return v_Answer.Valid == true && v_Score > 0 ? v_Score : 0;
    }

    /// <summary>
    /// Waits for all players to answer or until timeout (15 seconds).
    /// Uses Redis Pub/Sub for instant notification when all players answer - zero polling overhead.
    /// Scalable to thousands of simultaneous groups.
    /// </summary>
    private async Task WaitForAllPlayersOrTimeoutAsync(
        Guid p_PartyId,
        int p_QuestionId,
        CancellationToken p_CancellationToken)
    {
        string v_Channel = RedisKeys.Ranked.PartyQuestionAllAnsweredChannel(p_PartyId, p_QuestionId);

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
            await p_CacheService.RemoveAsync(RedisKeys.Ranked.PartyQuestionAnswered(p_PartyId, p_QuestionId), p_CancellationToken);
        }
    }
}
