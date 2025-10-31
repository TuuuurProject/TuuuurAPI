using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Parties;

internal class CreateSoloPartyUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<CreateSoloPartyUseCase> p_Logger, 
    IUserRoleService p_UserRoleService)
    : ADbUseCase<CreateSoloPartyRequest, GuidResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GuidResponse> HandleLogic(CreateSoloPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GuidResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
            
        IEnumerable<Question> v_Questions =  await m_UnitOfWork.QuestionRepository
            .GetQuestionsByThemesIdsAndDifficultiesIdsAndNumberOfQuestionsAsync(p_Request.ThemesIds,
                p_Request.DifficultiesIds, p_Request.NbQuestions, p_CancellationToken);

        Party v_Party = new()
        {
            IdPartyType = (int)PartyTypeType.Solo,
            Code = null,
            PartyUsers = [new PartyUser(){ IdUser = v_User.Id }],
            IdUserHost = v_User.Id,
            PartyQuestions = v_Questions
                .Select((p_Question, p_Order) => new PartyQuestion()
                {
                    IdQuestion = p_Question.Id,
                    Order = p_Order + 1,
                })
                .ToList(),          
            Active = true,
        };

        IMappingAddEntity<Party, IEntity> v_MappingAddEntity =
            await m_UnitOfWork.PartyRepository.CreatePartyAsync(v_Party, p_CancellationToken);
        m_UnitOfWork.Save();
            
        return new GuidResponse(v_MappingAddEntity.MapBoEntity.Id);
    }
}