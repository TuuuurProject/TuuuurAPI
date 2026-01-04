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
    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public string GetUserId(HubConnectionContext connection)
    {
        // Use ClaimTypes.Sid which contains the user ID (set in JwtFactory.cs)
        return connection.User?.FindFirst(ClaimTypes.Sid)?.Value;
    }
}
