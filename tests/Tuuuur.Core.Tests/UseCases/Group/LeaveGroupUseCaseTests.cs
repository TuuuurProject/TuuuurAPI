
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Group;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.Group;

public class LeaveGroupUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<IUserRoleService> m_UserRoleServiceMock;
    private readonly Mock<IGroupNotificationService> m_GroupPartyNotificationServiceMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;

    private readonly LeaveGroupUseCase m_UseCase;

    public LeaveGroupUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_UnitOfWorkMock = m_MockRepository.Create<IUnitOfWork>();
        Mock<ILogger<LeaveGroupUseCase>> v_LoggerMock = m_MockRepository.Create<ILogger<LeaveGroupUseCase>>();
        m_UserRoleServiceMock = m_MockRepository.Create<IUserRoleService>();
        m_GroupPartyNotificationServiceMock = m_MockRepository.Create<IGroupNotificationService>();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();

        m_UseCase = new LeaveGroupUseCase(m_UnitOfWorkMock.Object, v_LoggerMock.Object, m_UserRoleServiceMock.Object,
            m_GroupPartyNotificationServiceMock.Object, m_CacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ExpectedAsync()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        Party v_Party = BoFactory.CreateParty().Generate();
        m_UserRoleServiceMock.Setup(p_U => p_U.GetCurrentUserEmail()).Returns(v_User.Email);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<Guid?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party.Id);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<Guid>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party.Id);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<Party>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetMembersAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([v_User.Id, v_User.Id + 1]);
        m_CacheServiceMock
            .Setup(p_Cs => p_Cs.SetRemoveAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        m_GroupPartyNotificationServiceMock
            .Setup(p_Ns => p_Ns.NotifyPlayerLeftAsync(It.IsAny<string>(), It.IsAny<User>()))
            .Returns(Task.CompletedTask);


        LeaveGroupPartyRequest v_Request = new();

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.True(v_Result.Success);

        // Assert
        m_UnitOfWorkMock.Verify(
            p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(v_User.Email, It.IsAny<CancellationToken>()), Times.Once);
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
        m_MockRepository.VerifyAll();
    }
}