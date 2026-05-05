using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;

namespace Tuuuur.Core.Tests.UseCases.Authentication;

public class ResetPasswordUseCaseTests
{
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<IMediator> m_MediatorMock;

    private readonly ResetPasswordUseCase m_UseCase;
    
    public ResetPasswordUseCaseTests()
    {
        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        Mock<ILogger<ResetPasswordUseCase>> v_LoggerMock = new();
        m_MediatorMock = new Mock<IMediator>();

        m_UseCase = new ResetPasswordUseCase(m_UnitOfWorkMock.Object, v_LoggerMock.Object, m_MediatorMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUseExist_ShouldCreateUserAsync()
    {
        // Arrange
        const string v_Email = "test@test.com";
        const string v_Code = "987654";
        const string v_Password = "MySuperPassw0rd_";

        User v_User = new() { Email = v_Email };
        UserAuth v_UserAuth = new();
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailOrNickNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_User);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserAuthRepository.GetUserAuthByUserIdAndCodeAsync(It.IsAny<Guid>(),It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_UserAuth);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.UpdateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Verifiable();
        m_UnitOfWorkMock.Setup(p_U => p_U.UserAuthRepository.DeleteUserAuthAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).Verifiable();
        m_UnitOfWorkMock.Setup(p_U => p_U.Save());
        
        m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<HashRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new StringResponse(v_Code));

        ResetPasswordRequest v_Request = new(v_Email, v_Password, v_Code);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.GetUserByEmailOrNickNameAsync(v_User.Email, It.IsAny<CancellationToken>()), Times.Once);
        m_MediatorMock.Verify(p_M => p_M.Send(It.IsAny<HashRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
    }
}