using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ardalis.GuardClauses;
using Tuuuur.Domain.Security;

namespace Tuuuur.API.Security;

internal class UserRoleService(IHttpContextAccessor p_Context) : IUserRoleService
{
    public string GetEmail()
    {
        Guard.Against.Null(p_Context.HttpContext);
        return p_Context.HttpContext.User.Claims.FirstOrDefault(p_Claim => p_Claim.Type == ClaimTypes.Email)?.Value;
    }
    public Guid GetUserId()
    {
        Guard.Against.Null(p_Context.HttpContext);
        return Guid.TryParse(p_Context.HttpContext.User.Claims.FirstOrDefault(p_Claim => p_Claim.Type == ClaimNames.Id)
            ?.Value ?? string.Empty, out Guid v_UserId) ? v_UserId : Guid.Empty;
    }
}