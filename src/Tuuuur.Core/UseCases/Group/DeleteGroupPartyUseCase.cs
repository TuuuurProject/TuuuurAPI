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

namespace Tuuuur.Core.UseCases.Group;

internal class DeleteGroupPartyUseCase(IUnitOfWork p_UnitOfWork, 
    ILogger<DeleteGroupPartyUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    INotificationsService p_NotificationsService): 
    ADbUseCase<DeleteGroupPartyRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(DeleteGroupPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();

        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
        
        IEnumerable<Party> v_Parties;
        lock (InMemoryDataStore.PartyInProgress)
        {
            // Check if the user is already in party
            v_Parties  = InMemoryDataStore.PartyInProgress.Where(p_Party => p_Party.PartyUsers.Any(p_User => p_User.IdUser == v_User.Id)).ToList();
        }

        Party v_Party = null;
        if (!v_Parties.Any())
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        }
        if (v_Parties.Count() == 1)
        {
            v_Party = v_Parties.First();
        }
        else
        {
            //Delete the 
            foreach (List<PartyUser> v_PartyUsers in v_Parties
                         .Where(p_Party => v_Party != null && p_Party.Id != v_Party.Id)
                         .Select(p_Party => p_Party.PartyUsers))
            {
                PartyUser v_PartyUser = v_PartyUsers.FirstOrDefault(p_PartyUser => p_PartyUser.IdUser == v_User.Id);
                v_PartyUsers.Remove(v_PartyUser);
            }
        }
        
        // If the party not exist or if user is not the host
        if(v_Party?.IdUserHost != v_User.Id)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        
        // Send notification to other users
        foreach (PartyUser v_PartyUser in v_Party.PartyUsers.Where(p_P => p_P.IdUser != v_User.Id))
        {
            await p_NotificationsService.PushMessageAsync(ClientType.User, new Notification{ User = v_User, Action= nameof(NotificationEnum.Delete) }, v_PartyUser.User.NickName);
        }
        
        // Delete the party
        lock (InMemoryDataStore.PartyInProgress)
        {
            InMemoryDataStore.PartyInProgress.RemoveAll(p_Party => p_Party.Id == v_Party.Id);
        }

        return new EmptyResponse();
    }
}