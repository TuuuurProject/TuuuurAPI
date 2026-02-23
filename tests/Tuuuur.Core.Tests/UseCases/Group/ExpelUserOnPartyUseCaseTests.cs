using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Group;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.Group;

public class ExpelUserOnPartyUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<IUserRoleService> m_UserRoleServiceMock;
    private readonly Mock<IGroupNotificationService> m_GroupPartyNotificationServiceMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;

    private readonly ExpelUserOnPartyUseCase m_UseCase;

    public ExpelUserOnPartyUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_UnitOfWorkMock = m_MockRepository.Create<IUnitOfWork>();
        
        Mock<ILogger<ExpelUserOnPartyUseCase>> v_LoggerMock = new(); 
        
        m_UserRoleServiceMock = m_MockRepository.Create<IUserRoleService>();
        m_GroupPartyNotificationServiceMock = m_MockRepository.Create<IGroupNotificationService>();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();

        m_UseCase = new ExpelUserOnPartyUseCase(
            m_UnitOfWorkMock.Object, 
            v_LoggerMock.Object, 
            m_UserRoleServiceMock.Object,
            m_GroupPartyNotificationServiceMock.Object, 
            m_CacheServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_AsSimpleUser_ShouldLeaveGroup_ExpectedAsync()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        User v_TargetUser = BoFactory.CreateUser().Generate();
        v_TargetUser.Id = Guid.NewGuid();
        
        GroupParty v_Party = new()
        {
            Id = Guid.NewGuid(),
            Code = "123456",
            IdUserHost = v_User.Id,
            PartyUsers = []
        };

        m_UserRoleServiceMock.Setup(p_U => p_U.GetCurrentUserEmail())
            .Returns(v_User.Email);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(v_User.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_TargetUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_TargetUser);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<GroupParty>(RedisKeys.Party.ByCode(v_Party.Code), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetMembersAsync<Guid>(RedisKeys.Party.Users(v_Party.Code), It.IsAny<CancellationToken>()))
            .ReturnsAsync([v_User.Id, v_TargetUser.Id]);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<string>(RedisKeys.User.UserParty(v_User.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party.Code);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetRemoveAsync(RedisKeys.Party.Users(v_Party.Code), v_TargetUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.RemoveAsync(RedisKeys.User.UserParty(v_TargetUser.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        m_GroupPartyNotificationServiceMock
            .Setup(p_Ns => p_Ns.NotifyPlayerExpelledAsync(It.IsAny<string>(), It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        ExpelUserOnPartyRequest v_Request = new(v_TargetUser.Id);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();

        m_MockRepository.VerifyAll();
    }
}