using Microsoft.Extensions.Logging;
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
    : ADbUseCase<GetSoloPartyStateRequest, GenericEntityResponse<PartyBase>>(p_Logger,  p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<PartyBase>> HandleLogic(GetSoloPartyStateRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GenericEntityResponse<PartyBase>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);

        PartyBase v_Party = await m_UnitOfWork.PartyRepository.GetByIdAsync(p_Request.PartyId, v_User.Id, p_CancellationToken);
        
        // Check if the party type is correct
        // Check if the party contains only 1 player
        // Check that the player is this user
        if (v_Party is null || v_Party.IdPartyType != (int)PartyTypeType.Solo || v_Party.Users.Count != 1 && v_Party.Users.First().Id != v_User.Id)
            return new GenericEntityResponse<PartyBase>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(PartyBase)} was not found, Key: {p_Request.PartyId.ToString()}")]);

        // Check if question in progress
        Question v_Question = v_Party.Questions.OrderBy(p_P => p_P.Index)
            .FirstOrDefault(p_P => !p_P.DtAnsweredAt.HasValue 
                                   && p_P.DtPresentedAt.HasValue);
        
        // If the question is not answered, create it
        if(v_Question is null)
        {
            // Get next question who hasn't presented to user
            v_Question = v_Party.Questions.OrderBy(p_P => p_P.Index)
                .FirstOrDefault(p_P => !p_P.DtPresentedAt.HasValue);

            if (v_Question is not null)
            {
                // Associate user to this question of this party
                await m_UnitOfWork.UserPartyQuestionRepository.CreateUserQuestionAsync(v_Party.Id, v_User.Id, v_Question.Id, p_CancellationToken);
                _ = m_UnitOfWork.Save();
            }
        }
        
        v_Party = await m_UnitOfWork.PartyRepository.GetByIdAsync(p_Request.PartyId, v_User.Id, p_CancellationToken);
        v_Party.Questions = v_Party.Questions.Where(p_P => p_P.DtPresentedAt.HasValue).ToList();

        foreach (Question v_Question1 in v_Party.Questions)
        {
            if (!v_Question1.DtPresentedAt.HasValue || !v_Question1.DtAnsweredAt.HasValue)
            {
                v_Question1.ClearAnswer();
            }
            int v_Seed = v_Question1.AnswerSeed.GetHashCode();
            Random v_Random = new(v_Seed);
            v_Question1.Answers = v_Question1.Answers.OrderBy(_ => v_Random.Next()).ToList();
        }
        
        return new GenericEntityResponse<PartyBase>(v_Party);
    }
}