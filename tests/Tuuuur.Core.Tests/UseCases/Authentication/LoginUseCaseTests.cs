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
using Tuuuur.Domain.Interfaces.Emails;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Domain.Token;

namespace Tuuuur.Core.Tests.UseCases.Authentication
{
    public class LoginUseCaseTests
    {
        private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
        private readonly Mock<ILogger<LoginUseCase>> m_LoggerMock;
        private readonly Mock<IJwtFactory> m_JwtFactoryMock;
        private readonly Mock<IMediator> m_MediatorMock;
        private readonly Mock<IEmailService> m_EmailServiceMock;
        private readonly Mock<IRenderingService> m_RenderingServiceMock;
        private readonly LoginUseCase m_UseCase;

        public LoginUseCaseTests()
        {
            m_UnitOfWorkMock = new Mock<IUnitOfWork>();
            m_LoggerMock = new Mock<ILogger<LoginUseCase>>();
            m_JwtFactoryMock = new Mock<IJwtFactory>();
            m_MediatorMock = new Mock<IMediator>();
            m_EmailServiceMock = new Mock<IEmailService>();
            m_RenderingServiceMock = new Mock<IRenderingService>();

            m_UseCase = new LoginUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_MediatorMock.Object, m_RenderingServiceMock.Object,  m_EmailServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenUserExistsAndCredentialsAreValid_ShouldReturnUserTokenAsync()
        {
            // Arrange
            const string v_Email = "test@test.com";
            const string v_Password = "password123";
            const string v_HashedPassword = "hashedPassword123";

            User v_User = new User { Email = v_Email, Password = v_HashedPassword };
            m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailOrNickNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_User);
            m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<HashRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new StringResponse(v_HashedPassword));

            m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<GenerateOptRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GenericEntityResponse<UserAuth>(new UserAuth()));

            JwtTokenResponse v_JwtTokenResponse = new JwtTokenResponse();
            m_JwtFactoryMock.Setup(p_J => p_J.CreateToken(v_User)).Returns(v_JwtTokenResponse);

            LoginRequest v_Request = new(v_Email, v_Password);

            // Act
            EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

            // Assert
            Assert.NotNull(v_Result);
            Assert.True(v_Result.Success);
        }

        [Fact]
        public async Task Handle_WhenUserDoesNotExist_ShouldReturnNullAsync()
        {
            // Arrange
            const string v_Email = "test@test.com";
            const string v_Password = "password123";

            m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(v_Email, default)).ReturnsAsync((User)null);
            m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<HashRequest>(), default)).ReturnsAsync(new StringResponse("hashedPassword123"));

            LoginRequest v_Request = new LoginRequest(v_Email, v_Password);

            // Act
            var v_Result = await m_UseCase.Handle(v_Request, default);

            // Assert
            Assert.False(v_Result.Success);
            Assert.Equal(new ErrorDto(DomainErrors.Authentication.Invalid, "Invalid login and/or password"), v_Result.Errors.First());
        }

        [Fact]
        public async Task Handle_WhenUserExistsButCredentialsAreInvalid_ShouldReturnNullAsync()
        {
            // Arrange
            const string v_Email = "test@test.com";
            const string v_Password = "password123";
            const string v_HashedPassword = "hashedPassword123";

            User v_User = new User { Email = v_Email, Password = v_HashedPassword };
            m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(v_Email, default)).ReturnsAsync(v_User);
            m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<HashRequest>(), default)).ReturnsAsync(new StringResponse("invalidHashedPassword123"));

            LoginRequest v_Request = new LoginRequest(v_Email, v_Password);

            // Act
            var v_Result = await m_UseCase.Handle(v_Request, default);

            // Assert
            Assert.False(v_Result.Success);
            Assert.Equal(new ErrorDto(DomainErrors.Authentication.Invalid, "Invalid login and/or password"), v_Result.Errors.First());
        }
    }
}