using MediatR;
using Microsoft.Extensions.DependencyInjection;
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

public class CreateRankedPartyUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;
    private readonly Mock<IServiceScopeFactory> m_ScopeFactoryMock;
    private readonly Mock<IRankedNotificationService> m_NotificationServiceMock;
    private readonly RankedConfiguration m_RankedConfig;
    private readonly CreateRankedPartyUseCase m_UseCase;

    public CreateRankedPartyUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_UnitOfWorkMock = m_MockRepository.Create<IUnitOfWork>();
        Mock<ILogger<CreateRankedPartyUseCase>> v_LoggerMock = new Mock<ILogger<CreateRankedPartyUseCase>>();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();
        m_ScopeFactoryMock = m_MockRepository.Create<IServiceScopeFactory>();
        m_NotificationServiceMock = m_MockRepository.Create<IRankedNotificationService>();
        m_RankedConfig = new RankedConfiguration { InitialRankedScore = 5000 };

        m_UseCase = new CreateRankedPartyUseCase(
            m_UnitOfWorkMock.Object,
            v_LoggerMock.Object,
            m_CacheServiceMock.Object,
            m_ScopeFactoryMock.Object,
            m_NotificationServiceMock.Object,
            m_RankedConfig);
    }

    [Fact]
    public async Task Handle_WhenPlayer1NotFound_ShouldReturnError()
    {
        // Arrange
        User v_P1 = new() { Id = Guid.NewGuid(), Elo = [] };
        User v_P2 = new() { Id = Guid.NewGuid(), Elo = [] };

        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_P1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        CreateRankedPartyRequest v_Request = new(v_P1, v_P2);

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
        User v_P1 = new() { Id = Guid.NewGuid(), Elo = [] };
        User v_P2 = new() { Id = Guid.NewGuid(), Elo = [] };

        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_P1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_P1);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_P2.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        CreateRankedPartyRequest v_Request = new(v_P1, v_P2);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeFalse();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenBothPlayersFound_ShouldCreatePartyAndNotify()
    {
        // Arrange
        User v_P1 = new() { Id = Guid.NewGuid(), Elo = [], NickName = "P1" };
        User v_P2 = new() { Id = Guid.NewGuid(), Elo = [], NickName = "P2" };

        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_P1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_P1);
        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_P2.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_P2);

        // Cache operations
        m_CacheServiceMock
            .Setup(p_C => p_C.SetAsync(It.IsAny<string>(), It.IsAny<Party>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        m_CacheServiceMock
            .Setup(p_C => p_C.SetAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        m_CacheServiceMock
            .Setup(p_C => p_C.SetAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetAddAsync(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        m_NotificationServiceMock
            .Setup(p_N => p_N.NotifyMatchFoundAsync(v_P1, v_P2, It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Scope factory for background task - use loose here since background task runs independently
        Mock<IServiceScope> v_ScopeMock = new();
        Mock<IServiceProvider> v_ServiceProviderMock = new();
        Mock<IMediator> v_MediatorMock = new();
        v_MediatorMock
            .Setup(p_M => p_M.Send(It.IsAny<RankedLogicRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmptyResponse());
        v_ServiceProviderMock
            .Setup(p_Sp => p_Sp.GetService(typeof(IMediator)))
            .Returns(v_MediatorMock.Object);
        v_ScopeMock.Setup(p_S => p_S.ServiceProvider).Returns(v_ServiceProviderMock.Object);
        m_ScopeFactoryMock
            .Setup(p_F => p_F.CreateScope())
            .Returns(v_ScopeMock.Object);

        CreateRankedPartyRequest v_Request = new(v_P1, v_P2);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeTrue();
        m_UnitOfWorkMock.Verify(p_U => p_U.UserRepository.GetUserByIdAsync(v_P1.Id, It.IsAny<CancellationToken>()), Times.Once);
        m_UnitOfWorkMock.Verify(p_U => p_U.UserRepository.GetUserByIdAsync(v_P2.Id, It.IsAny<CancellationToken>()), Times.Once);
        m_NotificationServiceMock.Verify(p_N => p_N.NotifyMatchFoundAsync(v_P1, v_P2, It.IsAny<Guid>()), Times.Once);
    }
}
