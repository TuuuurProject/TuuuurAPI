using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Tuuuur.Domain.Notifications;

namespace Tuuuur.API.Notifications;

internal class NotificationsService(
    IHubContext<NotificationsHub, INotificationClient> p_HubContext)
    : INotificationsService
{

    public Task PushMessageAsync<T>(ClientType p_ClientType, T p_Message, IReadOnlyList<string> p_Users)
    {
        string v_Message = JsonSerializer.Serialize(p_Message);

        return PushMessageAsync(p_ClientType, v_Message, p_Users);
    }
        
    public Task PushMessageAsync(ClientType p_ClientType, string p_Message, IReadOnlyList<string> p_Users)
    {
        if (p_Users.Count == 0)
        {
            throw new ArgumentException("Users list is empty");
        }
        if (p_ClientType == ClientType.User)
        {
            if (p_Users.Count != 1)
                throw new ArgumentException("For ClientType.User, exactly one user ID must be provided.", nameof(p_Users));
            
            if (string.IsNullOrWhiteSpace(p_Users[0]))
                throw new ArgumentNullException(nameof(p_Users), "User ID cannot be null or whitespace.");
        }

        return p_ClientType switch
        {
            ClientType.User => PushMessageToUserAsync(p_Users[0], p_Message),
            ClientType.Users => PushMessageToUsersAsync(p_Users, p_Message),
            _ => throw new InvalidOperationException($"Unhandled ClientType: {p_ClientType}"),
        };
    }

    /// <summary>
    /// Push a message to a specific user
    /// </summary>
    /// <param name="p_User"></param>
    /// <param name="p_Message"></param>
    /// <returns></returns>
    private async Task PushMessageToUserAsync(string p_User, string p_Message)
    {
        await p_HubContext.Clients.User(p_User).Notify(p_Message);
    }
    
    /// <summary>
    /// Push a message to listed users
    /// </summary>
    /// <param name="p_Users"></param>
    /// <param name="p_Message"></param>
    private async Task PushMessageToUsersAsync(IReadOnlyList<string> p_Users, string p_Message)
    {
        await p_HubContext.Clients.Users(p_Users).Notify(p_Message);
    }
}