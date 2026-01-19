using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Requests.Parties;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Services;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Parties
{
    internal class UpdatePartyUseCase(
        IUnitOfWork p_UnitOfWork,
        ILogger<UpdatePartyUseCase> p_Logger,
        ICalculService p_CalculService,
        IUserRoleService p_UserRoleService)
        : ADbUseCase<UpdateSoloPartyStateRequest, GenericEntityResponse<PartyBase>>(p_Logger, p_UnitOfWork)
    {
        protected override async Task<GenericEntityResponse<PartyBase>> HandleLogic(UpdateSoloPartyStateRequest p_Request,
            CancellationToken p_CancellationToken)
        {
            DateTime v_CurrentDateTime = DateTime.UtcNow;
            string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
            User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);

            if (v_User == null)
            {
                return new GenericEntityResponse<PartyBase>([
                    new ErrorDto(DomainErrors.Data.NotFound,
                        $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")
                ]);
            }

            PartyBase v_Party = await m_UnitOfWork.PartyRepository.GetByIdAsync(p_Request.PartyId, v_User.Id, p_CancellationToken);
            if (v_Party.Finish)
                return new GenericEntityResponse<PartyBase>([
                    new ErrorDto(DomainErrors.Party.Finished, "Party is finished")
                ]);
            
            Question v_Question = v_Party.Questions.OrderBy(p_P => p_P.Index).FirstOrDefault(p_P => p_P.DtPresentedAt.HasValue && !p_P.DtAnsweredAt.HasValue);

            // Answer can be null if the request was send without response
            Answer v_Answer = v_Question.Answer.FirstOrDefault(p_P => p_P.Id == p_Request.AnswerId);

            v_Question.DtAnsweredAt = v_CurrentDateTime;
            if (v_Question.DtPresentedAt != null)
            {
                int v_Score = p_CalculService.CalculateScore(v_Question.DtPresentedAt.Value, v_Question.DtAnsweredAt);

                if (v_Answer is null)
                {
                    v_Question.UserAnswer = null;
                    v_Question.Correct = false;
                    v_Question.Score = 0;
                }
                else
                {
                    v_Question.UserAnswer = v_Answer.Id;
                    if (v_Answer.Valid.HasValue && v_Answer.Valid.Value && v_Score > 0)
                    {
                        v_Question.Correct = true;
                        v_Question.Score = v_Score;
                    }
                    else
                    {
                        v_Question.Score = 0;
                        v_Question.Correct = false;
                    }
                }
            }

            // If we are in the last question, mark the party as finish
            if (v_Question.Index == v_Party.NbQuestions)
            {
                v_Party.Finish = true;
                await m_UnitOfWork.PartyRepository.UpdateAsync(v_Party);
            }

            // TODO
            /*
            await m_UnitOfWork.UserPartyQuestionRepository.UpdateAsync(v_UserPartyQuestion);
            _ = m_UnitOfWork.Save();
            */
            v_Party = await m_UnitOfWork.PartyRepository.GetByIdAsync(p_Request.PartyId, v_User.Id,
                p_CancellationToken);
            v_Party.Questions = v_Party.Questions.Where(p_P => p_P.DtAnsweredAt.HasValue).ToList();
            
            foreach (Question v_LocalQuestion in v_Party.Questions)
            {
                int v_Seed = v_Question.AnswerSeed.GetHashCode();

                Random v_Random = new(v_Seed);
                
                v_Question.Answer = v_Question.Answer.OrderBy(_ => v_Random.Next()).ToList();
            }
            
            return new GenericEntityResponse<PartyBase>(v_Party);
        }
    }
}