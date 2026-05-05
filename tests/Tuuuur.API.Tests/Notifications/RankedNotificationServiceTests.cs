using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Tuuuur.API.Hubs;
using Tuuuur.API.Notifications;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Notifications;

namespace Tuuuur.API.Tests.Notifications;

public class RankedNotificationServiceTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IHubContext<RankedHub, IRankedClient>> m_HubContextMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;
    private readonly Mock<IHubClients<IRankedClient>> m_ClientsMock;
    private readonly Mock<IRankedClient> m_RankedClientMock;
    private readonly RankedNotificationService m_Service;

    public RankedNotificationServiceTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_HubContextMock = m_MockRepository.Create<IHubContext<RankedHub, IRankedClient>>();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();
        Mock<ILogger<RankedNotificationService>> v_LoggerMock = new Mock<ILogger<RankedNotificationService>>();
        m_ClientsMock = m_MockRepository.Create<IHubClients<IRankedClient>>();
        m_RankedClientMock = m_MockRepository.Create<IRankedClient>();

        m_Service = new RankedNotificationService(
            m_HubContextMock.Object,
            m_CacheServiceMock.Object,
            v_LoggerMock.Object);
    }

    private Party CreatePartyWithUsers(params Guid[] p_UserIds)
    {
        Party v_Party = new()
        {
            Id = Guid.NewGuid(),
            PartyUsers = p_UserIds.Select(p_Id => new PartyUser
            {
                IdUser = p_Id,
                User = new User { Id = p_Id, Elo = [] }
            }).ToList(),
            PartyQuestions = []
        };
        return v_Party;
    }

    // ── NotifyCountdownAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task NotifyCountdownAsync_WithUsers_ShouldBroadcastCountdown()
    {
        // Arrange
        Guid v_PartyId = Guid.NewGuid();
        Guid v_User1 = Guid.NewGuid();
        Guid v_User2 = Guid.NewGuid();
        Party v_Party = CreatePartyWithUsers(v_User1, v_User2);

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<Party>(RedisKeys.Ranked.ById(v_PartyId), CancellationToken.None))
            .ReturnsAsync(v_Party);

        m_HubContextMock.Setup(p_H => p_H.Clients).Returns(m_ClientsMock.Object);
        m_ClientsMock
            .Setup(p_C => p_C.Users(It.IsAny<IReadOnlyList<string>>()))
            .Returns(m_RankedClientMock.Object);
        m_RankedClientMock
            .Setup(p_R => p_R.OnCountdown(3))
            .Returns(Task.CompletedTask);

        // Act
        await m_Service.NotifyCountdownAsync(v_PartyId, 3);

        // Assert
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task NotifyCountdownAsync_WithNoUsers_ShouldNotBroadcast()
    {
        // Arrange
        Guid v_PartyId = Guid.NewGuid();
        Party v_Party = new() { Id = v_PartyId, PartyUsers = [], PartyQuestions = [] };

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<Party>(RedisKeys.Ranked.ById(v_PartyId), CancellationToken.None))
            .ReturnsAsync(v_Party);

        // Act
        await m_Service.NotifyCountdownAsync(v_PartyId, 3);

        // Assert
        m_MockRepository.VerifyAll();
    }

    // ── NotifyMatchFoundAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task NotifyMatchFoundAsync_ShouldNotifyEachPlayerWithTheirOpponent()
    {
        // Arrange
        User v_P1 = new() { Id = Guid.NewGuid(), NickName = "P1", Elo = [] };
        User v_P2 = new() { Id = Guid.NewGuid(), NickName = "P2", Elo = [] };
        Guid v_PartyId = Guid.NewGuid();

        m_HubContextMock.Setup(p_H => p_H.Clients).Returns(m_ClientsMock.Object);

        Mock<IRankedClient> v_P1Client = new();
        Mock<IRankedClient> v_P2Client = new();

        m_ClientsMock
            .Setup(p_C => p_C.User(v_P1.Id.ToString()))
            .Returns(v_P1Client.Object);
        m_ClientsMock
            .Setup(p_C => p_C.User(v_P2.Id.ToString()))
            .Returns(v_P2Client.Object);

        v_P1Client.Setup(p_C => p_C.OnOpponentFound(v_P2)).Returns(Task.CompletedTask);
        v_P2Client.Setup(p_C => p_C.OnOpponentFound(v_P1)).Returns(Task.CompletedTask);

        // Act
        await m_Service.NotifyMatchFoundAsync(v_P1, v_P2, v_PartyId);

        // Assert
        v_P1Client.Verify(p_C => p_C.OnOpponentFound(v_P2), Times.Once);
        v_P2Client.Verify(p_C => p_C.OnOpponentFound(v_P1), Times.Once);
    }

    // ── NotifyPartyQuestionSend ──────────────────────────────────────────────

    [Fact]
    public async Task NotifyPartyQuestionSend_ShouldSendQuestionToSpecificUser()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        RankedQuestion v_Question = new() { CurrentIndex = 0 };

        m_HubContextMock.Setup(p_H => p_H.Clients).Returns(m_ClientsMock.Object);
        m_ClientsMock
            .Setup(p_C => p_C.User(v_UserId.ToString()))
            .Returns(m_RankedClientMock.Object);
        m_RankedClientMock
            .Setup(p_R => p_R.OnQuestionSend(v_Question))
            .Returns(Task.CompletedTask);

        // Act
        await m_Service.NotifyPartyQuestionSend(v_UserId, v_Question);

        // Assert
        m_MockRepository.VerifyAll();
    }

    // ── NotifyQuestionAnswerSend ─────────────────────────────────────────────

    [Fact]
    public async Task NotifyQuestionAnswerSend_ShouldSendAnswerToUser()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        RankedQuestion v_Question = new() { CurrentIndex = 1, Score = 500 };

        m_HubContextMock.Setup(p_H => p_H.Clients).Returns(m_ClientsMock.Object);
        m_ClientsMock
            .Setup(p_C => p_C.Users(It.IsAny<IReadOnlyList<string>>()))
            .Returns(m_RankedClientMock.Object);
        m_RankedClientMock
            .Setup(p_R => p_R.OnQuestionAnswerSend(v_Question))
            .Returns(Task.CompletedTask);

        // Act
        await m_Service.NotifyQuestionAnswerSend(v_UserId, v_Question);

        // Assert
        m_MockRepository.VerifyAll();
    }

    // ── NotifyAllPlayerAnswered ──────────────────────────────────────────────

    [Fact]
    public async Task NotifyAllPlayerAnswered_WithUsers_ShouldBroadcast()
    {
        // Arrange
        Guid v_PartyId = Guid.NewGuid();
        Guid v_U1 = Guid.NewGuid();
        Party v_Party = CreatePartyWithUsers(v_U1);
        IEnumerable<UserAnswered> v_UserAnswers = [new UserAnswered { User = new User { Id = v_U1 }, Correct = true }];

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<Party>(RedisKeys.Ranked.ById(v_PartyId), CancellationToken.None))
            .ReturnsAsync(v_Party);

        m_HubContextMock.Setup(p_H => p_H.Clients).Returns(m_ClientsMock.Object);
        m_ClientsMock
            .Setup(p_C => p_C.Users(It.IsAny<IReadOnlyList<string>>()))
            .Returns(m_RankedClientMock.Object);
        m_RankedClientMock
            .Setup(p_R => p_R.OnAllPlayerAnswered(v_UserAnswers))
            .Returns(Task.CompletedTask);

        // Act
        await m_Service.NotifyAllPlayerAnswered(v_PartyId, v_UserAnswers);

        // Assert
        m_MockRepository.VerifyAll();
    }

    // ── NotifyPartyScoresAsync ───────────────────────────────────────────────

    [Fact]
    public async Task NotifyPartyScoresAsync_WithUsers_ShouldBroadcastScores()
    {
        // Arrange
        Guid v_PartyId = Guid.NewGuid();
        Guid v_U1 = Guid.NewGuid();
        Party v_Party = CreatePartyWithUsers(v_U1);
        List<UserScore> v_Scores = [new UserScore { User = new User { Id = v_U1 }, Score = 4000 }];

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<Party>(RedisKeys.Ranked.ById(v_PartyId), CancellationToken.None))
            .ReturnsAsync(v_Party);

        m_HubContextMock.Setup(p_H => p_H.Clients).Returns(m_ClientsMock.Object);
        m_ClientsMock
            .Setup(p_C => p_C.Users(It.IsAny<IReadOnlyList<string>>()))
            .Returns(m_RankedClientMock.Object);
        m_RankedClientMock
            .Setup(p_R => p_R.OnScoreUpdate(v_Scores))
            .Returns(Task.CompletedTask);

        // Act
        await m_Service.NotifyPartyScoresAsync(v_PartyId, v_Scores);

        // Assert
        m_MockRepository.VerifyAll();
    }

    // ── NotifyUserSendAnswerAsync ─────────────────────────────────────────────

    [Fact]
    public async Task NotifyUserSendAnswerAsync_ShouldSendUserToPartyGroup()
    {
        // Arrange
        Guid v_PartyId = Guid.NewGuid();
        User v_User = new() { Id = Guid.NewGuid(), Elo = [] };

        m_HubContextMock.Setup(p_H => p_H.Clients).Returns(m_ClientsMock.Object);
        m_ClientsMock
            .Setup(p_C => p_C.Users(It.IsAny<IReadOnlyList<string>>()))
            .Returns(m_RankedClientMock.Object);
        m_RankedClientMock
            .Setup(p_R => p_R.OnUserAnswer(v_User))
            .Returns(Task.CompletedTask);

        // Act
        await m_Service.NotifyUserSendAnswerAsync(v_PartyId, v_User);

        // Assert
        m_MockRepository.VerifyAll();
    }

    // ── NotifyUserWinAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task NotifyUserWinAsync_ShouldSendDeltaToWinner()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        int v_Delta = 15;

        m_HubContextMock.Setup(p_H => p_H.Clients).Returns(m_ClientsMock.Object);
        m_ClientsMock
            .Setup(p_C => p_C.User(v_UserId.ToString()))
            .Returns(m_RankedClientMock.Object);
        m_RankedClientMock
            .Setup(p_R => p_R.OnUserWin(v_Delta))
            .Returns(Task.CompletedTask);

        // Act
        await m_Service.NotifyUserWinAsync(v_UserId, v_Delta);

        // Assert
        m_MockRepository.VerifyAll();
    }

    // ── NotifyUserLooseAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task NotifyUserLooseAsync_ShouldSendDeltaToLoser()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        int v_Delta = -10;

        m_HubContextMock.Setup(p_H => p_H.Clients).Returns(m_ClientsMock.Object);
        m_ClientsMock
            .Setup(p_C => p_C.User(v_UserId.ToString()))
            .Returns(m_RankedClientMock.Object);
        m_RankedClientMock
            .Setup(p_R => p_R.OnUserLoose(v_Delta))
            .Returns(Task.CompletedTask);

        // Act
        await m_Service.NotifyUserLooseAsync(v_UserId, v_Delta);

        // Assert
        m_MockRepository.VerifyAll();
    }
}
