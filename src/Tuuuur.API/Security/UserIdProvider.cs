using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Tuuuur.API.Security;

/// <summary>
/// Custom UserIdProvider for SignalR to use User ID instead of email/username
/// </summary>
public class UserIdProvider : IUserIdProvider
{
    /// <summary>
    /// Gets the user identifier from ClaimNames.Id (User ID)
    /// </summary>
    public string GetUserId(HubConnectionContext connection)
    {
        // Utilise la constante centralisée ClaimNames.Id
        return connection.User?.FindFirst(Domain.Security.ClaimNames.Id)?.Value;
    }
}
