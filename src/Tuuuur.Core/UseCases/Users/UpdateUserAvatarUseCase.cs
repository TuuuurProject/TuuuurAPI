using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Users;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Users;

internal class UpdateUserAvatarUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<UpdateUserAvatarUseCase> p_Logger,
    IUserRoleService p_UserRoleService)
    : ADbUseCase<UpdateUserAvatarRequest, GenericEntityResponse<User>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<User>> HandleLogic(UpdateUserAvatarRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetEmail();

        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GenericEntityResponse<User>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
            
        v_User.Avatar = p_Request.Avatar;
            
        await m_UnitOfWork.UserRepository.UpdateUserAsync(v_User, p_CancellationToken);
        m_UnitOfWork.Save();
            
        return new GenericEntityResponse<User>(v_User);
    }
}