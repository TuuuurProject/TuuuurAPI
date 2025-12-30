using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;
using Tuuuur.Domain.Token;
using Tuuuur.Infrastructure.Jwt;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Tuuuur.Infrastructure.Tests.Jwt
{
    public class JwtFactoryTests
    {
        private readonly Mock<ILogger<JwtFactory>> m_LoggerMock;
        private readonly JwtConfiguration m_JwtConfiguration;

        public JwtFactoryTests()
        {
            m_LoggerMock = new Mock<ILogger<JwtFactory>>();
            m_JwtConfiguration = new JwtConfiguration
            {
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                Key = "TestKeyTestKeyTestKeyTestKey",
                Validity = 60
            };
        }

        [Fact]
        public async Task CreateToken_ReturnsValidJwtTokenResponse()
        {
            // Arrange
            User v_User = new()
            {
                Id = 1,
                NickName = "test",
                Email = "test@example.com",
                IsAdmin = true
            };

            Claim[] v_ExpectedClaims = {
                new(ClaimTypes.NameIdentifier, v_User.NickName),
                new(ClaimTypes.Email, v_User.Email),
                new(JwtRegisteredClaimNames.Email, v_User.Email),
                new(ClaimTypes.Role, RolesType.Admin)
            };

            JwtFactory v_JwtFactory = new(m_JwtConfiguration);
            Mock<IUnitOfWork> v_UnitOfWorkMock = new();
            v_UnitOfWorkMock.Setup(p_U => p_U.RefreshTokenRepository.CreateRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken rt, CancellationToken ct) => rt);

            // Act
            JwtTokenResponse v_Result = await v_JwtFactory.CreateTokenAsync(v_User, v_UnitOfWorkMock.Object, CancellationToken.None);

            // Assert
            Check.That(v_Result).IsNotNull();
            Check.That(v_Result.Token).IsNotEmpty();
            Check.That(v_Result.RefreshToken).IsNotEmpty();
            Check.That(v_Result.ValidFrom).IsBeforeOrEqualTo(DateTime.UtcNow);
            Check.That(v_Result.ValidTo).IsAfter(DateTime.UtcNow);
            Check.That(v_Result.RefreshTokenExpiresAt).IsAfter(DateTime.UtcNow);

            JwtSecurityTokenHandler v_Handler = new();
            JwtSecurityToken v_DecodedToken = v_Handler.ReadJwtToken(v_Result.Token);
            Check.That(v_DecodedToken.Issuer).IsEqualTo(m_JwtConfiguration.Issuer);
            Check.That(v_DecodedToken.Audiences).Contains(m_JwtConfiguration.Audience);

            foreach (Claim v_ExpectedClaim in v_ExpectedClaims)
                Check.That(v_DecodedToken.Claims.Any(p_X => p_X.Value == v_ExpectedClaim.Value)).IsTrue();

            // Vérifier que le refresh token a été sauvegardé
            v_UnitOfWorkMock.Verify(p_U => p_U.RefreshTokenRepository.CreateRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}