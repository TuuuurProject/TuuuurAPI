using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Users;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Services;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Users;

internal class GetCurrentUserUseCase(
    IUnitOfWork p_UnitOfWork,
    ILogger<GetCurrentUserUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    IEloService p_EloService) :
    ADbUseCase<GetCurrentUserRequest, GenericEntityResponse<User>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<User>> HandleLogic(GetCurrentUserRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_CurrentUserEmail = p_UserRoleService.GetEmail();
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_CurrentUserEmail, p_CancellationToken);
        if (v_User is null)
            return new GenericEntityResponse<User>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_CurrentUserEmail}")]);

        v_User.Rank = p_EloService.GetRank(v_User.GlobalElo);
        return new GenericEntityResponse<User>(v_User);
    }
}