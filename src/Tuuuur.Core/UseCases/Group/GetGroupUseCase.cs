using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Group;

internal class GetGroupUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<GetGroupUseCase> p_Logger, 
    IUserRoleService p_UserRoleService)
    : ADbUseCase<GetGroupRequest, GenericEntityResponse<GroupParty>>(p_Logger,  p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<GroupParty>> HandleLogic(GetGroupRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GenericEntityResponse<GroupParty>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);

        GroupParty v_Party = await m_UnitOfWork.PartyRepository.GetGroupByIdAsync(p_Request.PartyId, v_User.Id, p_CancellationToken);
        
        if (v_Party is null || v_Party.IdPartyType != (int)PartyTypeType.Group)
            return new GenericEntityResponse<GroupParty>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found, Key: {p_Request.PartyId.ToString()}")]);

        return new GenericEntityResponse<GroupParty>(v_Party);
    }
}