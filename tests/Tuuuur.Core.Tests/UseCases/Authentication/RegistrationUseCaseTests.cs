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

namespace Tuuuur.Core.Tests.UseCases.Authentication;

public class RegistrationUseCaseTests
{
    private Mock<IUnitOfWork> m_UnitOfWorkMock = new();
    private Mock<ILogger<RegistrationUseCase>> m_LoggerMock = new();
    private Mock<IMediator> m_MediatorMock = new();
    private readonly Mock<IRenderingService> m_RenderingServiceMock = new();
    private readonly Mock<IEmailService> m_EmailServiceMock = new();

    private RegistrationUseCase m_UseCase;

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldCreateUserAsync()
    {
        // Arrange
        CancellationToken v_CancellationToken = CancellationToken.None;

        User v_User = new()
        {
            Email = "test@test.com",
            Password = "password",
            NickName = "test"
        };

        RegistrationRequest v_Request = new(v_User);

        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        m_UnitOfWorkMock.Setup(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(v_User.Email, v_CancellationToken))
                        .ReturnsAsync((User)null);

        string v_HashedPassword = "hashedPassword";
        m_MediatorMock = new Mock<IMediator>();
        m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<HashRequest>(), v_CancellationToken))
        .ReturnsAsync(new StringResponse(v_HashedPassword));

        Mock<IMappingAddEntity<User, IEntity>> v_MappingAddEntityMock = new();
        v_MappingAddEntityMock.Setup(p_C => p_C.MapBoEntity).Returns(v_User);
        
        m_UnitOfWorkMock
            .Setup(p_X => p_X.ExecutionStrategy(It.IsAny<Func<Task<EmptyResponse>>>()))
            .Callback((Func<Task<EmptyResponse>> p_Func) => p_Func())
            .Returns(Task.FromResult(new EmptyResponse()));

        m_UnitOfWorkMock.Setup(p_Uow => p_Uow.UserRepository.CreateUserAsync(v_User, v_CancellationToken))
                        .ReturnsAsync(v_MappingAddEntityMock.Object);

        m_UnitOfWorkMock.Setup(p_Uow => p_Uow.Save());

        m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<GenerateOptRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UserAuthResponse(new UserAuth()));

        m_LoggerMock = new Mock<ILogger<RegistrationUseCase>>();

        m_UseCase = new RegistrationUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_MediatorMock.Object, m_RenderingServiceMock.Object, m_EmailServiceMock.Object);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, v_CancellationToken);

        // Assert
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(v_User.Email, v_CancellationToken), Times.Once);
        m_MediatorMock.Verify(p_M => p_M.Send(It.IsAny<HashRequest>(), v_CancellationToken), Times.Once);
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.CreateUserAsync(v_User, v_CancellationToken), Times.Once);
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.Save(), Times.Once);
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
    }

    [Fact]
    public async void Handle_WhenUserEmailExists_ShouldThrowDuplicateNameExceptionAsync()
    {
        // Arrange
        CancellationToken v_CancellationToken = CancellationToken.None;

        User v_User = new()
        {
            NickName = "test1",
            Email = "test@test.com",
            Password = "password",
        };

        RegistrationRequest v_Request = new(v_User);

        User v_ExistingUser = new()
        {
            NickName = "test2",
            Email = v_User.Email,
            Password = "hashedPassword",
        };

        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        m_UnitOfWorkMock.Setup(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(v_User.Email, v_CancellationToken))
                        .ReturnsAsync(v_ExistingUser);
        
        m_UnitOfWorkMock
            .Setup(p_X => p_X.ExecutionStrategy(It.IsAny<Func<Task<EmptyResponse>>>()))
            .Callback((Func<Task<EmptyResponse>> p_Func) => p_Func())
            .Returns(Task.FromResult(new EmptyResponse([new ErrorDto(DomainErrors.Data.AlreadyExist, string.Empty)])));

        m_LoggerMock = new Mock<ILogger<RegistrationUseCase>>();
        m_MediatorMock = new Mock<IMediator>();

        m_UseCase = new RegistrationUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_MediatorMock.Object, m_RenderingServiceMock.Object, m_EmailServiceMock.Object);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, v_CancellationToken);

        // Assert
        v_Result.Success.Should().BeFalse();
        v_Result.Errors.Should().NotBeEmpty().And.Satisfy(p_P => p_P.Code == DomainErrors.Data.AlreadyExist);
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(v_User.Email, v_CancellationToken), Times.Once);
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.CreateUserAsync(It.IsAny<User>(), v_CancellationToken), Times.Never);
    }
    
    [Fact]
    public async void Handle_WhenUserNickNameExists_ShouldThrowDuplicateNameExceptionAsync()
    {
        // Arrange
        CancellationToken v_CancellationToken = CancellationToken.None;

        User v_User = new()
        {
            NickName = "test",
            Email = "test@test.com",
            Password = "password",
        };

        RegistrationRequest v_Request = new(v_User);

        User v_ExistingUser = new()
        {
            NickName = v_User.NickName,
            Email = "test." + v_User.Email,
            Password = "hashedPassword",
        };

        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        m_UnitOfWorkMock.Setup(p_Uow => p_Uow.UserRepository.GetUserByNickNameAsync(v_User.NickName, v_CancellationToken))
            .ReturnsAsync(v_ExistingUser);
        
        m_UnitOfWorkMock
            .Setup(p_X => p_X.ExecutionStrategy(It.IsAny<Func<Task<EmptyResponse>>>()))
            .Callback((Func<Task<EmptyResponse>> p_Func) => p_Func())
            .Returns(Task.FromResult(new EmptyResponse([new ErrorDto(DomainErrors.Data.AlreadyExist, string.Empty)])));
        
        m_UnitOfWorkMock.Setup(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(v_User.Email, v_CancellationToken))
            .ReturnsAsync((User)null);

        m_LoggerMock = new Mock<ILogger<RegistrationUseCase>>();
        m_MediatorMock = new Mock<IMediator>();

        m_UseCase = new RegistrationUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_MediatorMock.Object, m_RenderingServiceMock.Object, m_EmailServiceMock.Object);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, v_CancellationToken);

        // Assert
        v_Result.Success.Should().BeFalse();
        v_Result.Errors.Should().NotBeEmpty().And.Satisfy(p_P => p_P.Code == DomainErrors.Data.AlreadyExist);
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(v_User.Email, v_CancellationToken), Times.Once);
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.CreateUserAsync(It.IsAny<User>(), v_CancellationToken), Times.Never);
    }
}
