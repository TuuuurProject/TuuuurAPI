using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Users;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Users;

internal class GetCurrentUserUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<GetCurrentUserUseCase> p_Logger,
    IUserRoleService p_UserRoleService): 
    ADbUseCase<GetCurrentUserRequest, GenericEntityResponse<User>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<User>> HandleLogic(GetCurrentUserRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_CurrentUserEmail = p_UserRoleService.GetCurrentUserEmail();
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_CurrentUserEmail, p_CancellationToken);
        return v_User is null 
            ? new GenericEntityResponse<User>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_CurrentUserEmail}")]) 
            : new GenericEntityResponse<User>(v_User);
    }
}