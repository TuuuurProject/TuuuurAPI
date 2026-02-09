using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Group;

internal class AnswerQuestionGroupUseCase(
    IUnitOfWork p_UnitOfWork,
    ICacheService p_CacheService,
    IGroupNotificationService p_GroupNotificationService,
    ILogger<AnswerQuestionGroupUseCase> p_Logger)
    : ADbUseCase<AnswerQuestionGroupPartyRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(AnswerQuestionGroupPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_Request.UserEmail;
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);

        if (v_User == null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);

        string v_PartieCode = await p_CacheService.GetAsync<string>(RedisKeys.User.UserParty(v_User.Id), p_CancellationToken);

        if (v_PartieCode is null)
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        }

        GroupParty v_Party = await p_CacheService.GetAsync<GroupParty>(RedisKeys.Party.ByCode(v_PartieCode), p_CancellationToken);

        // If user is not in the party and party is not in progress
        if (v_PartieCode != v_Party.Code || !v_Party.InProgress)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);

        // Get the index of question to get
        int v_CurrentIndex = await p_CacheService.GetAsync<int>(RedisKeys.Party.CurrentQuestionIndex(v_Party.Code), p_CancellationToken);

        // Get the question
        Question v_CurrentQuestion = await p_CacheService.SortedSetGetByIndexAsync<Question>(
            RedisKeys.Party.Questions(v_Party.Code),
            p_Index: v_CurrentIndex, p_CancellationToken: p_CancellationToken);

        Question v_Question = await m_UnitOfWork.QuestionRepository.GetQuestionByIdWithAnswerAsync(v_CurrentQuestion.Id, p_CancellationToken);

        if (v_Question.Answer.All(p_P => p_P.Id != p_Request.AnswerId))
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Answer)} was not found, Key: {p_Request.AnswerId}")]);
        }

        UserPartyQuestion v_UserPartyQuestion = await p_CacheService.GetAsync<UserPartyQuestion>(RedisKeys.Party.PartyQuestionUserAnswer(v_Party.Code, v_Question.Id, v_User.Id), p_CancellationToken: p_CancellationToken);
        v_UserPartyQuestion.IdAnswer = p_Request.AnswerId;
        v_UserPartyQuestion.DtAnsweredAt = DateTime.Now;

        await p_CacheService.SetAsync(RedisKeys.Party.PartyQuestionUserAnswer(v_Party.Code, v_Question.Id, v_User.Id), v_UserPartyQuestion, p_CancellationToken: p_CancellationToken);

        // Add user to the answered set for early round termination
        await p_CacheService.SetAddAsync(RedisKeys.Party.PartyQuestionAnswered(v_Party.Code, v_Question.Id), v_User.Id, p_CancellationToken: p_CancellationToken);

        // Check if all players have answered
        long v_AnsweredCount = await p_CacheService.SetLengthAsync(RedisKeys.Party.PartyQuestionAnswered(v_Party.Code, v_Question.Id), p_CancellationToken);
        long v_TotalPlayers = await p_CacheService.SetLengthAsync(RedisKeys.Party.Users(v_Party.Code), p_CancellationToken);

        if (v_AnsweredCount >= v_TotalPlayers)
        {
            // All players answered - publish instant notification via Pub/Sub
            await p_CacheService.PublishAsync(
                RedisKeys.Party.PartyQuestionAllAnsweredChannel(v_Party.Code, v_Question.Id),
                true,
                p_CancellationToken
            );
        }

        // Send group that user answer the question
        await p_GroupNotificationService.NotifyUserSendAnswerAsync(
            v_Party.Code,
            v_User
        );

        return new EmptyResponse();
    }
}
