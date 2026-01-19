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
        if (v_Party is null || v_Party.IdPartyType != (int)PartyTypeType.Solo || v_Party.Users.Count != 1 && v_Party.Users[0].Id != v_User.Id)
            return new GenericEntityResponse<PartyBase>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(PartyBase)} was not found, Key: {p_Request.PartyId.ToString()}")]);

        Question v_Question = v_Party.Questions.OrderBy(p_P => p_P.Index).FirstOrDefault(p_P => p_P.DtAnsweredAt == null);
        
        // If question are answered, send the next one
        if(v_Question == null)
        {
            v_Question = v_Party.Questions.OrderBy(p_P => p_P).FirstOrDefault(p_P => !p_P.DtAnsweredAt.HasValue);

            if (v_Question != null)
            {
                UserPartyQuestion v_UserPartyQuestionToAdd = new()
                {
                    IdUser = v_User.Id,
                    IdPartyQuestion = v_Question.Id,
                    Correct = null,
                    DtPresentedAt = DateTime.UtcNow
                };
                _ = await m_UnitOfWork.UserPartyQuestionRepository.CreateUserPartyQuestionAsync(v_UserPartyQuestionToAdd, p_CancellationToken);
            }

            _ = m_UnitOfWork.Save();
        }
        
        v_Party = await m_UnitOfWork.PartyRepository.GetByIdAsync(p_Request.PartyId, v_User.Id, p_CancellationToken);
        v_Party.Questions = v_Party.Questions.Where(p_P => p_P.DtAnsweredAt.HasValue).ToList();
        
        v_Party.Questions
            .Where(p_Question => p_Question.UserAnswer == null)
            .ToList()
            .ForEach(p_Question => 
            {
                p_Question.ClearAnswer();
            });

        foreach (Question v_Question1 in v_Party.Questions)
        {
            int v_Seed = v_Question1.AnswerSeed.GetHashCode();

            Random v_Random = new(v_Seed);
            
            v_Question1.Answer = v_Question1.Answer.OrderBy(_ => v_Random.Next()).ToList();
        }
        
        return new GenericEntityResponse<PartyBase>(v_Party);
    }
}