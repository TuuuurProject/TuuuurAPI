using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;

namespace Tuuuur.Core.Tests.UseCases.Authentication;

public class RegistrationUseCaseTests
{
    private Mock<IUnitOfWork> m_UnitOfWorkMock;
    private Mock<ILogger<RegistrationUseCase>> m_LoggerMock;
    private Mock<IMediator> m_MediatorMock;

    private RegistrationUseCase m_UseCase;

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldCreateUserAsync()
    {
        // Arrange
        CancellationToken v_CancellationToken = new();

        User v_User = new()
        {
            Email = "test@test.com",
            Password = "password",
            FirstName = "John",
            LastName = "Doe"
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

        m_UnitOfWorkMock.Setup(p_Uow => p_Uow.UserRepository.CreateUserAsync(v_User, v_CancellationToken))
                        .ReturnsAsync(v_MappingAddEntityMock.Object);

        m_UnitOfWorkMock.Setup(p_Uow => p_Uow.Save());

        m_LoggerMock = new Mock<ILogger<RegistrationUseCase>>();

        m_UseCase = new RegistrationUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_MediatorMock.Object);

        // Act
        UserResponse v_Result = await m_UseCase.Handle(v_Request, v_CancellationToken);

        // Assert
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(v_User.Email, v_CancellationToken), Times.Once);
        m_MediatorMock.Verify(p_M => p_M.Send(It.IsAny<HashRequest>(), v_CancellationToken), Times.Once);
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.CreateUserAsync(v_User, v_CancellationToken), Times.Once);
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.Save(), Times.Once);

        Check.That(v_Result.Value).IsEqualTo(v_MappingAddEntityMock.Object.MapBoEntity);
    }

    [Fact]
    public async void Handle_WhenUserExists_ShouldThrowDuplicateNameExceptionAsync()
    {
        // Arrange
        CancellationToken v_CancellationToken = new();

        User v_User = new()
        {
            Email = "test@test.com",
            Password = "password",
            FirstName = "John",
            LastName = "Doe"
        };

        RegistrationRequest v_Request = new(v_User);

        User v_ExistingUser = new()
        {
            Email = v_User.Email,
            Password = "hashedPassword",
            FirstName = "Jane",
            LastName = "Doe"
        };

        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        m_UnitOfWorkMock.Setup(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(v_User.Email, v_CancellationToken))
                        .ReturnsAsync(v_ExistingUser);

        m_LoggerMock = new Mock<ILogger<RegistrationUseCase>>();
        m_MediatorMock = new Mock<IMediator>();

        m_UseCase = new RegistrationUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_MediatorMock.Object);

        // Act
        UserResponse v_Result = await m_UseCase.Handle(v_Request, v_CancellationToken);

        // Assert
        v_Result.Success.Should().BeFalse();
        v_Result.Errors.Should().NotBeEmpty().And.Satisfy(p_P => p_P.Code == DomainErrors.Data.AlreadyExist);
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(v_User.Email, v_CancellationToken), Times.Once);
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.CreateUserAsync(It.IsAny<User>(), v_CancellationToken), Times.Never);
    }
}
