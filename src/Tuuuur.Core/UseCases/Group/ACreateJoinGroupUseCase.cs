using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Group;

internal abstract class ACreateJoinGroupUseCase<TRequest>(
    ILogger m_Logger,
    IUnitOfWork m_UnitOfWork, 
    IUserRoleService p_UserRoleService,
    ICacheService p_CacheService)    
    : ADbUseCase<TRequest, GenericEntityResponse<Party>>(m_Logger, m_UnitOfWork)
    where TRequest : IRequest<GenericEntityResponse<Party>>
{
    protected readonly ICacheService m_CacheService = p_CacheService;
    protected override async Task<GenericEntityResponse<Party>> HandleLogic(TRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();
        
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
        
        Guid? v_PartyId = await m_CacheService.GetAsync<Guid?>($"{nameof(User)}:{v_User.Id}:{nameof(Party)}", p_CancellationToken);

        // If a party already exist, 
        if (v_PartyId is not null)
        {
            Party v_ExistingParty = await m_CacheService.GetAsync<Party>($"{nameof(Party)}:{v_PartyId}", p_CancellationToken);
            List<int> v_UserInExistingParty = await m_CacheService.SetMembersAsync<int>($"{nameof(Party)}:{v_PartyId}:{nameof(User)}", p_CancellationToken: p_CancellationToken);
            foreach (int v_UserId in v_UserInExistingParty)
            {
                v_ExistingParty.PartyUsers.Add(new PartyUser { User = await m_UnitOfWork.UserRepository.GetUserByIdAsync(v_UserId, p_CancellationToken), IdUser =  v_UserId});
            }
            
            return new GenericEntityResponse<Party>(v_ExistingParty);
        }

        return await Process(p_Request, v_User, p_CancellationToken);
    }
    
    protected abstract Task<GenericEntityResponse<Party>> Process(TRequest p_Request, User p_User, CancellationToken p_CancellationToken);
}