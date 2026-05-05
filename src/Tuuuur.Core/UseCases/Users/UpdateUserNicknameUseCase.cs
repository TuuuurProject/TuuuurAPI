using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Users;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Users;

internal class UpdateUserNicknameUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<UpdateUserNicknameUseCase> p_Logger,
    IUserRoleService p_UserRoleService)
    : ADbUseCase<UpdateUserNicknameRequest, GenericEntityResponse<User>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<User>> HandleLogic(UpdateUserNicknameRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetEmail();

        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GenericEntityResponse<User>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
            
        User v_UserByNickName = await m_UnitOfWork.UserRepository.GetUserByNickNameAsync(p_Request.Nickname, p_CancellationToken);
        if (v_UserByNickName != null && v_UserByNickName.Id != v_User.Id)
        {
            return new GenericEntityResponse<User>([new ErrorDto(DomainErrors.Data.AlreadyExist, $"An {nameof(User)} already exist with the nickname {p_Request.Nickname}.")]);
        }
        
        v_User.NickName = p_Request.Nickname;
            
        await m_UnitOfWork.UserRepository.UpdateUserAsync(v_User, p_CancellationToken);
        m_UnitOfWork.Save();
            
        return new GenericEntityResponse<User>(v_User);
    }
}