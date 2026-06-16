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

internal class AnswerQuestionRankedUseCase(
    IUnitOfWork p_UnitOfWork,
    ICacheService p_CacheService,
    IRankedNotificationService p_NotificationService,
    RankedConfiguration p_RankedConfiguration,
    ILogger<AnswerQuestionRankedUseCase> p_Logger)
    : ADbUseCase<AnswerQuestionRankedRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(AnswerQuestionRankedRequest p_Request, CancellationToken p_CancellationToken)
    {
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
        
        Party v_Party = await p_CacheService.GetAsync<Party>(RedisKeys.Ranked.ById(v_PartyId), p_CancellationToken);

        // If user is not in the party and party is not in progress
        if (v_PartyId != v_Party.Id)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);

        // Get the index of question to get
        int v_CurrentIndex = await p_CacheService.GetAsync<int>(RedisKeys.Ranked.CurrentQuestionIndex(v_Party.Id), p_CancellationToken);

        // Get the question
        Question v_CurrentQuestion = await p_CacheService.SortedSetGetByIndexAsync<Question>(
            RedisKeys.Ranked.Questions(v_Party.Id),
            p_Index: v_CurrentIndex, p_CancellationToken: p_CancellationToken);

        Question v_Question = await m_UnitOfWork.QuestionRepository.GetQuestionByIdWithAnswerAsync(v_CurrentQuestion.Id, p_CancellationToken);

        if (v_Question.Answer.All(p_P => p_P.Id != p_Request.AnswerId))
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Answer)} was not found, Key: {p_Request.AnswerId}")]);
        }

        UserPartyQuestion v_UserPartyQuestion = await p_CacheService.GetAsync<UserPartyQuestion>(RedisKeys.Ranked.QuestionUserAnswer(v_Party.Id, v_Question.Id, p_Request.UserId), p_CancellationToken: p_CancellationToken);
        v_UserPartyQuestion.IdAnswer = p_Request.AnswerId;
        v_UserPartyQuestion.DtAnsweredAt = DateTime.Now;

        await p_CacheService.SetAsync(RedisKeys.Ranked.QuestionUserAnswer(v_Party.Id, v_Question.Id, p_Request.UserId), v_UserPartyQuestion, p_RankedConfiguration.PartyTtl, p_CancellationToken);

        // Add user to the answered set for early round termination
        await p_CacheService.SetAddAsync(RedisKeys.Ranked.PartyQuestionAnswered(v_Party.Id, v_Question.Id), p_Request.UserId, p_RankedConfiguration.PartyTtl, p_CancellationToken);

        // Check if all players have answered
        long v_AnsweredCount = await p_CacheService.SetLengthAsync(RedisKeys.Ranked.PartyQuestionAnswered(v_Party.Id, v_Question.Id), p_CancellationToken);
        long v_TotalPlayers = v_Party.PartyUsers.Count;

        if (v_AnsweredCount >= v_TotalPlayers)
        {
            // All players answered - publish instant notification via Pub/Sub
            await p_CacheService.PublishAsync(
                RedisKeys.Ranked.PartyQuestionAllAnsweredChannel(v_Party.Id, v_Question.Id),
                true,
                p_CancellationToken
            );
        }
        
        User v_CurrentUser = v_Party.PartyUsers.FirstOrDefault(p_P => p_P.IdUser == p_Request.UserId)?.User;

        // Send group that user answer the question
        await p_NotificationService.NotifyUserSendAnswerAsync(
            v_Party.Id,
            v_CurrentUser
        );

        return new EmptyResponse();
    }
}
