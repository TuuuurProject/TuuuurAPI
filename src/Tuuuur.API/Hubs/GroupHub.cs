using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Tuuuur.API.Hubs;

/// <summary>
/// SignalR notification hub for INotificationClient
/// </summary>
[Authorize]
public class GroupHub : Hub<IGroupClient>
{
    
}