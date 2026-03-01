using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Configuration;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Authentication.Google;
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
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.Authentication
{
    public class GoogleAuthentificationUseCaseTests
    {
        private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
        private readonly Mock<IJwtFactory> m_JwtFactoryMock;
        private readonly GoogleAuthentificationUseCase m_UseCase;

        public GoogleAuthentificationUseCaseTests()
        {
            m_UnitOfWorkMock = new Mock<IUnitOfWork>();
            Mock<ILogger<GoogleAuthentificationUseCase>> v_LoggerMock = new();
            m_JwtFactoryMock = new Mock<IJwtFactory>();

            m_UseCase = new GoogleAuthentificationUseCase(m_UnitOfWorkMock.Object, v_LoggerMock.Object, m_JwtFactoryMock.Object, new RankedConfiguration());
        }

        [Fact]
        public async Task Handle_WhenUserExists_ShouldReturnJwtTokenAsync()
        {
            // Arrange
            const string v_Email = "test@test.com";
            GoogleAuthentificationRequest v_Request = new(v_Email);
            User v_User = BoFactory.CreateUser();
            v_User.IsGoogleUser = true;

            m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_User);

            JwtTokenResponse v_JwtTokenResponse = new();
            m_JwtFactoryMock.Setup(p_J => p_J.CreateTokenAsync(It.IsAny<User>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_JwtTokenResponse);

            // Act
            JwtAuthenticationResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

            // Assert
            Assert.NotNull(v_Result);
            Assert.True(v_Result.Success);
        }

        [Fact]
        public async Task Handle_WhenUserNotExists_ShouldCreateUserAndReturnJwtTokenAsync()
        {
            // Arrange
            const string v_Email = "test@test.com";
            GoogleAuthentificationRequest v_Request = new(v_Email);
            User v_User = BoFactory.CreateUser();
            v_User.IsGoogleUser = true;

            m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.CreateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            m_UnitOfWorkMock.Setup(p_U => p_U.Save())
                .Returns(1)
                .Verifiable();

            m_UnitOfWorkMock.Setup(p_U => p_U.ThemeRepository.GetAllThemesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Theme>());

            JwtTokenResponse v_JwtTokenResponse = new();
            m_JwtFactoryMock.Setup(p_J => p_J.CreateTokenAsync(It.IsAny<User>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_JwtTokenResponse);

            // Act
            JwtAuthenticationResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

            // Assert
            Assert.NotNull(v_Result);
            Assert.True(v_Result.Success);
            m_UnitOfWorkMock.Verify(p_U => p_U.UserRepository.CreateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
            m_UnitOfWorkMock.Verify(p_U => p_U.Save(), Times.Once);
        }
    }
}