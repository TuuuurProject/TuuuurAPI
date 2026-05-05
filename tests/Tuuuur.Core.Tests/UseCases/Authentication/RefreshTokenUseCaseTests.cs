using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Core.UseCases.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Repositories;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Domain.Token;

namespace Tuuuur.Core.Tests.UseCases.Authentication;

public class RefreshTokenUseCaseTests
{
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ILogger<RefreshTokenUseCase>> m_LoggerMock;
    private readonly Mock<IJwtFactory> m_JwtFactoryMock;
    private readonly Mock<IRefreshTokenRepository> m_RefreshTokenRepositoryMock;
    private readonly Mock<IUserRepository> m_UserRepositoryMock;
    private readonly RefreshTokenUseCase m_UseCase;

    public RefreshTokenUseCaseTests()
    {
        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        m_LoggerMock = new Mock<ILogger<RefreshTokenUseCase>>();
        m_JwtFactoryMock = new Mock<IJwtFactory>();
        m_RefreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        m_UserRepositoryMock = new Mock<IUserRepository>();

        m_UnitOfWorkMock.Setup(p_U => p_U.RefreshTokenRepository).Returns(m_RefreshTokenRepositoryMock.Object);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository).Returns(m_UserRepositoryMock.Object);

        m_UseCase = new RefreshTokenUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_JwtFactoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenIsValid_ShouldReturnNewTokensAsync()
    {
        // Arrange
        const string v_RefreshTokenString = "valid-refresh-token";
        const string v_BearerToken = "valid-bearer-token";
        User v_User = new() { Id = Guid.NewGuid(), Email = "test@test.com", NickName = "test", IsGoogleUser = false };
        RefreshToken v_RefreshToken = new()
        {
            UserId = v_User.Id,
            Token = v_RefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
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
        m_JwtFactoryMock.Setup(p_J => p_J.GetUserIdFromToken(v_BearerToken))
            .Returns(v_User.Id);
        m_JwtFactoryMock.Setup(p_J => p_J.CreateTokenAsync(v_User, It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_NewTokenResponse);

        RefreshTokenRequest v_Request = new(v_BearerToken, v_RefreshTokenString);

        // Act
        JwtAuthenticationResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.True(v_Result.Success);
        Assert.NotNull(v_Result.Value);
        Assert.Equal(v_User, v_Result.Value.User);
        Assert.Equal(v_NewTokenResponse, v_Result.Value.Token);
        Assert.Equal(v_User.IsGoogleUser, v_Result.Value.IsGoogleUser);

        m_RefreshTokenRepositoryMock.Verify(p_R => p_R.DeleteRefreshTokenAsync(
            v_RefreshTokenString,
            It.IsAny<CancellationToken>()), Times.Once);

        m_UnitOfWorkMock.Verify(p_U => p_U.Save(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenDoesNotExist_ShouldReturnErrorAsync()
    {
        // Arrange
        const string v_RefreshTokenString = "invalid-token";
        const string v_BearerToken = "some-bearer-token";

        m_RefreshTokenRepositoryMock.Setup(p_R => p_R.GetRefreshTokenByTokenAsync(v_RefreshTokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken)null);
        m_JwtFactoryMock.Setup(p_J => p_J.GetUserIdFromToken(v_BearerToken))
            .Returns(Guid.NewGuid());

        RefreshTokenRequest v_Request = new(v_BearerToken, v_RefreshTokenString);

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
    public async Task Handle_WhenRefreshTokenIsExpired_ShouldReturnErrorAsync()
    {
        // Arrange
        const string v_RefreshTokenString = "expired-token";
        const string v_BearerToken = "some-bearer-token";
        RefreshToken v_RefreshToken = new()
        {
            UserId = Guid.NewGuid(),
            Token = v_RefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-91)
        };

        m_RefreshTokenRepositoryMock.Setup(p_R => p_R.GetRefreshTokenByTokenAsync(v_RefreshTokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_RefreshToken);
        m_JwtFactoryMock.Setup(p_J => p_J.GetUserIdFromToken(v_BearerToken))
            .Returns(Guid.NewGuid());

        RefreshTokenRequest v_Request = new(v_BearerToken, v_RefreshTokenString);

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
        const string v_BearerToken = "some-bearer-token";
        RefreshToken v_RefreshToken = new()
        {
            UserId = Guid.NewGuid(),
            Token = v_RefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        m_RefreshTokenRepositoryMock.Setup(p_R => p_R.GetRefreshTokenByTokenAsync(v_RefreshTokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_RefreshToken);
        m_UserRepositoryMock.Setup(p_U => p_U.GetUserByIdAsync(v_RefreshToken.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);
        m_JwtFactoryMock.Setup(p_J => p_J.GetUserIdFromToken(v_BearerToken))
            .Returns(v_RefreshToken.UserId);

        RefreshTokenRequest v_Request = new(v_BearerToken, v_RefreshTokenString);

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
