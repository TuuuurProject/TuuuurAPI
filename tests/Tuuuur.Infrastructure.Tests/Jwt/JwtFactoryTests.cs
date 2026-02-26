using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;
using Tuuuur.Domain.Token;
using Tuuuur.Infrastructure.Jwt;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Infrastructure.Tests.Jwt;

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
            Id = Guid.NewGuid(),
            NickName = "test",
            Email = "test@example.com",
            IsAdmin = false
        };

        Claim[] v_ExpectedClaims = {
            new(ClaimTypes.NameIdentifier, v_User.NickName),
            new(ClaimTypes.Email, v_User.Email),
            new(JwtRegisteredClaimNames.Email, v_User.Email),
            new(ClaimTypes.Role, RolesType.User)
        };
        
        RefreshToken v_RefreshToken = new()
        {
            UserId = v_User.Id,
            Token = "new-token-67890",
            ExpiresAt = DateTime.UtcNow.AddDays(90),
            CreatedAt = DateTime.UtcNow
        };
        
        Mock<IMappingAddEntity<RefreshToken, IEntity>> v_MappingAddEntityMock = new();
        v_MappingAddEntityMock.Setup(p_P => p_P.BoEntity).Returns(v_RefreshToken);
        
        JwtFactory v_JwtFactory = new(m_JwtConfiguration);
        Mock<IUnitOfWork> v_UnitOfWorkMock = new();
        v_UnitOfWorkMock.Setup(p_U => p_U.RefreshTokenRepository.CreateRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_MappingAddEntityMock.Object);

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

    [Fact]
    public void CreateAnonymousTokenAsync_ExpectedBehavior()
    {
        User v_User = BoFactory.CreateUser();
        
        JwtFactory v_JwtFactory = new(m_JwtConfiguration);
        JwtTokenResponse v_AnonymousToken = v_JwtFactory.CreateAnonymousTokenAsync(v_User);
        
        Check.That(v_AnonymousToken).IsNotNull();
    }
    
    [Fact]
    public async Task GetUserIdFromToken_ExpectedBehavior()
    {
        User v_User = BoFactory.CreateUser();
        RefreshToken v_RefreshToken = new()
        {
            UserId = v_User.Id,
            Token = "new-token-67890",
            ExpiresAt = DateTime.UtcNow.AddDays(90),
            CreatedAt = DateTime.UtcNow
        };
        
        Mock<IMappingAddEntity<RefreshToken, IEntity>> v_MappingAddEntityMock = new();
        v_MappingAddEntityMock.Setup(p_P => p_P.BoEntity).Returns(v_RefreshToken);
        JwtFactory v_JwtFactory = new(m_JwtConfiguration);
        Mock<IUnitOfWork> v_UnitOfWorkMock = new();
        v_UnitOfWorkMock.Setup(p_U => p_U.RefreshTokenRepository.CreateRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_MappingAddEntityMock.Object);

        // Act
        JwtTokenResponse v_Result = await v_JwtFactory.CreateTokenAsync(v_User, v_UnitOfWorkMock.Object, CancellationToken.None);
        
        Guid? v_UserIdFromToken = v_JwtFactory.GetUserIdFromToken(v_Result.Token);
        Check.That(v_UserIdFromToken).IsNotNull();
        Check.That(v_UserIdFromToken).Equals(v_User.Id);
    }
}
