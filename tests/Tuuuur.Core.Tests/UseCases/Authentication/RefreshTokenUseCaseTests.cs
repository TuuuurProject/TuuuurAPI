using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Core.UseCases.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Repositories;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Domain.Security;
using Tuuuur.Domain.Token;

namespace Tuuuur.Core.Tests.UseCases.Authentication;

public class RefreshTokenUseCaseTests
{
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ILogger<RefreshTokenUseCase>> m_LoggerMock;
    private readonly Mock<IJwtFactory> m_JwtFactoryMock;
    private readonly Mock<IRefreshTokenRepository> m_RefreshTokenRepositoryMock;
    private readonly Mock<IUserRepository> m_UserRepositoryMock;
    private readonly Mock<IUserRoleService> m_UserRoleServiceMock;
    private readonly RefreshTokenUseCase m_UseCase;

    public RefreshTokenUseCaseTests()
    {
        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        m_LoggerMock = new Mock<ILogger<RefreshTokenUseCase>>();
        m_JwtFactoryMock = new Mock<IJwtFactory>();
        m_RefreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        m_UserRepositoryMock = new Mock<IUserRepository>();
        m_UserRoleServiceMock = new Mock<IUserRoleService>();

        m_UnitOfWorkMock.Setup(p_U => p_U.RefreshTokenRepository).Returns(m_RefreshTokenRepositoryMock.Object);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository).Returns(m_UserRepositoryMock.Object);

        m_UseCase = new RefreshTokenUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_JwtFactoryMock.Object, m_UserRoleServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenIsValid_ShouldReturnNewTokensAsync()
    {
        // Arrange
        const string v_RefreshTokenString = "valid-refresh-token";
        User v_User = new() { Id = 1, Email = "test@test.com", NickName = "test", IsGoogleUser = false };
        RefreshToken v_RefreshToken = new()
        {
            Id = 1,
            UserId = v_User.Id,
            Token = v_RefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        JwtTokenResponse v_NewTokenResponse = new()
        {
            Token = "new-access-token",
            RefreshToken = "new-refresh-token",
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(90)
        };

        m_RefreshTokenRepositoryMock.Setup(p_R => p_R.GetRefreshTokenByTokenAsync(v_RefreshTokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_RefreshToken);
        m_UserRepositoryMock.Setup(p_U => p_U.GetUserByIdAsync(v_User.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);
        m_UserRoleServiceMock.Setup(p_S => p_S.GetCurrentUserEmail()).Returns(v_User.Email);
        m_JwtFactoryMock.Setup(p_J => p_J.CreateTokenAsync(v_User, It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_NewTokenResponse);

        RefreshTokenRequest v_Request = new(v_RefreshTokenString);

        // Act
        JwtAuthenticationResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.True(v_Result.Success);
        Assert.NotNull(v_Result.Value);
        Assert.Equal(v_User, v_Result.Value.User);
        Assert.Equal(v_NewTokenResponse, v_Result.Value.Token);
        Assert.Equal(v_User.IsGoogleUser, v_Result.Value.IsGoogleUser);

        // Vérifier que l'ancien token a été révoqué
        m_RefreshTokenRepositoryMock.Verify(p_R => p_R.UpdateRefreshTokenAsync(
            It.Is<RefreshToken>(rt => rt.IsRevoked && rt.RevokedAt != null),
            It.IsAny<CancellationToken>()), Times.Once);

        // Vérifier que Save a été appelé
        m_UnitOfWorkMock.Verify(p_U => p_U.Save(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenDoesNotExist_ShouldReturnErrorAsync()
    {
        // Arrange
        const string v_RefreshTokenString = "invalid-token";

        m_RefreshTokenRepositoryMock.Setup(p_R => p_R.GetRefreshTokenByTokenAsync(v_RefreshTokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken)null);

        RefreshTokenRequest v_Request = new(v_RefreshTokenString);

        // Act
        JwtAuthenticationResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.False(v_Result.Success);
        Assert.NotNull(v_Result.Errors);
        Assert.Single(v_Result.Errors);
        Assert.Equal(DomainErrors.Authentication.RefreshToken.Invalid, v_Result.Errors.First().Code);
        Assert.Contains("Invalid refresh token", v_Result.Errors.First().Description);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenIsRevoked_ShouldReturnErrorAsync()
    {
        // Arrange
        const string v_RefreshTokenString = "revoked-token";
        RefreshToken v_RefreshToken = new()
        {
            Id = 1,
            UserId = 1,
            Token = v_RefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = true,
            RevokedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        m_RefreshTokenRepositoryMock.Setup(p_R => p_R.GetRefreshTokenByTokenAsync(v_RefreshTokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_RefreshToken);

        RefreshTokenRequest v_Request = new(v_RefreshTokenString);

        // Act
        JwtAuthenticationResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.False(v_Result.Success);
        Assert.NotNull(v_Result.Errors);
        Assert.Single(v_Result.Errors);
        Assert.Equal(DomainErrors.Authentication.RefreshToken.Invalid, v_Result.Errors.First().Code);
        Assert.Contains("revoked", v_Result.Errors.First().Description);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenIsExpired_ShouldReturnErrorAsync()
    {
        // Arrange
        const string v_RefreshTokenString = "expired-token";
        RefreshToken v_RefreshToken = new()
        {
            Id = 1,
            UserId = 1,
            Token = v_RefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expiré hier
            CreatedAt = DateTime.UtcNow.AddDays(-91),
            IsRevoked = false
        };

        m_RefreshTokenRepositoryMock.Setup(p_R => p_R.GetRefreshTokenByTokenAsync(v_RefreshTokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_RefreshToken);

        RefreshTokenRequest v_Request = new(v_RefreshTokenString);

        // Act
        JwtAuthenticationResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.False(v_Result.Success);
        Assert.NotNull(v_Result.Errors);
        Assert.Single(v_Result.Errors);
        Assert.Equal(DomainErrors.Authentication.RefreshToken.Invalid, v_Result.Errors.First().Code);
        Assert.Contains("expired", v_Result.Errors.First().Description);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnErrorAsync()
    {
        // Arrange
        const string v_RefreshTokenString = "valid-token";
        RefreshToken v_RefreshToken = new()
        {
            Id = 1,
            UserId = 999,
            Token = v_RefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        m_RefreshTokenRepositoryMock.Setup(p_R => p_R.GetRefreshTokenByTokenAsync(v_RefreshTokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_RefreshToken);
        m_UserRepositoryMock.Setup(p_U => p_U.GetUserByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        RefreshTokenRequest v_Request = new(v_RefreshTokenString);

        // Act
        JwtAuthenticationResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.False(v_Result.Success);
        Assert.NotNull(v_Result.Errors);
        Assert.Single(v_Result.Errors);
        Assert.Equal(DomainErrors.Data.NotFound, v_Result.Errors.First().Code);
        Assert.Contains("User not found", v_Result.Errors.First().Description);
    }
}
