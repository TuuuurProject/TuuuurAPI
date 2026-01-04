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
        if (p_UserInfos.IsAdmin)
        {
            v_Role = RolesType.Admin;
        }
        else
        {
            v_Role = RolesType.User;
        }
        SecurityTokenDescriptor v_TokenDescriptor = new()
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Sid, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, p_UserInfos.NickName),
                new Claim(ClaimTypes.Email, p_UserInfos.Email),
                new Claim(JwtRegisteredClaimNames.Email, p_UserInfos.Email),
                new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, v_Role)
            ]),
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

<<<<<<< Updated upstream
=======
        string v_RefreshToken = GenerateRefreshToken();
        DateTime v_RefreshTokenExpiry = DateTime.UtcNow.AddDays(p_JwtConfiguration.RefreshTokenValidity);

        RefreshToken v_RefreshTokenEntity = new()
        {
            UserId = p_UserInfos.Id,
            Token = v_RefreshToken,
            ExpiresAt = v_RefreshTokenExpiry,
            CreatedAt = DateTime.UtcNow,
        };

        await p_UnitOfWork.RefreshTokenRepository.CreateRefreshTokenAsync(v_RefreshTokenEntity, p_CancellationToken);

>>>>>>> Stashed changes
        return new JwtTokenResponse
        {
            Token = v_TokenHandler.WriteToken(v_Token),
            ValidTo = v_Token.ValidTo,
            ValidFrom = v_Token.ValidFrom
        };
    }
}