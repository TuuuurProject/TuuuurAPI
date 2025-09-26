using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Tuuuur.API.Notifications;

/// <summary>
/// SignalR notification hub for INotificationClient
/// </summary>
[Authorize]
public class NotificationsHub : Hub<INotificationClient>
{
}

/// <summary>
/// Notification service used through signalR
/// </summary>
public interface INotificationClient
{
    /// <summary>
    /// Send a notification message to a connect client
    /// </summary>
    /// <param name="p_Message"></param>
    /// <returns></returns>
    Task Notify(string p_Message);
}