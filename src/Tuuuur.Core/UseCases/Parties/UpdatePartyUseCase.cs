using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Services;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Parties;

internal class UpdatePartyUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<UpdatePartyUseCase> p_Logger, 
    ICalculService p_CalculService,
    IUserRoleService p_UserRoleService)
    : ADbUseCase<UpdatePartyStateRequest, GenericEntityResponse<Party>>(p_Logger,  p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<Party>> HandleLogic(UpdatePartyStateRequest p_Request, CancellationToken p_CancellationToken)
    {
            DateTime v_CurrentDateTime = DateTime.UtcNow;
            string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
            User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
            
            if(v_User == null)
                return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);

            Party v_Party = await m_UnitOfWork.PartyRepository.GetByIdAsync(p_Request.PartyId, v_User.Id, p_CancellationToken);
            if (v_Party.Finish)
            {
                throw new InvalidOperationException();
            }
            
            PartyQuestion v_PartyQuestion = v_Party.PartyQuestions.FirstOrDefault(p_P => p_P.UserPartyQuestion?.Correct is null);
            if (v_PartyQuestion is null)
                throw new InvalidOperationException();
            
            Answer v_Answer = v_PartyQuestion.Question.Answer.FirstOrDefault(p_P => p_P.Id == p_Request.AnswerId);
            if (v_Answer is null)
                return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Answer)} was not found, Key: {p_Request.AnswerId.ToString()}")]);
            

            if (v_PartyQuestion.UserPartyQuestion is null)
            {
                return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(UserPartyQuestion)} was not found, Key: {p_Request.AnswerId.ToString()}")]);
            }
            UserPartyQuestion v_UserPartyQuestion = v_PartyQuestion.UserPartyQuestion;
            if(v_UserPartyQuestion is null || v_UserPartyQuestion.IdAnswer is not null)
                return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(UserPartyQuestion)} was not found, Key: {v_PartyQuestion.Id.ToString()}")]);

            v_UserPartyQuestion.IdAnswer = v_Answer.Id;
            v_UserPartyQuestion.Correct = v_Answer.Valid;
            v_UserPartyQuestion.DtAnsweredAt = v_CurrentDateTime;
            if (v_Answer.Valid.HasValue && v_Answer.Valid.Value)
            {
                v_UserPartyQuestion.Score = p_CalculService.CalculateScore(v_UserPartyQuestion.DtPresentedAt, v_UserPartyQuestion.DtAnsweredAt);
            }
            else
            {
                v_UserPartyQuestion.Score = 0;
            }

            if (v_PartyQuestion.Order == v_Party.PartyQuestions.Count)
            {
                v_Party.Finish = true;
                await m_UnitOfWork.PartyRepository.UpdateAsync(v_Party);
            }
            
            await m_UnitOfWork.UserPartyQuestionRepository.UpdateAsync(v_UserPartyQuestion);
            _ = m_UnitOfWork.Save();
            
            v_Party = await m_UnitOfWork.PartyRepository.GetByIdAsync(p_Request.PartyId, v_User.Id, p_CancellationToken);
            v_Party.NbQuestions = v_Party.PartyQuestions.Count;
            v_Party.PartyQuestions = v_Party.PartyQuestions.Where(p_P => p_P.UserPartyQuestion is not null).ToList();
            
            return new GenericEntityResponse<Party>(v_Party);
    }
}