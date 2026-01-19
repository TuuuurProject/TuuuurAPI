using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.History;

internal class GetAllHistoryUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<GetAllHistoryUseCase> p_Logger, 
    IUserRoleService p_UserRoleService)
    : ADbUseCase<GetAllHistoryRequest, GenericEntityResponse<Domain.Bo.History>>(p_Logger,  p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<Domain.Bo.History>> HandleLogic(GetAllHistoryRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GenericEntityResponse<Domain.Bo.History>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
        
        Domain.Bo.History v_Parties = await m_UnitOfWork.PartyRepository.GetUserHistoryAsync(v_User.Id, p_Request.Page, p_Request.Size , p_CancellationToken);
        return new GenericEntityResponse<Domain.Bo.History>(v_Parties);
    }
}