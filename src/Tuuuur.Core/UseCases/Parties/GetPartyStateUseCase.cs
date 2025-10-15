using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Parties;

internal class GetPartyStateUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<GetPartyStateUseCase> p_Logger, 
    IUserRoleService p_UserRoleService)
    : AUseCase(p_UnitOfWork, p_Logger), IRequestHandler<GetPartyStateRequest, GenericEntityResponse<Party>>
{
    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task<GenericEntityResponse<Party>> Handle(GetPartyStateRequest p_StateRequest, CancellationToken cancellationToken)
    {
        try
        {
            string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
            User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, cancellationToken)
                ?? throw new NotFoundException(v_UserEmail, nameof(User));

            Party v_Party = await m_UnitOfWork.PartyRepository.GetByIdAsync(p_StateRequest.PartyId, v_User.Id, cancellationToken);
            
            // Check if the party type is correct
            // Check if the party contains only 1 player
            // Check that the player is this user
            if (v_Party is null || v_Party.IdPartyType != (int)PartyTypeType.Solo || v_Party.PartyUsers.Count != 1 && v_Party.PartyUsers.First().IdUser != v_User.Id)
                throw new NotFoundException(p_StateRequest.PartyId.ToString(), nameof(Party));

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
                    };
                    _ = await m_UnitOfWork.UserPartyQuestionRepository.CreateUserPartyQuestionAsync(v_UserPartyQuestionToAdd, cancellationToken);
                }

                _ = m_UnitOfWork.Save();
            }
            
            v_Party = await m_UnitOfWork.PartyRepository.GetByIdAsync(p_StateRequest.PartyId, v_User.Id, cancellationToken);
            v_Party.PartyQuestions = v_Party.PartyQuestions.Where(p_P => p_P.UserPartyQuestion is not null).ToList();
            
            v_Party.PartyQuestions
                .Where(p_Question => p_Question.UserPartyQuestion.IdAnwser == null)
                .ToList()
                .ForEach(p_Question => 
                {
                    p_Question.Question.Answer.ForEach(p_Answer => p_Answer.Valid = null);
                });
            
            return new GenericEntityResponse<Party>(v_Party);
        }
        catch (Exception v_Ex)
        {
            m_Logger.LogError(v_Ex, "An error was thrown");
            return new GenericEntityResponse<Party>([v_Ex.ToError()]);
        }
    }
}