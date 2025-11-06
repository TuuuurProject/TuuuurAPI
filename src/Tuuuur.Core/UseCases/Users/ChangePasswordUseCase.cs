using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Requests.Users;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Users;

internal class ChangePasswordUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<ChangePasswordUseCase> p_Logger,
    IMediator p_Mediator,
    IUserRoleService p_UserRoleService): 
    ADbUseCase<ChangePasswordRequest, GenericEntityResponse<User>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<User>> HandleLogic(ChangePasswordRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_CurrentUserEmail = p_UserRoleService.GetCurrentUserEmail();
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_CurrentUserEmail, p_CancellationToken);
        if (v_User is null)
            return new GenericEntityResponse<User>([
                new ErrorDto(DomainErrors.Data.NotFound,
                    $"Queried object {nameof(User)} was not found, Key: {v_CurrentUserEmail}")
            ]);
        
        StringResponse v_HashCurrentPassword = await p_Mediator.Send(new HashRequest(p_Request.CurrentPassword), p_CancellationToken);
        if (!v_HashCurrentPassword.Success) 
            return new GenericEntityResponse<User>(v_HashCurrentPassword.Errors);
        
        if(v_HashCurrentPassword.Value != v_User.Password)
            return new GenericEntityResponse<User>([new ErrorDto(DomainErrors.Authentication.Invalid, "The current password is incorrect.")]);
        
        StringResponse v_HashNewPassword = await p_Mediator.Send(new HashRequest(p_Request.NewPassword), p_CancellationToken);
        
        if (!v_HashCurrentPassword.Success) 
            return new GenericEntityResponse<User>(v_HashCurrentPassword.Errors);
        
        v_User.Password = v_HashNewPassword.Value;
        
        await m_UnitOfWork.UserRepository.UpdateUserAsync(v_User, p_CancellationToken);
        _ = m_UnitOfWork.Save();
        
        return new GenericEntityResponse<User>(v_User);
    }
}