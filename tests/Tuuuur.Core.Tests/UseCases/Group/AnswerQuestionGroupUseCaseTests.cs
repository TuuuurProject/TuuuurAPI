using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Group;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.Group;

public class AnswerQuestionGroupUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;
    private readonly Mock<IGroupNotificationService> m_GroupNotificationServiceMock;

    private readonly AnswerQuestionGroupUseCase m_UseCase;

    public AnswerQuestionGroupUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Loose);
        m_UnitOfWorkMock = m_MockRepository.Create<IUnitOfWork>();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();
        m_GroupNotificationServiceMock = m_MockRepository.Create<IGroupNotificationService>();
        Mock<ILogger<AnswerQuestionGroupUseCase>> v_LoggerMock = new();

        m_UseCase = new AnswerQuestionGroupUseCase(
            m_UnitOfWorkMock.Object,
            m_CacheServiceMock.Object,
            m_GroupNotificationServiceMock.Object,
            v_LoggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_PartyNotFoundInCache_ReturnsError()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        AnswerQuestionGroupPartyRequest v_Request = new(1, v_UserId);

        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        v_Result.Errors.Should().NotBeNull();
        v_Result.Errors.Should().HaveCount(1);
        v_Result.Errors!.First().Code.Should().Be(DomainErrors.Data.NotFound);

        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_PartyNotInProgress_ReturnsError()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        string v_PartyCode = "ABC123";
        GroupParty v_Party = new GroupParty
        {
            Code = v_PartyCode,
            InProgress = false,
            IdUserHost = v_UserId
        };
        AnswerQuestionGroupPartyRequest v_Request = new(1, v_UserId);

        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_PartyCode);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<GroupParty>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        v_Result.Errors.Should().NotBeNull();
        v_Result.Errors.Should().HaveCount(1);

        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_InvalidAnswerId_ReturnsError()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        const string v_PartyCode = "ABC123";
        GroupParty v_Party = new()
        {
            Code = v_PartyCode,
            InProgress = true,
            IdUserHost = v_UserId
        };
        Question v_Question = new()
        {
            Id = 1,
            Answer =
            [
                new Answer { Id = 10 },
                new Answer { Id = 11 }
            ]
        };
        int v_InvalidAnswerId = 999;
        AnswerQuestionGroupPartyRequest v_Request = new(v_InvalidAnswerId, v_UserId);

        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_PartyCode);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<GroupParty>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SortedSetGetByIndexAsync<Question>(
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(v_Question);
        m_UnitOfWorkMock.Setup(p_U => p_U.QuestionRepository.GetQuestionByIdWithAnswerAsync(v_Question.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Question);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        v_Result.Errors.Should().NotBeNull();
        v_Result.Errors.Should().HaveCount(1);
        v_Result.Errors!.First().Code.Should().Be(DomainErrors.Data.NotFound);

        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_ValidAnswer_NotAllPlayersAnswered_Success()
    {
        // Arrange
        List<User> v_Users = BoFactory.CreateUser().Generate(6);
        Guid v_UserId = v_Users.FirstOrDefault()!.Id;
        const string v_PartyCode = "ABC123";
        const int v_ValidAnswerId = 10;
        GroupParty v_Party = new()
        {
            Code = v_PartyCode,
            InProgress = true,
            IdUserHost = v_UserId
        };
        Question v_Question = new()
        {
            Id = 1,
            Answer =
            [
                new Answer { Id = 10 },
                new Answer { Id = 11 }
            ]
        };
        UserPartyQuestion v_UserPartyQuestion = new()
        {
            IdUser = v_UserId,
            IdPartyQuestion = 1
        };
        AnswerQuestionGroupPartyRequest v_Request = new(v_ValidAnswerId, v_UserId);

        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_PartyCode);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<GroupParty>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SortedSetGetByIndexAsync<Question>(
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(v_Question);
        m_UnitOfWorkMock.Setup(p_U => p_U.QuestionRepository.GetQuestionByIdWithAnswerAsync(v_Question.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Question);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<UserPartyQuestion>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_UserPartyQuestion);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetAsync(
                It.IsAny<string>(),
                It.IsAny<UserPartyQuestion>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetAddAsync(
                It.IsAny<string>(),
                v_UserId, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(true);
        m_CacheServiceMock.SetupSequence(p_Cs => p_Cs.SetLengthAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1) // Answered count
            .ReturnsAsync(2); // Total players - not all answered yet
        m_GroupNotificationServiceMock.Setup(p_Gn => p_Gn.NotifyUserSendAnswerAsync(v_PartyCode, It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetMembersAsync<User>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { new User { Id = v_UserId } });
        m_CacheServiceMock
            .Setup(p_Cs => p_Cs.SetMembersAsync<User>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Users);
        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();

        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_ValidAnswer_AllPlayersAnswered_PublishesNotification()
    {
        // Arrange
        List<User> v_Users = BoFactory.CreateUser().Generate(6);
        const string v_PartyCode = "ABC123";
        const int v_ValidAnswerId = 10;
        GroupParty v_Party = new()
        {
            Code = v_PartyCode,
            InProgress = true,
            IdUserHost = v_Users.First().Id
        };
        Question v_Question = new()
        {
            Id = 1,
            Answer =
            [
                new Answer { Id = 10 },
                new Answer { Id = 11 }
            ]
        };
        UserPartyQuestion v_UserPartyQuestion = new()
        {
            IdUser = v_Users.First().Id,
            IdPartyQuestion = 1
        };
        AnswerQuestionGroupPartyRequest v_Request = new(v_ValidAnswerId, v_Users.First().Id);


        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_PartyCode);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<GroupParty>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SortedSetGetByIndexAsync<Question>(
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(v_Question);
        m_UnitOfWorkMock.Setup(p_U => p_U.QuestionRepository.GetQuestionByIdWithAnswerAsync(v_Question.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Question);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<UserPartyQuestion>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_UserPartyQuestion);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetAsync(
                It.IsAny<string>(),
                It.IsAny<UserPartyQuestion>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetAddAsync(
                It.IsAny<string>(),
                v_Users.First().Id,
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(true);
        m_CacheServiceMock.SetupSequence(p_Cs => p_Cs.SetLengthAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2) // Answered count
            .ReturnsAsync(2); // Total players
        m_CacheServiceMock
            .Setup(p_Cs => p_Cs.SetMembersAsync<User>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Users);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.PublishAsync(
                It.IsAny<string>(),
                true,
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask);
        m_GroupNotificationServiceMock.Setup(p_Gn => p_Gn.NotifyUserSendAnswerAsync(v_PartyCode, It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();

        m_MockRepository.VerifyAll();
        m_CacheServiceMock.Verify(p_Cs => p_Cs.PublishAsync(
            It.IsAny<string>(),
            true,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
