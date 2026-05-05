using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Ranked;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;

namespace Tuuuur.Core.Tests.UseCases.Ranked;

public class JoinSearchOpponentUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;
    private readonly JoinSearchOpponentUseCase m_UseCase;

    public JoinSearchOpponentUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_UnitOfWorkMock = m_MockRepository.Create<IUnitOfWork>();
        Mock<ILogger<JoinSearchOpponentUseCase>> v_LoggerMock = m_MockRepository.Create<ILogger<JoinSearchOpponentUseCase>>();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();

        m_UseCase = new JoinSearchOpponentUseCase(
            m_UnitOfWorkMock.Object,
            v_LoggerMock.Object,
            m_CacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnError()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        JoinSearchOpponentRequest v_Request = new(v_UserId);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Should().NotBeNull();
        v_Result.Success.Should().BeFalse();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyInMatchmakingAndHasParty_ShouldReturnError()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        User v_User = new() { Id = v_UserId, Elo = [new Elo { Value = 1000 }] };

        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);

        // User has an active party
        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid().ToString());

        // User is also in matchmaking queue
        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetGetAllWithScoresAsync<User>(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(User, int)> { (v_User, 1000) });
        
        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<Guid>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_UserId);

        JoinSearchOpponentRequest v_Request = new(v_UserId);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Should().NotBeNull();
        v_Result.Success.Should().BeFalse();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenUserNotInQueueAndNoActiveParty_ShouldAddToMatchmakingAndReturnSuccess()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        User v_User = new() { Id = v_UserId, Elo = [] };

        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);

        // No active party
        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        // Not in queue
        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetGetAllWithScoresAsync<User>(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(User, int)>());

        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetAddAsync(It.IsAny<string>(), v_User, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        m_CacheServiceMock
            .Setup(p_C => p_C.HashSetAsync(It.IsAny<string>(), v_UserId.ToString(), It.IsAny<long>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        m_CacheServiceMock
            .Setup(p_C => p_C.GetAsync<Guid>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty);

        JoinSearchOpponentRequest v_Request = new(v_UserId);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Should().NotBeNull();
        v_Result.Success.Should().BeTrue();
        m_MockRepository.VerifyAll();
    }
}
