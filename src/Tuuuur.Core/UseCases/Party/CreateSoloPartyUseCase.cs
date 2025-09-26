using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases;

internal class CreateSoloPartyUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<CreateSoloPartyUseCase> p_Logger, 
    IUserRoleService p_UserRoleService)
    : AUseCase(p_UnitOfWork, p_Logger), IRequestHandler<CreateSoloPartyRequest, GuidResponse>
{
    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task<GuidResponse> Handle(CreateSoloPartyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
            User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, cancellationToken)
                ?? throw new NotFoundException(v_UserEmail, nameof(User));
            
            IEnumerable<Question> v_Questions =  await m_UnitOfWork.QuestionRepository
                .GetQuestionsByThemesIdsAndDifficultiesIdsAndNumberOfQuestionsAsync(request.ThemesIds,
                    request.DifficultiesIds, request.NbQuestions, cancellationToken);

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
                        Order = p_Order,
                        Question = p_Question
                    })
                    .ToList(),          
                Active = true,
            };

            IMappingAddEntity<Party, IEntity> v_MappingAddEntity =
                await m_UnitOfWork.PartyRepository.CreatePartyAsync(v_Party, cancellationToken);
            m_UnitOfWork.Save();
            
            return new GuidResponse(v_MappingAddEntity.MapBoEntity.Id);
        }
        catch (Exception v_Ex)
        {
            m_Logger.LogError(v_Ex, "An error was thrown");
            return new GuidResponse([v_Ex.ToError()]);
        }
    }
}