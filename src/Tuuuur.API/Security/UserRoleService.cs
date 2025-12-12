using System.Security.Claims;
using Ardalis.GuardClauses;
using Tuuuur.Domain.Security;

namespace Tuuuur.API.Security;

internal class UserRoleService(IHttpContextAccessor p_Context) : IUserRoleService
{
    public string GetCurrentUserEmail()
    {
        Guard.Against.Null(p_Context.HttpContext);
        return p_Context.HttpContext.User.Claims.FirstOrDefault(p_Claim => p_Claim.Type == ClaimTypes.Email)?.Value;
    }
}