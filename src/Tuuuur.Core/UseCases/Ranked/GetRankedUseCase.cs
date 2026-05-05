using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Ranked;

internal class GetRankedUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<GetRankedUseCase> p_Logger, 
    IUserRoleService p_UserRoleService)
    : ADbUseCase<GetRankedRequest, GenericEntityResponse<RankedParty>>(p_Logger,  p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<RankedParty>> HandleLogic(GetRankedRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetEmail();
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GenericEntityResponse<RankedParty>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);

        RankedParty v_Party = await m_UnitOfWork.PartyRepository.GetRankedByIdAsync(p_Request.PartyId, v_User.Id, p_CancellationToken);
        
        if (v_Party is null || v_Party.IdPartyType != (int)PartyTypeType.Ranked)
            return new GenericEntityResponse<RankedParty>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found, Key: {p_Request.PartyId.ToString()}")]);

        return new GenericEntityResponse<RankedParty>(v_Party);
    }
}
