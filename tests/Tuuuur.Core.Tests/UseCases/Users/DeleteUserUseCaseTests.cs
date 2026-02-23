

using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Requests.Users;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Users;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.Users;

public class DeleteUserUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<ILogger<DeleteUserUseCase>> m_MockLogger;
    private readonly Mock<IUnitOfWork> m_MockUnitOfWork;
    private readonly Mock<IUserRoleService> m_MockUserRoleServiceMock;
    

    public DeleteUserUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_MockUnitOfWork = m_MockRepository.Create<IUnitOfWork>();
        m_MockLogger = m_MockRepository.Create<ILogger<DeleteUserUseCase>>();
        m_MockUserRoleServiceMock = m_MockRepository.Create<IUserRoleService>();
    }

    private DeleteUserUseCase CreateUseCase()
    {
        return new DeleteUserUseCase(
            m_MockUnitOfWork.Object,
            m_MockLogger.Object,
            m_MockUserRoleServiceMock.Object
        );
    }
    
    [Fact]
    public async Task Handle_ExpectedBehaviorAsync()
    {
        // Arrange
        CancellationToken v_CancellationToken = CancellationToken.None;
        DeleteUserUseCase v_UseCase = CreateUseCase();

        User v_User = BoFactory.CreateUser();
        
        DeleteUserRequest v_Request = new();

        m_MockUserRoleServiceMock.Setup(p_Urs => p_Urs.GetCurrentUserEmail()).Returns(v_User.Email);
        
        m_MockUnitOfWork
            .Setup(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())) 
            .ReturnsAsync(v_User);

        m_MockUnitOfWork
            .Setup(p_Uow => p_Uow.UserRepository.DeleteUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())) 
            .Returns(Task.CompletedTask);
        
        m_MockUnitOfWork
            .Setup(p_Uow => p_Uow.Save()) 
            .Returns(1);
        
        // Act
        EmptyResponse v_Result = await v_UseCase.Handle(v_Request, v_CancellationToken);

        // Assert
        m_MockRepository.VerifyAll();
        
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
    }
    
    [Fact]
    public async Task Handle_WhenUserNotExist_ExpectedBehaviorAsync()
    {
        // Arrange
        CancellationToken v_CancellationToken = CancellationToken.None;
        DeleteUserUseCase v_UseCase = CreateUseCase();

        User v_User = BoFactory.CreateUser();
        
        DeleteUserRequest v_Request = new();

        m_MockUserRoleServiceMock.Setup(p_Urs => p_Urs.GetCurrentUserEmail()).Returns(v_User.Email);
        
        m_MockUnitOfWork
            .Setup(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())) 
            .ReturnsAsync((User)null);
        
        // Act
        EmptyResponse v_Result = await v_UseCase.Handle(v_Request, v_CancellationToken);

        // Assert
        m_MockRepository.VerifyAll();
        
        
        v_Result.Success.Should().BeFalse();
        v_Result.Errors.Should().NotBeNull().And.Satisfy(p_P => p_P.Code == DomainErrors.Data.NotFound);
    }
}
