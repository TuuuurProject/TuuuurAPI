using Microsoft.Extensions.Logging;
using Tuuuur.Core.Configuration;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Ranked;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;

namespace Tuuuur.Core.Tests.UseCases.Ranked;

public class AnswerQuestionRankedUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;
    private readonly Mock<IRankedNotificationService> m_NotificationServiceMock;
    private readonly AnswerQuestionRankedUseCase m_UseCase;

    public AnswerQuestionRankedUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_UnitOfWorkMock = m_MockRepository.Create<IUnitOfWork>();
        Mock<ILogger<AnswerQuestionRankedUseCase>> v_LoggerMock = m_MockRepository.Create<ILogger<AnswerQuestionRankedUseCase>>();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();
        m_NotificationServiceMock = m_MockRepository.Create<IRankedNotificationService>();

        m_UseCase = new AnswerQuestionRankedUseCase(
            m_UnitOfWorkMock.Object,
            m_CacheServiceMock.Object,
            m_NotificationServiceMock.Object,
            new RankedConfiguration(),
            v_LoggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPartyIdNotFoundInCache_ShouldReturnError()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();

        // GetAsync<string> returns an invalid/null guid string
        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("not-a-guid");

        AnswerQuestionRankedRequest v_Request = new(1, v_UserId);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenAnswerNotInQuestion_ShouldReturnError()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        Guid v_PartyId = Guid.NewGuid();
        int v_QuestionId = 7;
        int v_WrongAnswerId = 999;

        Party v_Party = new()
        {
            Id = v_PartyId,
            PartyUsers = [new PartyUser { IdUser = v_UserId, User = new User { Id = v_UserId, Elo = [] } }],
            PartyQuestions = []
        };

        Question v_Question = new()
        {
            Id = v_QuestionId,
            Answer = [new Answer { Id = 1, Valid = true }]
        };

        Question v_CachedQuestion = new() { Id = v_QuestionId, Answer = [] };

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_PartyId.ToString());

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<Party>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetGetByIndexAsync<Question>(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_CachedQuestion);

        m_UnitOfWorkMock
            .Setup(p_U => p_U.QuestionRepository.GetQuestionByIdWithAnswerAsync(v_QuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Question);

        AnswerQuestionRankedRequest v_Request = new(v_WrongAnswerId, v_UserId);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenValidAnswer_ShouldSaveAndNotify()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        Guid v_PartyId = Guid.NewGuid();
        int v_QuestionId = 5;
        int v_AnswerId = 10;

        User v_User = new() { Id = v_UserId, NickName = "Test", Elo = [] };
        Party v_Party = new()
        {
            Id = v_PartyId,
            PartyUsers = [new PartyUser { IdUser = v_UserId, User = v_User }],
            PartyQuestions = []
        };

        Question v_CachedQuestion = new() { Id = v_QuestionId, Answer = [] };
        Question v_FullQuestion = new()
        {
            Id = v_QuestionId,
            Answer = [new Answer { Id = v_AnswerId, Valid = true }]
        };
        UserPartyQuestion v_UpqInCache = new() { IdUser = v_UserId, AnswersOrder = Guid.NewGuid() };

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_PartyId.ToString());

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<Party>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetGetByIndexAsync<Question>(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_CachedQuestion);

        m_UnitOfWorkMock
            .Setup(p_U => p_U.QuestionRepository.GetQuestionByIdWithAnswerAsync(v_QuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_FullQuestion);

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<UserPartyQuestion>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_UpqInCache);

        m_CacheServiceMock
            .Setup(p_C => p_C.SetAsync(It.IsAny<string>(), It.IsAny<UserPartyQuestion>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        m_CacheServiceMock
            .Setup(p_C => p_C.SetAddAsync(It.IsAny<string>(), v_UserId, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        m_CacheServiceMock
            .Setup(p_C => p_C.SetLengthAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1L);

        m_CacheServiceMock
            .Setup(p_C => p_C.PublishAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Only 1 player so publish fires, then notification fires
        m_NotificationServiceMock
            .Setup(p_N => p_N.NotifyUserSendAnswerAsync(v_PartyId, v_User))
            .Returns(Task.CompletedTask);

        AnswerQuestionRankedRequest v_Request = new(v_AnswerId, v_UserId);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeTrue();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenAllPlayersAnswered_ShouldPublishChannel()
    {
        // Arrange
        Guid v_UserId1 = Guid.NewGuid();
        Guid v_UserId2 = Guid.NewGuid();
        Guid v_PartyId = Guid.NewGuid();
        int v_QuestionId = 3;
        int v_AnswerId = 20;

        User v_User1 = new() { Id = v_UserId1, NickName = "P1", Elo = [] };
        User v_User2 = new() { Id = v_UserId2, NickName = "P2", Elo = [] };
        Party v_Party = new()
        {
            Id = v_PartyId,
            PartyUsers =
            [
                new PartyUser { IdUser = v_UserId1, User = v_User1 },
                new PartyUser { IdUser = v_UserId2, User = v_User2 }
            ],
            PartyQuestions = []
        };

        Question v_CachedQuestion = new() { Id = v_QuestionId, Answer = [] };
        Question v_FullQuestion = new()
        {
            Id = v_QuestionId,
            Answer = [new Answer { Id = v_AnswerId, Valid = true }]
        };
        UserPartyQuestion v_Upq = new() { IdUser = v_UserId1, AnswersOrder = Guid.NewGuid() };

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_PartyId.ToString());

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<Party>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetGetByIndexAsync<Question>(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_CachedQuestion);

        m_UnitOfWorkMock
            .Setup(p_U => p_U.QuestionRepository.GetQuestionByIdWithAnswerAsync(v_QuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_FullQuestion);

        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<UserPartyQuestion>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Upq);

        m_CacheServiceMock
            .Setup(p_C => p_C.SetAsync(It.IsAny<string>(), It.IsAny<UserPartyQuestion>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        m_CacheServiceMock
            .Setup(p_C => p_C.SetAddAsync(It.IsAny<string>(), v_UserId1, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Both players answered
        m_CacheServiceMock
            .Setup(p_C => p_C.SetLengthAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2L);

        m_CacheServiceMock
            .Setup(p_C => p_C.PublishAsync(It.IsAny<string>(), true, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        m_NotificationServiceMock
            .Setup(p_N => p_N.NotifyUserSendAnswerAsync(v_PartyId, v_User1))
            .Returns(Task.CompletedTask);

        AnswerQuestionRankedRequest v_Request = new(v_AnswerId, v_UserId1);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeTrue();
        m_CacheServiceMock.Verify(p_C => p_C.PublishAsync(It.IsAny<string>(), true, It.IsAny<CancellationToken>()), Times.Once);
        m_MockRepository.VerifyAll();
    }
}
