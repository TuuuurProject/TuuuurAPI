using System.Text.Json;
using StackExchange.Redis;
using Tuuuur.Infrastructure.Services;

namespace Tuuuur.Infrastructure.Tests.Services;

/// <summary>
/// Tests for the lock and pub/sub methods added for the Ranked feature.
/// </summary>
public class CacheServiceLockPubSubTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IConnectionMultiplexer> m_ConnectionMock;
    private readonly Mock<IDatabase> m_DatabaseMock;
    private readonly CacheService m_CacheService;

    public CacheServiceLockPubSubTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);
        m_ConnectionMock = m_MockRepository.Create<IConnectionMultiplexer>();
        m_DatabaseMock = m_MockRepository.Create<IDatabase>();

        m_ConnectionMock
            .Setup(p_Cm => p_Cm.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(m_DatabaseMock.Object);

        m_CacheService = new CacheService(m_ConnectionMock.Object);
    }

    // ── AcquireLockAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task AcquireLockAsync_WhenKeyDoesNotExist_ShouldReturnTrue()
    {
        // Arrange
        const string v_Key = "lock:key";
        const string v_OwnerId = "owner1";
        TimeSpan v_Expiry = TimeSpan.FromSeconds(10);

        m_DatabaseMock
            .Setup(p_Db => p_Db.StringSetAsync(v_Key, v_OwnerId, v_Expiry, When.NotExists))
            .ReturnsAsync(true);

        // Act
        bool v_Result = await m_CacheService.AcquireLockAsync(v_Key, v_OwnerId, v_Expiry);

        // Assert
        v_Result.Should().BeTrue();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task AcquireLockAsync_WhenKeyAlreadyExists_ShouldReturnFalse()
    {
        // Arrange
        const string v_Key = "lock:key";
        const string v_OwnerId = "owner2";
        TimeSpan v_Expiry = TimeSpan.FromSeconds(10);

        m_DatabaseMock
            .Setup(p_Db => p_Db.StringSetAsync(v_Key, v_OwnerId, v_Expiry, When.NotExists))
            .ReturnsAsync(false);

        // Act
        bool v_Result = await m_CacheService.AcquireLockAsync(v_Key, v_OwnerId, v_Expiry);

        // Assert
        v_Result.Should().BeFalse();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task AcquireLockAsync_WhenCancelled_ShouldReturnFalse()
    {
        // Arrange
        CancellationTokenSource v_Cts = new();
        await v_Cts.CancelAsync();

        // Act
        bool v_Result = await m_CacheService.AcquireLockAsync("key", "owner", TimeSpan.FromSeconds(1), v_Cts.Token);

        // Assert
        v_Result.Should().BeFalse();
        m_MockRepository.VerifyAll();
    }

    // ── RefreshLockAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task RefreshLockAsync_WhenOwnerMatches_ShouldReturnTrue()
    {
        // Arrange
        const string v_Key = "lock:key";
        const string v_OwnerId = "owner1";
        TimeSpan v_Expiry = TimeSpan.FromSeconds(10);

        m_DatabaseMock
            .Setup(p_Db => p_Db.ScriptEvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<RedisKey[]>(),
                It.IsAny<RedisValue[]>(),
                CommandFlags.None))
            .ReturnsAsync(RedisResult.Create((RedisValue)1L));

        // Act
        bool v_Result = await m_CacheService.RefreshLockAsync(v_Key, v_OwnerId, v_Expiry);

        // Assert
        v_Result.Should().BeTrue();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task RefreshLockAsync_WhenOwnerDoesNotMatch_ShouldReturnFalse()
    {
        // Arrange
        const string v_Key = "lock:key";
        const string v_OwnerId = "someone-else";
        TimeSpan v_Expiry = TimeSpan.FromSeconds(10);

        m_DatabaseMock
            .Setup(p_Db => p_Db.ScriptEvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<RedisKey[]>(),
                It.IsAny<RedisValue[]>(),
                CommandFlags.None))
            .ReturnsAsync(RedisResult.Create((RedisValue)0L));

        // Act
        bool v_Result = await m_CacheService.RefreshLockAsync(v_Key, v_OwnerId, v_Expiry);

        // Assert
        v_Result.Should().BeFalse();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task RefreshLockAsync_WhenCancelled_ShouldReturnFalse()
    {
        // Arrange
        CancellationTokenSource v_Cts = new();
        await v_Cts.CancelAsync();

        // Act
        bool v_Result = await m_CacheService.RefreshLockAsync("key", "owner", TimeSpan.FromSeconds(1), v_Cts.Token);

        // Assert
        v_Result.Should().BeFalse();
        m_MockRepository.VerifyAll();
    }

    // ── ReleaseLockAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ReleaseLockAsync_WhenOwnerMatches_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        const string v_Key = "lock:key";
        const string v_OwnerId = "owner1";

        m_DatabaseMock
            .Setup(p_Db => p_Db.ScriptEvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<RedisKey[]>(),
                It.IsAny<RedisValue[]>(),
                CommandFlags.None))
            .ReturnsAsync(RedisResult.Create((RedisValue)1L));

        // Act
        bool v_Result = await m_CacheService.ReleaseLockAsync(v_Key, v_OwnerId);

        // Assert
        v_Result.Should().BeTrue();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task ReleaseLockAsync_WhenOwnerDoesNotMatch_ShouldReturnFalse()
    {
        // Arrange
        const string v_Key = "lock:key";
        const string v_OwnerId = "different-owner";

        m_DatabaseMock
            .Setup(p_Db => p_Db.ScriptEvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<RedisKey[]>(),
                It.IsAny<RedisValue[]>(),
                CommandFlags.None))
            .ReturnsAsync(RedisResult.Create((RedisValue)0L));

        // Act
        bool v_Result = await m_CacheService.ReleaseLockAsync(v_Key, v_OwnerId);

        // Assert
        v_Result.Should().BeFalse();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task ReleaseLockAsync_WhenCancelled_ShouldReturnFalse()
    {
        // Arrange
        CancellationTokenSource v_Cts = new();
        await v_Cts.CancelAsync();

        // Act
        bool v_Result = await m_CacheService.ReleaseLockAsync("key", "owner", v_Cts.Token);

        // Assert
        v_Result.Should().BeFalse();
        m_MockRepository.VerifyAll();
    }

    // ── PublishAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task PublishAsync_ShouldSerializeAndPublishToChannel()
    {
        // Arrange
        const string v_Channel = "test:channel";
        bool v_Payload = true;
        string v_Json = JsonSerializer.Serialize(v_Payload);

        Mock<ISubscriber> v_SubscriberMock = m_MockRepository.Create<ISubscriber>();

        m_ConnectionMock
            .Setup(p_Cm => p_Cm.GetSubscriber(It.IsAny<object>()))
            .Returns(v_SubscriberMock.Object);

        v_SubscriberMock
            .Setup(p_S => p_S.PublishAsync(
                RedisChannel.Literal(v_Channel),
                v_Json,
                CommandFlags.None))
            .ReturnsAsync(0L);

        // Act
        await m_CacheService.PublishAsync(v_Channel, v_Payload);

        // Assert
        m_MockRepository.VerifyAll();
    }

    // ── ExistsAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ExistsAsync_WhenKeyExists_ShouldReturnTrue()
    {
        // Arrange
        const string v_Key = "some:key";

        m_DatabaseMock
            .Setup(p_Db => p_Db.KeyExistsAsync((RedisKey)v_Key, CommandFlags.None))
            .ReturnsAsync(true);

        // Act
        bool v_Result = await m_CacheService.ExistsAsync(v_Key);

        // Assert
        v_Result.Should().BeTrue();
        m_MockRepository.VerifyAll();
    }

    // ── IncrementAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task IncrementAsync_ShouldCallStringIncrementAndReturnValue()
    {
        // Arrange
        const string v_Key = "counter";

        m_DatabaseMock
            .Setup(p_Db => p_Db.StringIncrementAsync(v_Key, 1L, CommandFlags.None))
            .ReturnsAsync(5L);

        // Act
        long v_Result = await m_CacheService.IncrementAsync(v_Key, 1);

        // Assert
        v_Result.Should().Be(5L);
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task IncrementAsync_WhenCancelled_ShouldReturnZero()
    {
        // Arrange
        CancellationTokenSource v_Cts = new();
        await v_Cts.CancelAsync();

        // Act
        long v_Result = await m_CacheService.IncrementAsync("key", 1, v_Cts.Token);

        // Assert
        v_Result.Should().Be(0);
        m_MockRepository.VerifyAll();
    }
}
