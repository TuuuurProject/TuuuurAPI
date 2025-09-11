using Ardalis.GuardClauses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Domain.Security;
using Tuuuur.Domain.Token;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Tuuuur.Infrastructure.Jwt;

/// <summary>
/// Jwt factory implem
/// </summary>
internal class JwtFactory : IJwtFactory
{
    private readonly JwtConfiguration m_JwtConfiguration;

    public JwtFactory(JwtConfiguration p_JwtConfiguration)
    {
        m_JwtConfiguration = p_JwtConfiguration;
    }

    public JwtTokenResponse CreateToken(User p_UserInfos)
    {
        Guard.Against.Null(p_UserInfos);

        string v_Role;
        if (p_UserInfos.IsSuperAdmin)
        {
            v_Role = RolesType.SuperAdmin;
        }
        else if (p_UserInfos.IsAdmin)
        {
            v_Role = RolesType.Admin;
        }
        else
        {
            v_Role = RolesType.User;
        }
        SecurityTokenDescriptor v_TokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Sid, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.GivenName, p_UserInfos.FullName),
                new Claim(ClaimTypes.Name, p_UserInfos.FullName),
                new Claim(JwtRegisteredClaimNames.Sub, p_UserInfos.FullName),
                new Claim(ClaimTypes.Email, p_UserInfos.Email),
                new Claim(JwtRegisteredClaimNames.Email, p_UserInfos.Email),
                new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, v_Role)
            }),
            Expires = DateTime.UtcNow.AddMinutes(m_JwtConfiguration.Validity),
            Issuer = m_JwtConfiguration.Issuer,
            Audience = m_JwtConfiguration.Audience,
            SigningCredentials = new SigningCredentials
            (new SymmetricSecurityKey(Encoding.ASCII.GetBytes
                    (m_JwtConfiguration.Key)),
                SecurityAlgorithms.HmacSha512Signature)
        };
        JwtSecurityTokenHandler v_TokenHandler = new();
        SecurityToken v_Token = v_TokenHandler.CreateToken(v_TokenDescriptor);

        return new JwtTokenResponse
        {
            Token = v_TokenHandler.WriteToken(v_Token),
            ValidTo = v_Token.ValidTo,
            ValidFrom = v_Token.ValidFrom
        };
    }
}