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

public class JoinGroupUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ILogger<JoinGroupUseCase>> m_LoggerMock;
    private readonly Mock<IUserRoleService> m_UserRoleServiceMock;
    private readonly Mock<IGroupNotificationService> m_GroupPartyNotificationServiceMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;

    private readonly JoinGroupUseCase m_UseCase;

    public JoinGroupUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_UnitOfWorkMock = m_MockRepository.Create<IUnitOfWork>();
        m_LoggerMock = m_MockRepository.Create<ILogger<JoinGroupUseCase>>();
        m_UserRoleServiceMock = m_MockRepository.Create<IUserRoleService>();
        m_GroupPartyNotificationServiceMock = m_MockRepository.Create<IGroupNotificationService>();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();

        m_UseCase = new JoinGroupUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_UserRoleServiceMock.Object, m_CacheServiceMock.Object, m_GroupPartyNotificationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ExpectedAsync()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        GroupParty v_Party = BoFactory.CreateGroupParty().Generate();
        m_UserRoleServiceMock.Setup(p_U => p_U.GetCurrentUserEmail()).Returns(v_User.Email);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_User);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_User);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(string.Empty);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<GroupParty>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_Party);

        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetMembersAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([v_User.Id, v_User.Id + 1]);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetAddAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetAsync(It.IsAny<string>(), It.IsAny<string>(), TimeSpan.Zero, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        m_GroupPartyNotificationServiceMock.Setup(p_Ns => p_Ns.NotifyPlayerJoinedAsync(It.IsAny<string>(), It.IsAny<User>())).Returns(Task.CompletedTask);
        JoinGroupPartyRequest v_Request = new(v_Party.Code);

        // Act
        GenericEntityResponse<GroupParty> v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.True(v_Result.Success);

        // Assert
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(v_User.Email, It.IsAny<CancellationToken>()), Times.Once);
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
        m_MockRepository.VerifyAll();
    }
}