using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Group;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Services;
using Tuuuur.Domain.Notifications;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.Group;

public class GroupLogicUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;
    private readonly Mock<IGroupNotificationService> m_GroupNotificationServiceMock;
    private readonly Mock<ICalculService> m_CalculServiceMock;
    private readonly Mock<IMediator> m_MediatorMock;
    private readonly Mock<ILogger<GroupLogicUseCase>> m_LoggerMock;

    private readonly GroupLogicUseCase m_UseCase;

    public GroupLogicUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_UnitOfWorkMock = m_MockRepository.Create<IUnitOfWork>();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();
        m_GroupNotificationServiceMock = m_MockRepository.Create<IGroupNotificationService>();
        m_CalculServiceMock = m_MockRepository.Create<ICalculService>();
        m_MediatorMock = m_MockRepository.Create<IMediator>();
        m_LoggerMock = m_MockRepository.Create<ILogger<GroupLogicUseCase>>();

        m_UseCase = new GroupLogicUseCase(
            m_UnitOfWorkMock.Object,
            m_CacheServiceMock.Object,
            m_GroupNotificationServiceMock.Object,
            m_CalculServiceMock.Object,
            m_MediatorMock.Object,
            m_LoggerMock.Object);
    }

    [Fact]
    public async Task Handle_ExpectedAsync()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        GroupParty v_Party = BoFactory.CreateGroupParty().Generate();
        v_Party.ScoreEachRound = false;
        v_Party.NbQuestions = 5;

        List<Question> v_Questions = BoFactory.CreateQuestion().Generate(5);
        List<Answer> v_Answers = BoFactory.CreateAnswer(v_Questions[0].Id).Generate(4);
        v_Questions[0].Answer = v_Answers;

        const int v_CurrentIndex = 0;

        m_CacheServiceMock.Setup(p_C => p_C.GetAsync<GroupParty>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Party);
        m_CacheServiceMock.Setup(p_C => p_C.GetAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_CurrentIndex);
        m_CacheServiceMock.Setup(p_C => p_C.SortedSetGetByIndexAsync<Question>(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Questions[v_CurrentIndex]);

        m_UnitOfWorkMock.Setup(p_U => p_U.QuestionRepository.GetQuestionByIdWithAnswerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_Questions[v_CurrentIndex]);

        m_CacheServiceMock.Setup(p_C => p_C.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        m_GroupNotificationServiceMock.Setup(p_G => p_G.NotifyCountdownAsync(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        m_CacheServiceMock.Setup(p_C => p_C.SetMembersAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([v_User.Id]);

        m_CacheServiceMock.Setup(p_C => p_C.SetAsync(It.IsAny<string>(), It.IsAny<UserPartyQuestion>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        m_CacheServiceMock.Setup(p_C => p_C.SubscribeAndWaitAsync<bool>(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        m_GroupNotificationServiceMock.Setup(p_G => p_G.NotifyPartyQuestionSend(It.IsAny<int>(), It.IsAny<GroupQuestion>()))
            .Returns(Task.CompletedTask);

        m_CacheServiceMock.Setup(p_C => p_C.SortedSetGetAllWithScoresAsync<User>(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([(v_User, 0)]);

        m_CalculServiceMock.Setup(p_C => p_C.CalculateScore(It.IsAny<DateTime>(), It.IsAny<DateTime?>()))
            .Returns(100);

        m_CacheServiceMock.Setup(p_C => p_C.GetAsync<UserPartyQuestion>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserPartyQuestion
            {
                IdUser = v_User.Id,
                IdAnswer = v_Answers[0].Id,
                DtPresentedAt = DateTime.Now,
                DtAnsweredAt = DateTime.Now.AddSeconds(5)
            });

        m_CacheServiceMock.Setup(p_C => p_C.SortedSetAddAsync(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        m_GroupNotificationServiceMock.Setup(p_G => p_G.NotifyPartyQuestionAnswerSend(It.IsAny<int>(), It.IsAny<GroupQuestion>()))
            .Returns(Task.CompletedTask);

        m_CacheServiceMock.Setup(p_C => p_C.SetAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<GroupLogicRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmptyResponse());

        GroupLogicRequest v_Request = new(v_Party.Code);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Should().NotBeNull();
        v_Result.Success.Should().BeTrue();
        m_MediatorMock.Verify(p_M => p_M.Send(It.IsAny<GroupLogicRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}