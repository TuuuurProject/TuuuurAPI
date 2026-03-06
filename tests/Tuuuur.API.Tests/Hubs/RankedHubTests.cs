using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Tuuuur.API.Hubs;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Security;

namespace Tuuuur.API.Tests.Hubs;

public class RankedHubTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IMediator> m_MediatorMock;
    private readonly Mock<IHubCallerClients<IRankedClient>> m_ClientsMock;
    private readonly Mock<IRankedClient> m_CallerMock;
    private readonly Mock<HubCallerContext> m_ContextMock;
    private readonly RankedHub m_Hub;

    public RankedHubTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_MediatorMock = m_MockRepository.Create<IMediator>();
        m_ClientsMock = m_MockRepository.Create<IHubCallerClients<IRankedClient>>();
        m_CallerMock = m_MockRepository.Create<IRankedClient>();
        m_ContextMock = m_MockRepository.Create<HubCallerContext>();

        m_Hub = new RankedHub(m_MediatorMock.Object)
        {
            Clients = m_ClientsMock.Object,
            Context = m_ContextMock.Object
        };
    }

    private ClaimsPrincipal CreateClaimsPrincipal(Guid p_UserId)
    {
        Claim[] v_Claims = [new Claim(ClaimNames.Id, p_UserId.ToString())];
        return new ClaimsPrincipal(new ClaimsIdentity(v_Claims));
    }

    // ── JoinSearchOpponent ────────────────────────────────────────────────────

    [Fact]
    public async Task JoinSearchOpponent_WhenUserIsAuthenticated_ShouldSendMediatorRequest()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        m_ContextMock.Setup(p_C => p_C.User).Returns(CreateClaimsPrincipal(v_UserId));

        m_MediatorMock
            .Setup(p_M => p_M.Send(It.Is<JoinSearchOpponentRequest>(p_R => p_R.UserId == v_UserId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmptyResponse());

        // Act
        await m_Hub.JoinSearchOpponent();

        // Assert
        m_MediatorMock.Verify(p_M => p_M.Send(It.Is<JoinSearchOpponentRequest>(p_R => p_R.UserId == v_UserId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task JoinSearchOpponent_WhenUserIsNull_ShouldNotSendMediatorRequest()
    {
        // Arrange
        m_ContextMock.Setup(p_C => p_C.User).Returns((ClaimsPrincipal)null);

        // Act
        await m_Hub.JoinSearchOpponent();

        // Assert - mediator should not be called
        m_MediatorMock.Verify(p_M => p_M.Send(It.IsAny<JoinSearchOpponentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task JoinSearchOpponent_WhenMediatorThrows_ShouldNotifyCallerWithError()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        const string v_ErrorMessage = "Something went wrong";
        m_ContextMock.Setup(p_C => p_C.User).Returns(CreateClaimsPrincipal(v_UserId));

        m_MediatorMock
            .Setup(p_M => p_M.Send(It.IsAny<JoinSearchOpponentRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(v_ErrorMessage));

        m_ClientsMock.Setup(p_C => p_C.Caller).Returns(m_CallerMock.Object);
        m_CallerMock.Setup(p_C => p_C.OnError(v_ErrorMessage)).Returns(Task.CompletedTask);

        // Act
        await m_Hub.JoinSearchOpponent();

        // Assert
        m_CallerMock.Verify(p_C => p_C.OnError(v_ErrorMessage), Times.Once);
    }

    // ── LeaveSearchOpponent ───────────────────────────────────────────────────

    [Fact]
    public async Task LeaveSearchOpponent_WhenUserIsAuthenticated_ShouldSendMediatorRequest()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        m_ContextMock.Setup(p_C => p_C.User).Returns(CreateClaimsPrincipal(v_UserId));

        m_MediatorMock
            .Setup(p_M => p_M.Send(It.Is<LeaveSeachOpponentRequest>(p_R => p_R.UserId == v_UserId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmptyResponse());

        // Act
        await m_Hub.LeaveSearchOpponent();

        // Assert
        m_MediatorMock.Verify(p_M => p_M.Send(It.Is<LeaveSeachOpponentRequest>(p_R => p_R.UserId == v_UserId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LeaveSearchOpponent_WhenUserIsNull_ShouldNotSendMediatorRequest()
    {
        // Arrange
        m_ContextMock.Setup(p_C => p_C.User).Returns((ClaimsPrincipal)null);

        // Act
        await m_Hub.LeaveSearchOpponent();

        // Assert
        m_MediatorMock.Verify(p_M => p_M.Send(It.IsAny<LeaveSeachOpponentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LeaveSearchOpponent_WhenMediatorThrows_ShouldNotifyCallerWithError()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        const string v_ErrorMessage = "Leave error";
        m_ContextMock.Setup(p_C => p_C.User).Returns(CreateClaimsPrincipal(v_UserId));

        m_MediatorMock
            .Setup(p_M => p_M.Send(It.IsAny<LeaveSeachOpponentRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(v_ErrorMessage));

        m_ClientsMock.Setup(p_C => p_C.Caller).Returns(m_CallerMock.Object);
        m_CallerMock.Setup(p_C => p_C.OnError(v_ErrorMessage)).Returns(Task.CompletedTask);

        // Act
        await m_Hub.LeaveSearchOpponent();

        // Assert
        m_CallerMock.Verify(p_C => p_C.OnError(v_ErrorMessage), Times.Once);
    }

    // ── SendAnswer ────────────────────────────────────────────────────────────

    [Fact]
    public async Task SendAnswer_WhenUserIsAuthenticated_ShouldSendMediatorRequest()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        int v_AnswerId = 5;
        m_ContextMock.Setup(p_C => p_C.User).Returns(CreateClaimsPrincipal(v_UserId));

        m_MediatorMock
            .Setup(p_M => p_M.Send(It.Is<AnswerQuestionRankedRequest>(p_R => p_R.AnswerId == v_AnswerId && p_R.UserId == v_UserId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmptyResponse());

        // Act
        await m_Hub.SendAnswer(v_AnswerId);

        // Assert
        m_MediatorMock.Verify(p_M => p_M.Send(It.Is<AnswerQuestionRankedRequest>(p_R => p_R.AnswerId == v_AnswerId && p_R.UserId == v_UserId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAnswer_WhenUserIsNull_ShouldNotSendMediatorRequest()
    {
        // Arrange
        m_ContextMock.Setup(p_C => p_C.User).Returns((ClaimsPrincipal)null);

        // Act
        await m_Hub.SendAnswer(1);

        // Assert
        m_MediatorMock.Verify(p_M => p_M.Send(It.IsAny<AnswerQuestionRankedRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendAnswer_WhenMediatorThrows_ShouldNotifyCallerWithError()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        const string v_ErrorMessage = "Answer error";
        m_ContextMock.Setup(p_C => p_C.User).Returns(CreateClaimsPrincipal(v_UserId));

        m_MediatorMock
            .Setup(p_M => p_M.Send(It.IsAny<AnswerQuestionRankedRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(v_ErrorMessage));

        m_ClientsMock.Setup(p_C => p_C.Caller).Returns(m_CallerMock.Object);
        m_CallerMock.Setup(p_C => p_C.OnError(v_ErrorMessage)).Returns(Task.CompletedTask);

        // Act
        await m_Hub.SendAnswer(1);

        // Assert
        m_CallerMock.Verify(p_C => p_C.OnError(v_ErrorMessage), Times.Once);
    }
}
