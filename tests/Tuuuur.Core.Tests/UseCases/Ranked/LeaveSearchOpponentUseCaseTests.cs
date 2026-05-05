using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Ranked;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;

namespace Tuuuur.Core.Tests.UseCases.Ranked;

public class LeaveSearchOpponentUseCaseTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ICacheService> m_CacheServiceMock;
    private readonly LeaveSearchOpponentUseCase m_UseCase;

    public LeaveSearchOpponentUseCaseTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_UnitOfWorkMock = m_MockRepository.Create<IUnitOfWork>();
        Mock<ILogger<LeaveSearchOpponentUseCase>> v_LoggerMock = m_MockRepository.Create<ILogger<LeaveSearchOpponentUseCase>>();
        m_CacheServiceMock = m_MockRepository.Create<ICacheService>();

        m_UseCase = new LeaveSearchOpponentUseCase(
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

        LeaveSeachOpponentRequest v_Request = new(v_UserId);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Should().NotBeNull();
        v_Result.Success.Should().BeFalse();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenUserNotInMatchmakingQueue_ShouldReturnSuccessWithoutRemoval()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        User v_User = new() { Id = v_UserId, Elo = [] };

        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);

        // User is not in the queue
        User v_OtherUser = new() { Id = Guid.NewGuid(), Elo = [] };
        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetGetAllWithScoresAsync<User>(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(User, int)> { (v_OtherUser, 900) });

        LeaveSeachOpponentRequest v_Request = new(v_UserId);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeTrue();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenUserInMatchmakingQueue_ShouldRemoveAndReturnSuccess()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();
        User v_User = new() { Id = v_UserId, Elo = [] };

        m_UnitOfWorkMock
            .Setup(p_U => p_U.UserRepository.GetUserByIdAsync(v_UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_User);

        // User is in the queue
        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetGetAllWithScoresAsync<User>(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(User, int)> { (v_User, 1000) });

        m_CacheServiceMock
            .Setup(p_C => p_C.SortedSetRemoveAsync(It.IsAny<string>(), v_User, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        m_CacheServiceMock
            .Setup(p_C => p_C.HashDeleteAsync(It.IsAny<string>(), v_UserId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        LeaveSeachOpponentRequest v_Request = new(v_UserId);

        // Act
        EmptyResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        v_Result.Success.Should().BeTrue();
        m_MockRepository.VerifyAll();
    }
}
