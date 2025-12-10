using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;
using Notification = Tuuuur.Domain.Bo.Enum.Notification;

namespace Tuuuur.Core.UseCases.Group;

internal class JoinGroupPartyUseCase(IUnitOfWork p_UnitOfWork, 
    ILogger<JoinGroupPartyUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    INotificationsService p_NotificationsService,
    ICacheService p_CacheService): 
    ADbUseCase<JoinGroupPartyRequest, GenericEntityResponse<Party>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<Party>> HandleLogic(JoinGroupPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();

        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
        
        Guid? v_PartyId = await p_CacheService.GetAsync<Guid?>($"{nameof(User)}:{v_User.Id}:{nameof(Party)}", p_CancellationToken);

        if (v_PartyId is not null)
        {
            Party v_ExistingParty = await p_CacheService.GetAsync<Party>($"{nameof(Party)}:{v_PartyId}", p_CancellationToken);
            List<int> v_UserInExistingParty = await p_CacheService.SetMembersAsync<int>($"{nameof(Party)}:{v_PartyId}:{nameof(User)}", p_CancellationToken: p_CancellationToken);
            foreach (int v_UserId in v_UserInExistingParty)
            {
                v_ExistingParty.PartyUsers.Add(new PartyUser { User = await m_UnitOfWork.UserRepository.GetUserByIdAsync(v_UserId, p_CancellationToken), IdUser =  v_UserId});
            }
            
            return new GenericEntityResponse<Party>(v_ExistingParty);
        }

        Party v_Party = null;
        if (!string.IsNullOrWhiteSpace(p_Request.Code))
        {
            v_Party = await p_CacheService.GetAsync<Party>($"{nameof(Party)}:{p_Request.Code}",  p_CancellationToken);
        }
        
        if(v_Party == null)
            return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found, Key: {p_Request.Code}")]);
        
        List<int> v_UserInParty = await p_CacheService.SetMembersAsync<int>($"{nameof(Party)}:{v_Party.Id}:{nameof(User)}", p_CancellationToken: p_CancellationToken);
        
        await p_CacheService.SetAddAsync($"{nameof(Party)}:{v_Party.Id}:{nameof(User)}", v_User.Id, p_CancellationToken: p_CancellationToken);
        await p_CacheService.SetAsync($"{nameof(User)}:{v_User.Id}:{nameof(Party)}", v_Party.Id, p_CancellationToken: p_CancellationToken);

        foreach (int v_UserIdToNotif in v_UserInParty)
        {
            User v_UserToNotify = await m_UnitOfWork.UserRepository.GetUserByIdAsync(v_UserIdToNotif, p_CancellationToken);
            if (v_UserIdToNotif != v_User.Id)
            {
                await p_NotificationsService.PushMessageAsync(ClientType.User, new Domain.Bo.Notification{ User = v_User, Action= nameof(Notification.Join) }, v_UserToNotify.NickName);
            }
            v_Party.PartyUsers.Add(new PartyUser { User = v_UserToNotify, IdUser =  v_UserIdToNotif});
        }
        v_Party.PartyUsers.Add(new PartyUser { User = v_User, IdUser =  v_User.Id});
        return new GenericEntityResponse<Party>(v_Party);
    }
}