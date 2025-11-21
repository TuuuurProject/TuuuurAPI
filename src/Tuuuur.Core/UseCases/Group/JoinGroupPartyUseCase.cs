using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;
using Notification = Tuuuur.Domain.Bo.Enum.Notification;

namespace Tuuuur.Core.UseCases.Group;

internal class JoinGroupPartyUseCase(IUnitOfWork p_UnitOfWork, 
    ILogger<JoinGroupPartyUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    INotificationsService p_NotificationsService): 
    ADbUseCase<JoinGroupPartyRequest, GenericEntityResponse<Party>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<Party>> HandleLogic(JoinGroupPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();

        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);

        IEnumerable<Party> v_Parties;
        lock (InMemoryDataStore.PartyInProgress)
        {
            // Check if the user is already in party
            v_Parties  = InMemoryDataStore.PartyInProgress.Where(p_Party => p_Party.PartyUsers.Any(u => u.IdUser == v_User.Id));
        }

        Party v_Party = null;
        string v_CodeToFind = p_Request.Code;
        IEnumerable<Party> v_Enumerable = v_Parties as Party[] ?? v_Parties.ToArray();
        // If the user is already in party, put it in the party
        if (v_Enumerable.Count() == 1 && v_CodeToFind is null)
        {
            v_Party = v_Enumerable.First();
        }
        else if (!string.IsNullOrWhiteSpace(v_CodeToFind))
        {
            lock (InMemoryDataStore.PartyInProgress)
            {
                v_Party = InMemoryDataStore.PartyInProgress.FirstOrDefault(p_Party => p_Party.Code == v_CodeToFind);
            }
        }
        
        if(v_Party == null)
            return new GenericEntityResponse<Party>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found, Key: {p_Request.Code}")]);

        // If the user is already in the party, not add it again
        if (v_Party.PartyUsers.FirstOrDefault(p_P => p_P.IdUser == v_User.Id) is not null)
        {
            return new GenericEntityResponse<Party>(v_Party);
        }
        
        // Add the user to the party
        lock (InMemoryDataStore.PartyInProgress)
        {
            v_Party.PartyUsers.Add(new PartyUser{ IdUser = v_User.Id, User = v_User});
        }

        // Send notification to other users
        foreach (PartyUser v_PartyUser in v_Party.PartyUsers.Where(p_P => p_P.IdUser != v_User.Id))
        {
            await p_NotificationsService.PushMessageAsync(ClientType.User, new Domain.Bo.Notification{ User = v_User, Action= nameof(Notification.Join) }, v_PartyUser.User.NickName);
        }

        return new GenericEntityResponse<Party>(v_Party);
    }
}