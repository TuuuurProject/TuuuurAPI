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
    : ADbUseCase<GetHistoryRequest, GenericEntityResponse<HistoryPage>>(p_Logger,  p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<HistoryPage>> HandleLogic(GetHistoryRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GenericEntityResponse<HistoryPage>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
        
        HistoryPage v_Parties = await m_UnitOfWork.PartyRepository.GetUserHistoryAsync(v_User.Id, p_Request.Page, p_Request.Size , p_CancellationToken);
        return new GenericEntityResponse<HistoryPage>(v_Parties);
    }
}