using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Configuration;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Ranked;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Services;
using Tuuuur.Domain.Notifications;

namespace Tuuuur.Core.Tests.UseCases.Ranked;

public class RankedLogicUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;
    private readonly Mock<IRankedNotificationService> m_NotificationServiceMock;
    private readonly Mock<ICalculService> m_CalculServiceMock;
    private readonly Mock<IEloService> m_EloServiceMock;
    private readonly Mock<IMediator> m_MediatorMock;
    private readonly RankedConfiguration m_Config;
    private readonly RankedLogicUseCase m_UseCase;

    public RankedLogicUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_UnitOfWorkMock = m_MockRepository.Create<IUnitOfWork>();
        Mock<ILogger<RankedLogicUseCase>> v_LoggerMock = new Mock<ILogger<RankedLogicUseCase>>();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();
        m_NotificationServiceMock = m_MockRepository.Create<IRankedNotificationService>();
        m_CalculServiceMock = m_MockRepository.Create<ICalculService>();
        m_EloServiceMock = m_MockRepository.Create<IEloService>();
        m_MediatorMock = m_MockRepository.Create<IMediator>();
        m_Config = new RankedConfiguration
        {
            ThresholdRound = 5,
            MultiplierIncrement = 0.5,
            InitialRankedScore = 5000,
            DefaultElo = 1000
        };

        m_UseCase = new RankedLogicUseCase(
            m_UnitOfWorkMock.Object,
            v_LoggerMock.Object,
            m_NotificationServiceMock.Object,
            m_CalculServiceMock.Object,
            m_EloServiceMock.Object,
            m_CacheServiceMock.Object,
            m_MediatorMock.Object,
            m_Config);
    }

    private Party CreateParty(Guid p_P1Id, Guid p_P2Id)
    {
        return new Party
        {
            Id = Guid.NewGuid(),
            PartyUsers =
            [
                new PartyUser { IdUser = p_P1Id, User = new User { Id = p_P1Id, Elo = [] } },
                new PartyUser { IdUser = p_P2Id, User = new User { Id = p_P2Id, Elo = [] } }
            ],
            PartyQuestions = []
        };
    }

    [Fact]
    public async Task Handle_WhenPlayer1NotFound_ShouldReturnError()
    {
        // Arrange
        Guid v_P1Id = Guid.NewGuid();
        Guid v_P2Id = Guid.NewGuid();
        Party v_Party = CreateParty(v_P1Id, v_P2Id);

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<Party>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);
        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_P1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        RankedLogicRequest v_Request = new(v_Party.Id);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenPlayer2NotFound_ShouldReturnError()
    {
        // Arrange
        Guid v_P1Id = Guid.NewGuid();
        Guid v_P2Id = Guid.NewGuid();
        Party v_Party = CreateParty(v_P1Id, v_P2Id);
        User v_P1 = new() { Id = v_P1Id, Elo = [] };

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<Party>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);
        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_P1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_P1);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_P2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        RankedLogicRequest v_Request = new(v_Party.Id);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenCancelled_BeforeCountdown_ShouldAbortGracefully()
    {
        // Arrange
        Guid v_P1Id = Guid.NewGuid();
        Guid v_P2Id = Guid.NewGuid();
        Party v_Party = CreateParty(v_P1Id, v_P2Id);
        User v_P1 = new() { Id = v_P1Id, Elo = [] };
        User v_P2 = new() { Id = v_P2Id, Elo = [] };

        Question v_Question = new()
        {
            Id = 1,
            Label = "Q1",
            Answer = [new Answer { Id = 1, Valid = true, Value = "A" }]
        };

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<Party>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);
        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetGetAllWithScoresAsync<Question>(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Question, int)>());
        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetAddAsync(It.IsAny<string>(), It.IsAny<Question>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_P1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_P1);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_P2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_P2);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.QuestionRepository.GetRandomQuestionExcludingAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Question);

        // Notify countdown will be called then Task.Delay will be cancelled
        m_NotificationServiceMock
            .Setup(p_N => p_N.NotifyCountdownAsync(v_Party.Id, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        using CancellationTokenSource v_Cts = new();
        // Cancel after very short time, ensuring the countdown loop is cancelled
        v_Cts.CancelAfter(50);

        RankedLogicRequest v_Request = new(v_Party.Id);

        // Act - AUseCase.Handle catches OperationCanceledException and returns an error response
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, v_Cts.Token);

        // Assert - should have failed due to cancellation
        v_Result.Success.Should().BeFalse();
    }
}
