using Ardalis.GuardClauses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Domain.Security;
using Tuuuur.Domain.Token;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Tuuuur.Infrastructure.Jwt;

/// <summary>
/// Jwt factory implem
/// </summary>
internal class JwtFactory(JwtConfiguration p_JwtConfiguration) : IJwtFactory
{
    public async Task<JwtTokenResponse> CreateTokenAsync(User p_UserInfos, IUnitOfWork p_UnitOfWork, CancellationToken p_CancellationToken = default)
    {
        Guard.Against.Null(p_UserInfos);
        Guard.Against.Null(p_UnitOfWork);

        string v_Role = p_UserInfos.IsAdmin ? RolesType.Admin : RolesType.User;
        SecurityTokenDescriptor v_TokenDescriptor = new()
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimNames.Id, p_UserInfos.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, p_UserInfos.NickName),
                new Claim(JwtRegisteredClaimNames.Email, p_UserInfos.Email),
                new Claim(ClaimNames.Role, v_Role)
            ]),

            Expires = DateTime.UtcNow.AddMinutes(p_JwtConfiguration.Validity),
            Issuer = p_JwtConfiguration.Issuer,
            Audience = p_JwtConfiguration.Audience,
            SigningCredentials = new SigningCredentials
            (new SymmetricSecurityKey(Encoding.ASCII.GetBytes
                    (p_JwtConfiguration.Key)),
                SecurityAlgorithms.HmacSha512Signature)
        };
        JwtSecurityTokenHandler v_TokenHandler = new();
        SecurityToken v_Token = v_TokenHandler.CreateToken(v_TokenDescriptor);

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
        p_UnitOfWork.Save();

        return new JwtTokenResponse
        {
            Token = v_TokenHandler.WriteToken(v_Token),
            ValidTo = v_Token.ValidTo,
            ValidFrom = v_Token.ValidFrom,
            RefreshToken = v_RefreshToken,
            RefreshTokenExpiresAt = v_RefreshTokenExpiry
        };
    }

    public int? GetUserIdFromToken(string p_Token)
    {
        if (string.IsNullOrWhiteSpace(p_Token))
            return null;

        try
        {
            // Remove "Bearer " prefix if present
            string v_Token = p_Token.Trim();
            if (v_Token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                v_Token = v_Token[7..].Trim();
            }

            JwtSecurityTokenHandler v_TokenHandler = new();
            TokenValidationParameters v_ValidationParameters = new()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(p_JwtConfiguration.Key)),
                ValidateIssuer = true,
                ValidIssuer = p_JwtConfiguration.Issuer,
                ValidateAudience = true,
                ValidAudience = p_JwtConfiguration.Audience,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            ClaimsPrincipal v_Principal = v_TokenHandler.ValidateToken(v_Token, v_ValidationParameters, out SecurityToken v_ValidatedToken);

            // Verify token hasn't expired more than 1 month ago
            if (v_ValidatedToken.ValidTo != DateTime.MinValue)
            {
                DateTime v_OneMonthAgo = DateTime.UtcNow.AddMonths(-1);
                if (v_ValidatedToken.ValidTo < v_OneMonthAgo)
                {
                    return null; // Token expired more than 1 month ago
                }
            }

            string v_UserIdClaim = v_Principal.FindFirst(JwtRegisteredClaimNames.Sid)?.Value;

            if (string.IsNullOrWhiteSpace(v_UserIdClaim))
                return null;

            return int.TryParse(v_UserIdClaim, out int v_UserId) ? v_UserId : null;
        }
        catch
        {
            return null;
        }
    }

    private static string GenerateRefreshToken()
    {
        byte[] v_RandomNumber = new byte[64];
        using RandomNumberGenerator v_Rng = RandomNumberGenerator.Create();
        v_Rng.GetBytes(v_RandomNumber);
        return Convert.ToBase64String(v_RandomNumber);
    }
}