using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Requests.Parties;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Parties;

internal class GetPartyUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<GetPartyUseCase> p_Logger, 
    IUserRoleService p_UserRoleService)
    : ADbUseCase<GetSoloPartyStateRequest, GenericEntityResponse<Party>>(p_Logger,  p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<Party>> HandleLogic(GetSoloPartyStateRequest p_Request, CancellationToken p_CancellationToken)
    {
            string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
            User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
            
            if(v_User == null)
                return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);

            Party v_Party = await m_UnitOfWork.PartyRepository.GetByIdAsync(p_Request.PartyId, v_User.Id, p_CancellationToken);
            
            // Check if the party type is correct
            // Check if the party contains only 1 player
            // Check that the player is this user
            if (v_Party is null || v_Party.IdPartyType != (int)PartyTypeType.Solo || v_Party.PartyUsers.Count != 1 && v_Party.PartyUsers[0].IdUser != v_User.Id)
                return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found, Key: {p_Request.PartyId.ToString()}")]);

            IEnumerable<UserPartyQuestion> v_UserPartyQuestions = v_Party.PartyQuestions.Select(p_P => p_P.UserPartyQuestion).Where(p_P => p_P is not null);

            IEnumerable<UserPartyQuestion> v_PartyQuestions = v_UserPartyQuestions as UserPartyQuestion[] ?? v_UserPartyQuestions.ToArray();
            UserPartyQuestion v_UserPartyQuestion = v_PartyQuestions.FirstOrDefault(p_P => p_P.Correct == null);

            // If user party question not exist, create it
            if(v_UserPartyQuestion is null)
            {
                PartyQuestion v_PartyQuestion = v_Party.PartyQuestions.OrderBy(p_P => p_P.Order).FirstOrDefault(p_P => p_P.UserPartyQuestion is null);

                if (v_PartyQuestion != null)
                {
                    UserPartyQuestion v_UserPartyQuestionToAdd = new()
                    {
                        IdUser = v_User.Id,
                        IdPartyQuestion = v_PartyQuestion.Id,
                        Correct = null,
                        DtPresentedAt = DateTime.UtcNow
                    };
                    _ = await m_UnitOfWork.UserPartyQuestionRepository.CreateUserPartyQuestionAsync(v_UserPartyQuestionToAdd, p_CancellationToken);
                }

                _ = m_UnitOfWork.Save();
            }
            
            v_Party = await m_UnitOfWork.PartyRepository.GetByIdAsync(p_Request.PartyId, v_User.Id, p_CancellationToken);
            v_Party.NbQuestions = v_Party.PartyQuestions.Count;
            v_Party.PartyQuestions = v_Party.PartyQuestions.Where(p_P => p_P.UserPartyQuestion is not null).ToList();
            
            v_Party.PartyQuestions
                .Where(p_Question => p_Question.UserPartyQuestion.IdAnswer == null)
                .ToList()
                .ForEach(p_Question => 
                {
                    p_Question.Question.Answer.ForEach(p_Answer => p_Answer.Valid = null);
                });

            foreach (PartyQuestion v_PartyQuestion in v_Party.PartyQuestions)
            {
                int v_Seed = v_PartyQuestion.UserPartyQuestion.AnswersOrder.GetHashCode();

                Random v_Random = new(v_Seed);
                
                v_PartyQuestion.Question.Answer = v_PartyQuestion.Question.Answer.OrderBy(_ => v_Random.Next()).ToList();
            }
            
            return new GenericEntityResponse<Party>(v_Party);
    }
}