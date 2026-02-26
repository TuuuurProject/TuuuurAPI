using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Tuuuur.API.Hubs;
using Tuuuur.API.Notifications;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Factory.Tests;

namespace Tuuuur.API.Tests.Notifications;

public class GroupNotificationServiceTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IHubContext<GroupHub, IGroupClient>> m_HubContextMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;
    private readonly Mock<IHubClients<IGroupClient>> m_ClientsMock;
    private readonly Mock<IGroupClient> m_GroupClientMock;
    private readonly GroupNotificationService m_Service;

    public GroupNotificationServiceTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_HubContextMock = m_MockRepository.Create<IHubContext<GroupHub, IGroupClient>>();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();
        Mock<ILogger<GroupNotificationService>> v_LoggerMock = m_MockRepository.Create<ILogger<GroupNotificationService>>();
        m_ClientsMock = m_MockRepository.Create<IHubClients<IGroupClient>>();
        m_GroupClientMock = m_MockRepository.Create<IGroupClient>();

        m_Service = new GroupNotificationService(
            m_HubContextMock.Object,
            m_CacheServiceMock.Object,
            v_LoggerMock.Object
        );
    }

    [Fact]
    public async Task NotifyPlayerJoinedAsync_WithUsers_ShouldSendNotificationToOtherUsers()
    {
        // Arrange
        const string v_PartyCode = "123456";
        User v_User = new() { Id = Guid.NewGuid(), NickName = "TestUser", Email = "test@example.com" };
        List<User> v_UserIds = BoFactory.CreateUser().Generate(3);

        m_CacheServiceMock
            .Setup(p_C => p_C.SetMembersAsync<User>(
                RedisKeys.Party.Users(v_PartyCode),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_UserIds);

        m_HubContextMock
            .Setup(p_H => p_H.Clients)
            .Returns(m_ClientsMock.Object);

        m_ClientsMock
            .Setup(p_C => p_C.Users(It.IsAny<IReadOnlyList<string>>()))
            .Returns(m_GroupClientMock.Object);

        m_GroupClientMock
            .Setup(p_G => p_G.OnPlayerJoined(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        await m_Service.NotifyPlayerJoinedAsync(v_PartyCode, v_User);

        // Assert
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task NotifyPlayerJoinedAsync_WithNoUsers_ShouldNotSendNotification()
    {
        // Arrange
        const string v_PartyCode = "123456";
        User v_User = new() { Id = Guid.NewGuid(), NickName = "TestUser", Email = "test@example.com" };
        List<User> v_UserIds = [];

        m_CacheServiceMock
            .Setup(p_C => p_C.SetMembersAsync<User>(
                RedisKeys.Party.Users(v_PartyCode),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_UserIds);

        // Act
        await m_Service.NotifyPlayerJoinedAsync(v_PartyCode, v_User);

        // Assert
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task NotifyPlayerLeftAsync_WithUsers_ShouldSendNotificationToOtherUsers()
    {
        // Arrange
        const string v_PartyCode = "123456";
        User v_User = new() { Id = Guid.NewGuid(), NickName = "LeavingUser", Email = "leaving@example.com" };
        List<User> v_UserIds = BoFactory.CreateUser().Generate(3);

        m_CacheServiceMock
            .Setup(p_C => p_C.SetMembersAsync<User>(
                RedisKeys.Party.Users(v_PartyCode),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_UserIds);

        m_HubContextMock
            .Setup(p_H => p_H.Clients)
            .Returns(m_ClientsMock.Object);

        m_ClientsMock
            .Setup(p_C => p_C.Users(It.IsAny<IReadOnlyList<string>>()))
            .Returns(m_GroupClientMock.Object);
        
        m_GroupClientMock
            .Setup(p_G => p_G.OnPlayerLeft(v_User))
            .Returns(Task.CompletedTask);

        // Act
        await m_Service.NotifyPlayerLeftAsync(v_PartyCode, v_User);

        // Assert
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task NotifyPartyDeletedAsync_WithUsers_ShouldSendNotificationToOtherUsers()
    {
        // Arrange
        const string v_PartyCode = "123456";
        List<User> v_Users = BoFactory.CreateUser().Generate(3);
        User v_User = new() { Id = Guid.NewGuid(), NickName = "HostUser", Email = "host@example.com" };

        m_CacheServiceMock
            .Setup(p_C => p_C.SetMembersAsync<User>(
                RedisKeys.Party.Users(v_PartyCode),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Users);

        m_HubContextMock
            .Setup(p_H => p_H.Clients)
            .Returns(m_ClientsMock.Object);

        m_ClientsMock
            .Setup(p_C => p_C.Users(It.IsAny<IReadOnlyList<string>>()))
            .Returns(m_GroupClientMock.Object);

        m_GroupClientMock
            .Setup(p_G => p_G.OnPartyDeleted(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        await m_Service.NotifyPartyDeletedAsync(v_PartyCode, v_User);

        // Assert
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task NotifyPartyUpdatedAsync_WithUsers_ShouldSendNotificationToAllUsers()
    {
        // Arrange
        const string v_PartyCode = "123456";
        GroupParty v_Party = new()
        {
            Code = v_PartyCode,
            IdUserHost = Guid.NewGuid(),
            Active = true
        };
        List<User> v_UserIds = BoFactory.CreateUser().Generate(3);

        m_CacheServiceMock
            .Setup(p_C => p_C.SetMembersAsync<User>(
                RedisKeys.Party.Users(v_PartyCode),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_UserIds);

        m_HubContextMock
            .Setup(p_H => p_H.Clients)
            .Returns(m_ClientsMock.Object);

        m_ClientsMock
            .Setup(p_C => p_C.Users(It.IsAny<IReadOnlyList<string>>()))
            .Returns(m_GroupClientMock.Object);

        m_GroupClientMock
            .Setup(p_G => p_G.OnPartyUpdated(v_Party))
            .Returns(Task.CompletedTask);

        // Act
        await m_Service.NotifyPartyUpdatedAsync(v_PartyCode, v_Party);

        // Assert
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task NotifyPartyUpdatedAsync_WithNoUsers_ShouldNotSendNotification()
    {
        // Arrange
        const string v_PartyCode = "123456";
        GroupParty v_Party = new()
        {
            Code = v_PartyCode
        };
        List<User> v_UserIds = [];

        m_CacheServiceMock
            .Setup(p_C => p_C.SetMembersAsync<User>(
                RedisKeys.Party.Users(v_PartyCode),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_UserIds);

        // Act
        await m_Service.NotifyPartyUpdatedAsync(v_PartyCode, v_Party);

        // Assert
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task NotifyPlayerJoinedAsync_WithOnlyJoiningUser_ShouldNotSendNotification()
    {
        // Arrange
        const string v_PartyCode = "123456";
        User v_User = new() { Id = Guid.NewGuid(), NickName = "OnlyUser", Email = "only@example.com" };
        List<User> v_UserIds = BoFactory.CreateUser().Generate(3);

        m_CacheServiceMock
            .Setup(p_C => p_C.SetMembersAsync<User>(
                RedisKeys.Party.Users(v_PartyCode),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_UserIds);

        m_HubContextMock
            .Setup(p_H => p_H.Clients)
            .Returns(m_ClientsMock.Object);

        m_ClientsMock
            .Setup(p_C => p_C.Users(It.IsAny<IReadOnlyList<string>>()))
            .Returns(m_GroupClientMock.Object);

        m_GroupClientMock
            .Setup(p_G => p_G.OnPlayerJoined(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        await m_Service.NotifyPlayerJoinedAsync(v_PartyCode, v_User);

        // Assert - No notification should be sent (list becomes empty after removing the user)
        m_MockRepository.VerifyAll();
    }
}
