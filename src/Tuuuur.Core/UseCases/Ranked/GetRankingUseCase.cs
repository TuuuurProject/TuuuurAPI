using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Ranked;

internal class GetRankingUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<GetRankingUseCase> p_Logger, 
    IUserRoleService p_UserRoleService)
    : ADbUseCase<GetRankingRequest, GenericEntityResponse<RankingPage>>(p_Logger,  p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<RankingPage>> HandleLogic(GetRankingRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetEmail();
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        RankingPage v_Ranking = await m_UnitOfWork.UserRepository.GetRankingPageAsync(v_User?.Id, p_Request.Page, p_Request.Size , p_CancellationToken);
        return new GenericEntityResponse<RankingPage>(v_Ranking);
    }
}