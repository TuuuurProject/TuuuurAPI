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

internal class DeleteGroupPartyUseCase(IUnitOfWork p_UnitOfWork, 
    ILogger<DeleteGroupPartyUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    INotificationsService p_NotificationsService,
    ICacheService p_CacheService): 
    ADbUseCase<DeleteGroupPartyRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(DeleteGroupPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();

        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
        
        Guid? v_Parties = await p_CacheService.GetAsync<Guid?>($"{nameof(User)}:{v_User.Id}:{nameof(Party)}", p_CancellationToken);
        if (v_Parties is null)
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        } 
        
        Party v_Party = await p_CacheService.GetAsync<Party>($"{nameof(Party)}:{v_Parties}",  p_CancellationToken);
        
        List<int> v_UserInParty = await p_CacheService.SetMembersAsync<int>($"{nameof(Party)}:{v_Party.Id}:{nameof(User)}", p_CancellationToken: p_CancellationToken);
        
        // If the party not exist or if user is not the host
        if(v_Party.IdUserHost != v_User.Id)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        
        // Send notification to other users
        foreach (int v_UserIdToNotif in v_UserInParty.Where(p_P => p_P != v_User.Id))
        {
            User v_UserToNotify = await m_UnitOfWork.UserRepository.GetUserByIdAsync(v_UserIdToNotif, p_CancellationToken);
            await p_NotificationsService.PushMessageAsync(ClientType.User, new Domain.Bo.Notification{ User = v_User, Action= nameof(Notification.Delete) }, v_UserToNotify.NickName);
            await p_CacheService.RemoveAsync($"{nameof(User)}:{v_UserIdToNotif}:{nameof(Party)}", p_CancellationToken: p_CancellationToken);
        }
        
        await p_CacheService.RemoveAsync($"{nameof(Party)}:{v_Party.Code}", p_CancellationToken: p_CancellationToken);
        await p_CacheService.RemoveAsync($"{nameof(Party)}:{v_Party.Id}", p_CancellationToken: p_CancellationToken);
        await p_CacheService.RemoveAsync($"{nameof(Party)}:{v_Party.Id}:{nameof(User)}", p_CancellationToken: p_CancellationToken);
        await p_CacheService.RemoveAsync($"{nameof(User)}:{v_User.Id}:{nameof(Party)}", p_CancellationToken: p_CancellationToken);
        
        return new EmptyResponse();
    }
}