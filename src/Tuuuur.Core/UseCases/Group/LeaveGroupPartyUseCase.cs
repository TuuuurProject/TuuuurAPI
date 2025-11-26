using MediatR;
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

internal class LeaveGroupPartyUseCase(IUnitOfWork p_UnitOfWork, 
    ILogger<LeaveGroupPartyUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    IMediator p_Mediator,
    INotificationsService p_NotificationsService): 
    ADbUseCase<LeaveGroupPartyRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(LeaveGroupPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();

        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);
        
        if(v_User == null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);
        
        
        IEnumerable<Party> v_Parties;
        lock (InMemoryDataStore.PartyInProgress)
        {
            // Check if the user is already in party
            v_Parties  = InMemoryDataStore.PartyInProgress.Where(p_Party => p_Party.PartyUsers.Any(u => u.IdUser == v_User.Id)).ToList();
        }
        
        
        if (!v_Parties.Any())
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        }
        Party v_Party = v_Parties.First();
        
        // If user is not in the party
        if(v_Party?.PartyUsers.FirstOrDefault(p_P => p_P.IdUser == v_User.Id) is null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);

        // If the host user leave the party, destroy the party
        if (v_Party.IdUserHost == v_User.Id)
        {
            await p_Mediator.Send(new DeleteGroupPartyRequest(), CancellationToken.None);
        }
        else
        {
            // Remove the user to the party
            lock (InMemoryDataStore.PartyInProgress)
            {
                PartyUser v_PartyUser = v_Party.PartyUsers.FirstOrDefault(p_PartyUser => p_PartyUser.IdUser == v_User.Id);
                if (v_PartyUser != null)
                {
                    v_Party.PartyUsers.Remove(v_PartyUser);
                }
            }
            
            // Send notification to other users
            string[] v_UsersInParty = v_Party.PartyUsers.Where(p_P => p_P.IdUser != v_User.Id).Select(p_P => p_P.User.NickName).ToArray();
            await p_NotificationsService.PushMessageAsync(ClientType.Users, new Domain.Bo.Notification{ User = v_User, Action= nameof(Notification.Leave) }, v_UsersInParty);
        }

        return new EmptyResponse();
    }
}