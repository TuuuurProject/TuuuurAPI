using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.History;

internal class GetHistoryUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<GetHistoryUseCase> p_Logger, 
    IUserRoleService p_UserRoleService)
    : ADbUseCase<GetHistoryRequest, GenericEntityResponse<PartyBase>>(p_Logger,  p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<PartyBase>> HandleLogic(GetHistoryRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GenericEntityResponse<PartyBase>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
        
        PartyBase v_Party = await m_UnitOfWork.PartyRepository.GetByIdAsync(p_Request.PartyId, v_User.Id, p_CancellationToken);
        return new GenericEntityResponse<PartyBase>(v_Party);
    }
}