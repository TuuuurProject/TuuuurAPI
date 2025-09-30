using System.Security.Claims;

namespace Tuuuur.API.Extensions;

/// <summary>
/// Extension for Identity
/// </summary>
public static class IdentityExtensions
{
    /// <summary>
    /// Get the email of the user
    /// </summary>
    /// <param name="p_User"></param>
    /// <returns></returns>
    public static string GetUserEmail(this ClaimsPrincipal p_User) =>
        p_User.Claims.First(p_X => p_X.Type == ClaimTypes.Email)?.Value;
}
