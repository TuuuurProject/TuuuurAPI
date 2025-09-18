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
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Emails;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Domain.Token;

namespace Tuuuur.Core.Tests.UseCases.Authentication;

public class ForgotPasswordUseCaseTests
{
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ILogger<ForgotPasswordUseCase>> m_LoggerMock;
    private readonly Mock<IJwtFactory> m_JwtFactoryMock;
    private readonly Mock<IMediator> m_MediatorMock;
    private readonly Mock<IEmailService> m_EmailServiceMock;
    private readonly Mock<IRenderingService> m_RenderingServiceMock;

    private readonly ForgotPasswordUseCase m_UseCase;
    
    public ForgotPasswordUseCaseTests()
    {
        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        m_LoggerMock = new Mock<ILogger<ForgotPasswordUseCase>>();
        m_JwtFactoryMock = new Mock<IJwtFactory>();
        m_MediatorMock = new Mock<IMediator>();
        m_EmailServiceMock = new Mock<IEmailService>();
        m_RenderingServiceMock = new Mock<IRenderingService>();

        m_UseCase = new ForgotPasswordUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_MediatorMock.Object, m_RenderingServiceMock.Object,  m_EmailServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUseExist_ShouldCreateUserAsync()
    {
        // Arrange
        const string v_Email = "test@test.com";

        User v_User = new() { Email = v_Email };
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailOrNickNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_User);
        
        m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<GenerateOptRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UserAuthResponse(new UserAuth()));

        ForgotPasswordRequest v_Request = new(v_Email);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.True(v_Result.Success);

        // Assert
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.GetUserByEmailOrNickNameAsync(v_User.Email, It.IsAny<CancellationToken>()), Times.Once);
        m_MediatorMock.Verify(p_M => p_M.Send(It.IsAny<GenerateOptRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
    }
}