using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Tuuuur.API.Security;

/// <summary>
/// Custom UserIdProvider for SignalR to use User ID instead of email/username
/// </summary>
public class UserIdProvider : IUserIdProvider
{
    /// <summary>
    /// Gets the user identifier from ClaimTypes.Sid (User ID)
    /// </summary>
    public string GetUserId(HubConnectionContext p_Connection)
    {
        // Use ClaimTypes.Sid which contains the user ID (set in JwtFactory.cs)
        return p_Connection.User?.FindFirst(ClaimTypes.Sid)?.Value;
    }
}
