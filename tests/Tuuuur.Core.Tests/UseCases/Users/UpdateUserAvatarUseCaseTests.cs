using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Users;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Users;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.Users;

public class UpdateUserAvatarUseCaseTests
{
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock = new();
    private readonly Mock<ILogger<UpdateUserAvatarUseCase>> m_LoggerMock = new();
    private readonly Mock<IUserRoleService> m_UserRoleServiceMock = new();

    private UpdateUserAvatarUseCase m_UseCase;

    [Fact]
    public async Task Handle_ExpectedBehaviorAsync()
    {
        // Arrange
        CancellationToken v_CancellationToken = CancellationToken.None;

        User v_User = BoFactory.CreateUser();
        
        UpdateUserAvatarRequest v_Request = new("MySuperBase64Image");

        m_UnitOfWorkMock
            .Setup(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())) 
            .ReturnsAsync(v_User);
        
        m_UnitOfWorkMock
            .Setup(p_Uow => p_Uow.UserRepository.UpdateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())) 
            .Returns(Task.CompletedTask);

        m_UseCase = new UpdateUserAvatarUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object,  m_UserRoleServiceMock.Object);

        // Act
        GenericEntityResponse<User> v_Result = await m_UseCase.Handle(v_Request, v_CancellationToken);

        // Assert
        m_UnitOfWorkMock.VerifyAll();
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
        v_User.Avatar = v_Request.Avatar;
        v_Result.Value.Should().BeEquivalentTo(v_User);
    }
    
    [Fact]
    public async Task Handle_WhenUserNotExist_ExpectedBehaviorAsync()
    {
        // Arrange
        CancellationToken v_CancellationToken = CancellationToken.None;

        User v_User = BoFactory.CreateUser();
        
        UpdateUserAvatarRequest v_Request = new("MySuperBase64Image");

        m_UnitOfWorkMock
            .Setup(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())) 
            .ReturnsAsync((User)null);
        
        m_UseCase = new UpdateUserAvatarUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object,  m_UserRoleServiceMock.Object);

        // Act
        GenericEntityResponse<User> v_Result = await m_UseCase.Handle(v_Request, v_CancellationToken);

        // Assert
        m_UnitOfWorkMock.VerifyAll();
        v_Result.Success.Should().BeFalse();
        v_Result.Errors.Should().NotBeNull().And.Satisfy(p_P => p_P.Code == DomainErrors.Data.NotFound);
    }
    
    [Fact]
    public async Task Handle_WhenExceptionRaised_ExpectedBehaviorAsync()
    {
        // Arrange
        CancellationToken v_CancellationToken = CancellationToken.None;

        User v_User = BoFactory.CreateUser();
        
        UpdateUserAvatarRequest v_Request = new("MySuperBase64Image");

        m_UnitOfWorkMock
            .Setup(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())) 
            .Throws<Exception>();
        
        m_UseCase = new UpdateUserAvatarUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object,  m_UserRoleServiceMock.Object);

        // Act
        GenericEntityResponse<User> v_Result = await m_UseCase.Handle(v_Request, v_CancellationToken);

        // Assert
        m_UnitOfWorkMock.VerifyAll();
        v_Result.Success.Should().BeFalse();
        v_Result.Errors.Should().NotBeNull().And.Satisfy(p_P => p_P.Code == DomainErrors.UnknowError);
    }
}
