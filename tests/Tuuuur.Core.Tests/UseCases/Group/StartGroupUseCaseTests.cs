using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Group;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.Group;

public class StartGroupUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<IGroupNotificationService> m_GroupPartyNotificationServiceMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;
    private readonly Mock<IServiceScopeFactory> m_ServiceScopeFactoryMock;

    private readonly StartGroupUseCase m_UseCase;

    public StartGroupUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_UnitOfWorkMock = m_MockRepository.Create<IUnitOfWork>();
        Mock<ILogger<StartGroupUseCase>> v_LoggerMock = m_MockRepository.Create<ILogger<StartGroupUseCase>>();
        m_GroupPartyNotificationServiceMock = m_MockRepository.Create<IGroupNotificationService>();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();
        m_ServiceScopeFactoryMock = m_MockRepository.Create<IServiceScopeFactory>();

        m_UseCase = new StartGroupUseCase(m_UnitOfWorkMock.Object, v_LoggerMock.Object, m_CacheServiceMock.Object, m_GroupPartyNotificationServiceMock.Object, m_ServiceScopeFactoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExpectedAsync()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        GroupParty v_Party = BoFactory.CreateGroupParty().Generate();
        v_Party.IdUserHost = v_User.Id;
        v_Party.InProgress = false;
        v_Party.NbQuestions = 20;
        v_Party.PartyTheme = [new PartyTheme() { IdTheme = 1 }];
        v_Party.PartyDifficulty = [new PartyDifficulty() { IdDifficulty = 1 }];

        List<Question> v_Questions = BoFactory.CreateQuestion().Generate(v_Party.NbQuestions);

        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_User);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_Party.Code);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.GetAsync<GroupParty>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_Party);
        m_UnitOfWorkMock.Setup(p_U => p_U.QuestionRepository.GetQuestionsByThemesIdsAndDifficultiesIdsAndNumberOfQuestionsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_Questions);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SortedSetAddAsync(It.IsAny<string>(), It.IsAny<Question>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetAsync(It.IsAny<string>(), It.IsAny<GroupParty>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SetMembersAsync<Guid>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([v_User.Id]);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUsersByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([v_User]);
        m_CacheServiceMock.Setup(p_Cs => p_Cs.SortedSetAddAsync(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        m_GroupPartyNotificationServiceMock.Setup(p_Ns => p_Ns.NotifyPartyStartedAsync(It.IsAny<string>(), It.IsAny<GroupParty>())).Returns(Task.CompletedTask);

        Mock<IServiceScope> v_ServiceScopeMock = m_MockRepository.Create<IServiceScope>();
        Mock<IServiceProvider> v_ServiceProviderMock = m_MockRepository.Create<IServiceProvider>();
        Mock<IMediator> v_MediatorMock = m_MockRepository.Create<IMediator>();

        v_ServiceScopeMock.Setup(p_S => p_S.ServiceProvider).Returns(v_ServiceProviderMock.Object);
        v_ServiceProviderMock.Setup(p_Sp => p_Sp.GetService(typeof(IMediator))).Returns(v_MediatorMock.Object);
        v_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<GroupLogicRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EmptyResponse());
        m_ServiceScopeFactoryMock.Setup(p_Ssf => p_Ssf.CreateScope()).Returns(v_ServiceScopeMock.Object);

        StartGroupPartyRequest v_Request = new(v_User.Email);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Wait a bit for the background task to complete
        await Task.Delay(1000);

        // Assert
        Assert.NotNull(v_Result);
        Assert.True(v_Result.Success);
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
    }
}