using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Configuration;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Ranked;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;
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
    private readonly Mock<IRankService> m_RankServiceMock;
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
        m_RankServiceMock = m_MockRepository.Create<IRankService>();
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
            m_RankServiceMock.Object,
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

        m_RankServiceMock
            .Setup(p_R => p_R.GetAverageTier(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(1);

        m_RankServiceMock
            .Setup(p_R => p_R.GetPoolForTier(It.IsAny<int>()))
            .Returns((RankPoolConfiguration)null);

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

    // ── Scoring delta tests (cancel after NotifyAllPlayerAnswered, ~3.5s) ────

    /// <summary>
    /// Sets up all mocks needed for a full question round, up to and including
    /// the answer-reveal notifications. The CancellationToken is cancelled at 3.5 s
    /// to skip the 5-second score-display delay and keep the test fast.
    /// </summary>
    private void SetupRoundMocks(
        Guid p_PartyId,
        Guid p_P1Id,
        Guid p_P2Id,
        User p_P1,
        User p_P2,
        Question p_Question,
        Question p_QuestionWithAnswers,
        UserPartyQuestion p_Upq1,
        UserPartyQuestion p_Upq2,
        int p_CurrentIndex = 0)
    {
        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<Party>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Party
            {
                Id = p_PartyId,
                PartyUsers =
                [
                    new PartyUser { IdUser = p_P1Id, User = p_P1 },
                    new PartyUser { IdUser = p_P2Id, User = p_P2 }
                ],
                PartyQuestions = []
            });

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(p_CurrentIndex);

        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetGetAllWithScoresAsync<Question>(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Question, int)>());

        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetAddAsync(It.IsAny<string>(), It.IsAny<Question>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(p_P1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(p_P1);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(p_P2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(p_P2);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.QuestionRepository.GetRandomQuestionExcludingAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(p_Question);

        m_NotificationServiceMock
            .Setup(p_N => p_N.NotifyCountdownAsync(It.IsAny<Guid>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        m_CacheServiceMock
            .Setup(p_C => p_C.SetAsync(It.IsAny<string>(), It.IsAny<UserPartyQuestion>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        m_NotificationServiceMock
            .Setup(p_N => p_N.NotifyPartyQuestionSend(It.IsAny<Guid>(), It.IsAny<RankedQuestion>()))
            .Returns(Task.CompletedTask);

        m_CacheServiceMock
            .Setup(p_C => p_C.SubscribeAndWaitAsync<bool>(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<User>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);
        
        m_CacheServiceMock
            .Setup(p_C => p_C.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<UserPartyQuestion>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((string key, CancellationToken ct) =>
                Task.FromResult(key.Contains(p_P1Id.ToString()) ? p_Upq1 : p_Upq2));

        m_UnitOfWorkMock
            .Setup(p_U => p_U.QuestionRepository.GetQuestionByIdWithAnswerAsync(p_Question.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(p_QuestionWithAnswers);

        m_NotificationServiceMock
            .Setup(p_N => p_N.NotifyQuestionAnswerSend(It.IsAny<Guid>(), It.IsAny<RankedQuestion>()))
            .Returns(Task.CompletedTask);

        m_NotificationServiceMock
            .Setup(p_N => p_N.NotifyAllPlayerAnswered(It.IsAny<Guid>(), It.IsAny<IEnumerable<UserAnswered>>()))
            .Returns(Task.CompletedTask);

        m_RankServiceMock
            .Setup(p_R => p_R.GetAverageTier(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(1);

        m_RankServiceMock
            .Setup(p_R => p_R.GetPoolForTier(It.IsAny<int>()))
            .Returns((RankPoolConfiguration)null);
    }

    [Fact]
    public async Task Handle_WhenPlayer1CorrectPlayer2TimedOut_ShouldDeductScoreFromPlayer2()
    {
        // Arrange
        Guid v_P1Id = Guid.NewGuid();
        Guid v_P2Id = Guid.NewGuid();
        Guid v_PartyId = Guid.NewGuid();
        User v_P1 = new() { Id = v_P1Id, Elo = [] };
        User v_P2 = new() { Id = v_P2Id, Elo = [] };

        int v_AnswerId = 10;
        Question v_Question = new() { Id = 1, Label = "Q1", Answer = [new Answer { Id = v_AnswerId, Valid = true, Value = "A" }] };
        Question v_QuestionWithAnswers = new() { Id = 1, Label = "Q1", Answer = [new Answer { Id = v_AnswerId, Valid = true, Value = "A" }] };

        UserPartyQuestion v_Upq1 = new()
        {
            IdUser = v_P1Id,
            IdAnswer = v_AnswerId,           // P1 answered correctly
            DtPresentedAt = DateTime.Now.AddSeconds(-3),
            DtAnsweredAt = DateTime.Now,
            AnswersOrder = Guid.NewGuid()
        };

        // P2 timed out → null UPQ
        SetupRoundMocks(v_PartyId, v_P1Id, v_P2Id, v_P1, v_P2, v_Question, v_QuestionWithAnswers, v_Upq1, p_Upq2: null);

        // CalculateScore called only for P1 (P2 UPQ is null)
        m_CalculServiceMock
            .Setup(p_C => p_C.CalculateScore(It.IsAny<DateTime>(), It.IsAny<DateTime?>()))
            .Returns(100);

        using CancellationTokenSource v_Cts = new();
        v_Cts.CancelAfter(TimeSpan.FromSeconds(4)); // allows 3s countdown + scoring; skips 5s delay

        RankedLogicRequest v_Request = new(v_PartyId);

        // Act
        await m_UseCase.Handle(v_Request, v_Cts.Token);

        // Assert: CalculateScore was called once (P1 only, P2 was null)
        m_CalculServiceMock.Verify(
            p_C => p_C.CalculateScore(It.IsAny<DateTime>(), It.IsAny<DateTime?>()),
            Times.Once);

        // NotifyQuestionAnswerSend must have been called for both players
        m_NotificationServiceMock.Verify(
            p_N => p_N.NotifyQuestionAnswerSend(v_P1Id, It.Is<RankedQuestion>(q => q.Score == 0)),
            Times.Once);  // P1 correct → delta=0
        m_NotificationServiceMock.Verify(
            p_N => p_N.NotifyQuestionAnswerSend(v_P2Id, It.Is<RankedQuestion>(q => q.Score == -100)),
            Times.Once);  // P2 wrong → delta=-(rawScore1*1.0)=-100
    }

    [Fact]
    public async Task Handle_WhenBothPlayersAnswerCorrectly_ShouldNotDeductAnyScore()
    {
        // Arrange
        Guid v_P1Id = Guid.NewGuid();
        Guid v_P2Id = Guid.NewGuid();
        Guid v_PartyId = Guid.NewGuid();
        User v_P1 = new() { Id = v_P1Id, Elo = [] };
        User v_P2 = new() { Id = v_P2Id, Elo = [] };

        int v_AnswerId = 10;
        Question v_Question = new() { Id = 1, Label = "Q1", Answer = [new Answer { Id = v_AnswerId, Valid = true, Value = "A" }] };
        Question v_QuestionWithAnswers = new() { Id = 1, Label = "Q1", Answer = [new Answer { Id = v_AnswerId, Valid = true, Value = "A" }] };

        UserPartyQuestion v_Upq1 = new()
        {
            IdUser = v_P1Id,
            IdAnswer = v_AnswerId,
            DtPresentedAt = DateTime.Now.AddSeconds(-3),
            DtAnsweredAt = DateTime.Now,
            AnswersOrder = Guid.NewGuid()
        };
        UserPartyQuestion v_Upq2 = new()
        {
            IdUser = v_P2Id,
            IdAnswer = v_AnswerId,
            DtPresentedAt = DateTime.Now.AddSeconds(-4),
            DtAnsweredAt = DateTime.Now,
            AnswersOrder = Guid.NewGuid()
        };

        SetupRoundMocks(v_PartyId, v_P1Id, v_P2Id, v_P1, v_P2, v_Question, v_QuestionWithAnswers, v_Upq1, v_Upq2);

        // Both players answered → CalculateScore called twice
        m_CalculServiceMock
            .Setup(p_C => p_C.CalculateScore(It.IsAny<DateTime>(), It.IsAny<DateTime?>()))
            .Returns(100);

        using CancellationTokenSource v_Cts = new();
        v_Cts.CancelAfter(TimeSpan.FromSeconds(4));

        RankedLogicRequest v_Request = new(v_PartyId);

        // Act
        await m_UseCase.Handle(v_Request, v_Cts.Token);

        // Assert: both correct → delta = 0 for both (draw: no score change)
        m_NotificationServiceMock.Verify(
            p_N => p_N.NotifyQuestionAnswerSend(v_P1Id, It.Is<RankedQuestion>(q => q.Score == 0)),
            Times.Once);
        m_NotificationServiceMock.Verify(
            p_N => p_N.NotifyQuestionAnswerSend(v_P2Id, It.Is<RankedQuestion>(q => q.Score == 0)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBothPlayersTimedOut_ShouldNotDeductAnyScore()
    {
        // Arrange: both UPQs are null (timeout) → both wrong → draw, no deduction
        Guid v_P1Id = Guid.NewGuid();
        Guid v_P2Id = Guid.NewGuid();
        Guid v_PartyId = Guid.NewGuid();
        User v_P1 = new() { Id = v_P1Id, Elo = [] };
        User v_P2 = new() { Id = v_P2Id, Elo = [] };

        Question v_Question = new() { Id = 1, Label = "Q1", Answer = [new Answer { Id = 10, Valid = true, Value = "A" }] };
        Question v_QuestionWithAnswers = new() { Id = 1, Label = "Q1", Answer = [new Answer { Id = 10, Valid = true, Value = "A" }] };

        // Both null: neither player answered in time
        SetupRoundMocks(v_PartyId, v_P1Id, v_P2Id, v_P1, v_P2, v_Question, v_QuestionWithAnswers, p_Upq1: null, p_Upq2: null);

        // CalculateScore must NOT be called when both UPQs are null
        // (no setup needed — MockBehavior.Strict will throw if it's called unexpectedly)

        using CancellationTokenSource v_Cts = new();
        v_Cts.CancelAfter(TimeSpan.FromSeconds(4));

        RankedLogicRequest v_Request = new(v_PartyId);

        // Act
        await m_UseCase.Handle(v_Request, v_Cts.Token);

        // Assert: both timed-out → delta = 0 for both
        m_NotificationServiceMock.Verify(
            p_N => p_N.NotifyQuestionAnswerSend(v_P1Id, It.Is<RankedQuestion>(q => q.Score == 0)),
            Times.Once);
        m_NotificationServiceMock.Verify(
            p_N => p_N.NotifyQuestionAnswerSend(v_P2Id, It.Is<RankedQuestion>(q => q.Score == 0)),
            Times.Once);
        m_CalculServiceMock.Verify(
            p_C => p_C.CalculateScore(It.IsAny<DateTime>(), It.IsAny<DateTime?>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoundExceedsThreshold_ShouldApplyIncreasedMultiplier()
    {
        // Arrange: round index = 5 → round = 6 > ThresholdRound(5) → multiplier = 1 + (6-5)*0.5 = 1.5
        // P1 correct (rawScore=100), P2 null → v_Delta2 = -(100 * 1.5) = -150
        Guid v_P1Id = Guid.NewGuid();
        Guid v_P2Id = Guid.NewGuid();
        Guid v_PartyId = Guid.NewGuid();
        User v_P1 = new() { Id = v_P1Id, Elo = [] };
        User v_P2 = new() { Id = v_P2Id, Elo = [] };

        int v_AnswerId = 10;
        Question v_Question = new() { Id = 1, Label = "Q1", Answer = [new Answer { Id = v_AnswerId, Valid = true, Value = "A" }] };
        Question v_QuestionWithAnswers = new() { Id = 1, Label = "Q1", Answer = [new Answer { Id = v_AnswerId, Valid = true, Value = "A" }] };

        UserPartyQuestion v_Upq1 = new()
        {
            IdUser = v_P1Id,
            IdAnswer = v_AnswerId,
            DtPresentedAt = DateTime.Now.AddSeconds(-3),
            DtAnsweredAt = DateTime.Now,
            AnswersOrder = Guid.NewGuid()
        };

        // currentIndex = 5 → round = 6, multiplier = 1.5
        SetupRoundMocks(v_PartyId, v_P1Id, v_P2Id, v_P1, v_P2, v_Question, v_QuestionWithAnswers, v_Upq1, p_Upq2: null, p_CurrentIndex: 5);

        m_CalculServiceMock
            .Setup(p_C => p_C.CalculateScore(It.IsAny<DateTime>(), It.IsAny<DateTime?>()))
            .Returns(100);

        using CancellationTokenSource v_Cts = new();
        v_Cts.CancelAfter(TimeSpan.FromSeconds(4));

        RankedLogicRequest v_Request = new(v_PartyId);

        // Act
        await m_UseCase.Handle(v_Request, v_Cts.Token);

        // Assert: P2 delta = -(100 * 1.5) = -150 due to increased multiplier
        m_NotificationServiceMock.Verify(
            p_N => p_N.NotifyQuestionAnswerSend(v_P2Id, It.Is<RankedQuestion>(q => q.Score == -150)),
            Times.Once);
        // P1 is correct → delta = 0 regardless of multiplier
        m_NotificationServiceMock.Verify(
            p_N => p_N.NotifyQuestionAnswerSend(v_P1Id, It.Is<RankedQuestion>(q => q.Score == 0)),
            Times.Once);
    }

    // ── End-game path (full run, ~10s) ────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenGameEnds_ShouldUpdateEloAndSaveParty()
    {
        // Arrange: P1 correct (rawScore=100), P2 null UPQ, P2 current score=50.
        // v_Delta2 = -(100 * 1.0) = -100 → v_Total2 = max(0, 50-100) = 0 → game ends.
        // Expected: EloRepository.UpdateValueAsync called for both players, party persisted.
        Guid v_P1Id = Guid.NewGuid();
        Guid v_P2Id = Guid.NewGuid();
        Guid v_PartyId = Guid.NewGuid();
        int v_ThemeId = 1;

        User v_P1 = new() { Id = v_P1Id, Elo = [new Domain.Bo.Elo { IdTheme = v_ThemeId, Value = 1000 }] };
        User v_P2 = new() { Id = v_P2Id, Elo = [new Domain.Bo.Elo { IdTheme = v_ThemeId, Value = 1000 }] };

        int v_AnswerId = 10;
        Question v_Question = new() { Id = 1, Label = "Q1", Answer = [new Answer { Id = v_AnswerId, Valid = true, Value = "A" }] };
        Question v_QuestionWithAnswers = new() { Id = 1, Label = "Q1", Answer = [new Answer { Id = v_AnswerId, Valid = true, Value = "A" }] };

        UserPartyQuestion v_Upq1 = new()
        {
            IdUser = v_P1Id,
            IdAnswer = v_AnswerId,
            DtPresentedAt = DateTime.Now.AddSeconds(-3),
            DtAnsweredAt = DateTime.Now,
            AnswersOrder = Guid.NewGuid()
        };

        SetupRoundMocks(v_PartyId, v_P1Id, v_P2Id, v_P1, v_P2, v_Question, v_QuestionWithAnswers, v_Upq1, p_Upq2: null);

        m_CalculServiceMock
            .Setup(p_C => p_C.CalculateScore(It.IsAny<DateTime>(), It.IsAny<DateTime?>()))
            .Returns(100); // rawScore1=100, P2 null → delta2=-100

        // P2 current score = 50 → after -100 → total2 = max(0, 50-100) = 0 → game ends
        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetGetAllWithScoresAsync<User>(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(User, int)> { (v_P1, 5000), (v_P2, 0) });

        // Score updates (only P1's UPQ set since P2 UPQ is null)
        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetAddAsync(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        m_NotificationServiceMock
            .Setup(p_N => p_N.NotifyPartyScoresAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<UserScore>>()))
            .Returns(Task.CompletedTask);

        // End-game: elo updates
        Theme v_Theme = new() { Id = v_ThemeId, Label = "Science" };
        m_UnitOfWorkMock
            .Setup(p_U => p_U.ThemeRepository.GetAllThemesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Theme> { v_Theme });

        m_EloServiceMock
            .Setup(p_E => p_E.ComputeEloDelta(1000, 1000))
            .Returns((20, 20));

        m_UnitOfWorkMock
            .Setup(p_U => p_U.EloRepository.UpdateValueAsync(v_P1Id, v_ThemeId, 1020, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.EloRepository.UpdateValueAsync(v_P2Id, v_ThemeId, 980, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.Save())
            .Returns(1);

        m_NotificationServiceMock
            .Setup(p_N => p_N.NotifyUserWinAsync(v_P1Id, It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        m_NotificationServiceMock
            .Setup(p_N => p_N.NotifyUserLooseAsync(v_P2Id, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Party persistence
        m_CacheServiceMock
            .Setup(p_C => p_C.SetAsync(It.IsAny<string>(), It.IsAny<Party>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetRangeByRankAsync<Question>(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Question> { v_Question });

        PartyQuestion v_SavedPq = new() { Id = 1, IdQuestion = v_Question.Id };
        Party v_SavedParty = new() { Id = Guid.NewGuid(), PartyQuestions = [v_SavedPq] };
        Mock<IMappingAddEntity<PartyBase, IEntity>> v_MappingMock = new(MockBehavior.Loose);
        v_MappingMock.Setup(p_M => p_M.MapBoEntity).Returns(v_SavedParty);

        m_UnitOfWorkMock
            .Setup(p_U => p_U.PartyRepository.CreatePartyAsync(It.IsAny<PartyBase>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_MappingMock.Object);

        // UPQ save loop: GetAsync<UserPartyQuestion> for (savedPq, p1) and (savedPq, p2)
        // v_Party.Id is already Guid.Empty at that point, so key-based filtering won't work.
        // Use a counter to return upq1 on first call of this iteration, null on second.
        int v_EndGameUpqCallCount = 0;
        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<UserPartyQuestion>(
                It.Is<string>(s => s.Contains(Guid.Empty.ToString())),
                It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                v_EndGameUpqCallCount++;
                return Task.FromResult(v_EndGameUpqCallCount == 1 ? v_Upq1 : null);
            });

        Mock<IMappingAddEntity<UserPartyQuestion, IEntity>> v_UpqMappingMock = new(MockBehavior.Loose);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserPartyQuestionRepository.CreateUserPartyQuestionAsync(It.IsAny<UserPartyQuestion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_UpqMappingMock.Object);

        m_CacheServiceMock
            .Setup(p_C => p_C.RemoveByPatternAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        RankedLogicRequest v_Request = new(v_PartyId);

        // Act — no cancellation: test runs for ~10s (3s countdown + 5s display + 2s between rounds)
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeTrue();

        m_UnitOfWorkMock.Verify(
            p_U => p_U.EloRepository.UpdateValueAsync(v_P1Id, v_ThemeId, 1020, It.IsAny<CancellationToken>()),
            Times.Once);
        m_UnitOfWorkMock.Verify(
            p_U => p_U.EloRepository.UpdateValueAsync(v_P2Id, v_ThemeId, 980, It.IsAny<CancellationToken>()),
            Times.Once);
        m_UnitOfWorkMock.Verify(p_U => p_U.PartyRepository.CreatePartyAsync(It.IsAny<PartyBase>(), It.IsAny<CancellationToken>()), Times.Once);
        m_NotificationServiceMock.Verify(p_N => p_N.NotifyUserWinAsync(v_P1Id, It.IsAny<int>()), Times.Once);
        m_NotificationServiceMock.Verify(p_N => p_N.NotifyUserLooseAsync(v_P2Id, It.IsAny<int>()), Times.Once);
    }
}
