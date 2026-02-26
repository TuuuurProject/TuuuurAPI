using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Users;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Users;

internal class DeleteUserUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<DeleteUserUseCase> p_Logger,
    IUserRoleService p_UserRoleService): 
    ADbUseCase<DeleteUserRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(DeleteUserRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_CurrentUserEmail = p_UserRoleService.GetEmail();
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_CurrentUserEmail, p_CancellationToken);
        if (v_User is null)
            return new EmptyResponse([
                new ErrorDto(DomainErrors.Data.NotFound,
                    $"Queried object {nameof(User)} was not found, Key: {v_CurrentUserEmail}")
            ]);
        
        await m_UnitOfWork.UserRepository.DeleteUserAsync(v_User.Id, p_CancellationToken);
        _ = m_UnitOfWork.Save();
        return new EmptyResponse();
    }
}