using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Core.UseCases.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Domain.Token;

namespace Tuuuur.Core.Tests.UseCases.Authentication;

public class VerifyAccountUseCaseTests
{
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ILogger<VerifyAccountUseCase>> m_LoggerMock;
    private readonly Mock<IJwtFactory> m_JwtFactoryMock;
    private readonly VerifyAccountUseCase m_UseCase;

    public VerifyAccountUseCaseTests()
    {
        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        m_LoggerMock = new Mock<ILogger<VerifyAccountUseCase>>();
        m_JwtFactoryMock = new Mock<IJwtFactory>();
        m_UseCase = new VerifyAccountUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_JwtFactoryMock.Object);
    }
    
    [Fact]
    public async Task Handle_WhenUserExistsAndCredentialsAreValid_ShouldReturnUserTokenAsync()
    {
        // Arrange
        const string v_Login = "test@test.com";
        const string v_Code = "357654";

        User v_User = new(){ Email = v_Login };
        UserAuth v_UserAuth = new(){ User = v_User, Code = v_Code };
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailOrNickNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_User);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserAuthRepository.GetUserAuthByUserIdAndCodeAsync(It.IsAny<int>(), It.IsAny<string>() , It.IsAny<CancellationToken>())).ReturnsAsync(v_UserAuth);
        
        JwtTokenResponse v_JwtTokenResponse = new();
        m_JwtFactoryMock.Setup(p_J => p_J.CreateToken(v_User)).Returns(v_JwtTokenResponse);

        VerifyAccountRequest v_Request = new(v_Login, v_Code);

        // Act
        JwtAuthenticationResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.True(v_Result.Success);
        Assert.Equal(v_User, v_Result.Value.User);
        Assert.Equal(v_JwtTokenResponse, v_Result.Value.Token);
    }
    
    [Fact]
    public async Task Handle_WhenUserNotExistsAndCredentialsAreValid_ShouldReturnUserTokenAsync()
    {
        // Arrange
        const string v_Login = "test@test.com";
        const string v_Code = "357654";

        User v_User = new(){ Email = v_Login };
        UserAuth v_UserAuth = new(){ User = v_User, Code = v_Code };
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailOrNickNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User)null);

        VerifyAccountRequest v_Request = new(v_Login, v_Code);

        // Act
        JwtAuthenticationResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.False(v_Result.Success);
        Assert.Equal(DomainErrors.Data.NotFound, v_Result.Errors.First().Code);
    }
}
