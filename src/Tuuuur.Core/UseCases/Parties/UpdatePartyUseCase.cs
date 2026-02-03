using Microsoft.Extensions.Logging;
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

            if (v_Question is null)
                return new GenericEntityResponse<PartyBase>([
                    new ErrorDto(DomainErrors.Party.NoQuestionSent, "No questions have been sent")
                ]);
            
            // Get the user answer
            Answer v_Answer = v_Question.Answers.FirstOrDefault(p_P => p_P.Id == p_Request.AnswerId);
            
            if (v_Question.DtPresentedAt.HasValue)
            {
                int v_Score = p_CalculService.CalculateScore(v_Question.DtPresentedAt.Value, DateTime.UtcNow);

                if (v_Answer is null)
                {
                    v_Question.IdUserAnswer = null;
                    v_Question.Correct = false;
                    v_Question.Score = 0;
                }
                else
                {
                    v_Question.IdUserAnswer = v_Answer.Id;
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
            
            await m_UnitOfWork.UserPartyQuestionRepository.UpdateUserQuestionAsync(v_Party.Id, v_User.Id, v_Question.Id, v_Question.Correct, v_Answer?.Id, v_Question.Score, p_CancellationToken);
            
            // If we are in the last question, mark the party as finish
            if (v_Question.Index == v_Party.NbQuestions)
            {
                await m_UnitOfWork.PartyRepository.FinishPartyAsync(v_Party, p_CancellationToken);
            }
            
            _ = m_UnitOfWork.Save();
            
            v_Party = await m_UnitOfWork.PartyRepository.GetByIdAsync(p_Request.PartyId, v_User.Id,
                p_CancellationToken);
            v_Party.Questions = v_Party.Questions.Where(p_P => p_P.DtAnsweredAt.HasValue).ToList();
            
            foreach (Question v_Question1 in v_Party.Questions)
            {
                int v_Seed = v_Question1.AnswerSeed.GetHashCode();

                Random v_Random = new(v_Seed);
            
                v_Question1.Answers = v_Question1.Answers.OrderBy(_ => v_Random.Next()).ToList();
            }
            
            return new GenericEntityResponse<PartyBase>(v_Party);
        }
    }
}