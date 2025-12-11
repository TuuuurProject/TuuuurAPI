using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;
using Notification = Tuuuur.Domain.Bo.Enum.Notification;

namespace Tuuuur.Core.UseCases.Group;

internal class JoinGroupUseCase(IUnitOfWork p_UnitOfWork, 
    ILogger<JoinGroupUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    ICacheService p_CacheService,
    INotificationsService p_NotificationsService): 
    ACreateJoinGroupUseCase<JoinGroupPartyRequest>(p_Logger, p_UnitOfWork, p_UserRoleService, p_CacheService)
{
    
    protected override async Task<GenericEntityResponse<Party>> Process(JoinGroupPartyRequest p_Request, User p_User, CancellationToken p_CancellationToken)
    {
        Party v_Party = null;
        if (!string.IsNullOrWhiteSpace(p_Request.Code))
        {
            v_Party = await m_CacheService.GetAsync<Party>($"{nameof(Party)}:{p_Request.Code}",  p_CancellationToken);
        }
        
        if(v_Party == null)
            return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found, Key: {p_Request.Code}")]);
        
        List<int> v_UserInParty = await m_CacheService.SetMembersAsync<int>($"{nameof(Party)}:{v_Party.Id}:{nameof(User)}", p_CancellationToken: p_CancellationToken);
        
        await m_CacheService.SetAddAsync($"{nameof(Party)}:{v_Party.Id}:{nameof(User)}", p_User.Id, p_CancellationToken: p_CancellationToken);
        await m_CacheService.SetAsync($"{nameof(User)}:{p_User.Id}:{nameof(Party)}", v_Party.Id, p_CancellationToken: p_CancellationToken);

        foreach (int v_UserIdToNotif in v_UserInParty)
        {
            User v_UserToNotify = await m_UnitOfWork.UserRepository.GetUserByIdAsync(v_UserIdToNotif, p_CancellationToken);
            if (v_UserIdToNotif != p_User.Id)
            {
                await p_NotificationsService.PushMessageAsync(ClientType.User, new Domain.Bo.Notification{ User = p_User, Action= nameof(Notification.Join) }, v_UserToNotify.NickName);
            }
            v_Party.PartyUsers.Add(new PartyUser { User = v_UserToNotify, IdUser =  v_UserIdToNotif});
        }
        v_Party.PartyUsers.Add(new PartyUser { User = p_User, IdUser =  p_User.Id});
        return new GenericEntityResponse<Party>(v_Party);
    }
}