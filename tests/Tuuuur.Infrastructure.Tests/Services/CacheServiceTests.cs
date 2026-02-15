using System.Text.Json;
using StackExchange.Redis;
using Tuuuur.Infrastructure.Services;

namespace Tuuuur.Infrastructure.Tests.Services;

public class CacheServiceTests
{
    private readonly MockRepository m_MockRepository;
    private readonly Mock<IDatabase> m_DatabaseMock;
    private readonly CacheService m_CacheService;

    public CacheServiceTests()
    {
        m_MockRepository = new MockRepository(MockBehavior.Strict);

        Mock<IConnectionMultiplexer> v_ConnectionMultiplexerMock = m_MockRepository.Create<IConnectionMultiplexer>();
        m_DatabaseMock = m_MockRepository.Create<IDatabase>();

        v_ConnectionMultiplexerMock
            .Setup(p_Cm => p_Cm.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(m_DatabaseMock.Object);

        m_CacheService = new CacheService(v_ConnectionMultiplexerMock.Object);
    }

    private record TestObject(int Id, string Name);

    [Fact]
    public void CreateKey_ShouldJoinWithColons()
    {
        // Act
        string v_Result = m_CacheService.CreateKey("parties", "123", "users");

        // Assert
        v_Result.Should().Be("parties:123:users");
    }

    [Fact]
    public async Task SetAsync_WithoutExpiration_ShouldCallStringSetAsync()
    {
        // Arrange
        const string v_Key = "test:key";
        TestObject v_Data = new TestObject(1, "Test");
        string v_Json = JsonSerializer.Serialize(v_Data);

        m_DatabaseMock
            .Setup(p_Db => p_Db.StringSetAsync(
                v_Key,
                v_Json,
                TimeSpan.FromHours(24),
                default(ValueCondition),
                CommandFlags.None
            ))
            .ReturnsAsync(true);

        // Act
        await m_CacheService.SetAsync(v_Key, v_Data, TimeSpan.Zero, CancellationToken.None);

        // Assert
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task SetAsync_WithExpiration_ShouldCallStringSetAsyncWithTimeSpanAsync()
    {
        // Arrange
        const string v_Key = "test:key";
        TestObject v_Data = new TestObject(1, "Test");
        string v_Json = JsonSerializer.Serialize(v_Data);
        TimeSpan v_Expiration = TimeSpan.FromMinutes(10);

        m_DatabaseMock
            .Setup(p_Db => p_Db.StringSetAsync(
                v_Key,
                v_Json,
                v_Expiration,
                default(ValueCondition),
                CommandFlags.None
            ))
            .ReturnsAsync(true);

        // Act
        await m_CacheService.SetAsync(v_Key, v_Data, v_Expiration, CancellationToken.None);

        // Assert
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task GetAsync_KeyExists_ShouldReturnDeserializedObjectAsync()
    {
        // Arrange
        const string v_Key = "test:key";
        TestObject v_ExpectedData = new TestObject(1, "Test");
        string v_Json = JsonSerializer.Serialize(v_ExpectedData);

        m_DatabaseMock
            .Setup(p_Db => p_Db.StringGetAsync(v_Key, CommandFlags.None))
            .ReturnsAsync(v_Json);

        // Act
        TestObject v_Result = await m_CacheService.GetAsync<TestObject>(v_Key, CancellationToken.None);

        // Assert
        v_Result.Should().NotBeNull();
        v_Result.Should().BeEquivalentTo(v_ExpectedData);
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task GetAsync_KeyDoesNotExist_ShouldReturnNullAsync()
    {
        // Arrange
        const string v_Key = "test:missing";

        m_DatabaseMock
            .Setup(p_Db => p_Db.StringGetAsync(v_Key, CommandFlags.None))
            .ReturnsAsync(RedisValue.Null);

        // Act
        TestObject v_Result = await m_CacheService.GetAsync<TestObject>(v_Key, CancellationToken.None);

        // Assert
        v_Result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallKeyDeleteAsync()
    {
        // Arrange
        const string v_Key = "test:key";
        m_DatabaseMock
            .Setup(p_Db => p_Db.KeyDeleteAsync(v_Key, CommandFlags.None))
            .ReturnsAsync(true);

        // Act
        bool v_Result = await m_CacheService.RemoveAsync(v_Key, CancellationToken.None);

        // Assert
        v_Result.Should().BeTrue();
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task HashSetAsync_ShouldSerializeAndCallHashSetAsync()
    {
        // Arrange
        const string v_MasterKey = "master";
        const string v_FieldKey = "field";
        TestObject v_Data = new TestObject(1, "HashVal");
        string v_Json = JsonSerializer.Serialize(v_Data);

        m_DatabaseMock
            .Setup(p_Db => p_Db.HashSetAsync(v_MasterKey, v_FieldKey, v_Json, When.Always, CommandFlags.None))
            .ReturnsAsync(true);
        m_DatabaseMock
            .Setup(p_Db => p_Db.KeyExpireAsync(v_MasterKey, TimeSpan.FromHours(24), ExpireWhen.Always, CommandFlags.None))
            .ReturnsAsync(true);

        // Act
        await m_CacheService.HashSetAsync(v_MasterKey, v_FieldKey, v_Data, TimeSpan.Zero, CancellationToken.None);

        // Assert
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task HashGetAllAsync_ShouldReturnDictionaryAsync()
    {
        // Arrange
        const string v_MasterKey = "master";
        TestObject v_Obj1 = new TestObject(1, "A");
        TestObject v_Obj2 = new TestObject(2, "B");

        HashEntry[] v_RedisEntries =
        [
            new HashEntry("key1", JsonSerializer.Serialize(v_Obj1)),
            new HashEntry("key2", JsonSerializer.Serialize(v_Obj2))
        ];

        m_DatabaseMock
            .Setup(p_Db => p_Db.HashGetAllAsync(v_MasterKey, CommandFlags.None))
            .ReturnsAsync(v_RedisEntries);

        // Act
        Dictionary<string, TestObject> v_Result = await m_CacheService.HashGetAllAsync<TestObject>(v_MasterKey, CancellationToken.None);

        // Assert
        v_Result.Should().HaveCount(2);
        v_Result["key1"].Should().BeEquivalentTo(v_Obj1);
        v_Result["key2"].Should().BeEquivalentTo(v_Obj2);
    }


    [Fact]
    public async Task ListRightPushAsync_ShouldSerializeAndPushAsync()
    {
        // Arrange
        const string v_Key = "list:key";
        TestObject v_Data = new TestObject(99, "Push");
        string v_Json = JsonSerializer.Serialize(v_Data);

        m_DatabaseMock
            .Setup(p_Db => p_Db.ListRightPushAsync(v_Key, v_Json, When.Always, CommandFlags.None))
            .ReturnsAsync(1);
        m_DatabaseMock
            .Setup(p_Db => p_Db.KeyExpireAsync(v_Key, TimeSpan.FromHours(24), ExpireWhen.Always, CommandFlags.None))
            .ReturnsAsync(true);

        // Act
        long v_Result = await m_CacheService.ListRightPushAsync(v_Key, v_Data, default, CancellationToken.None);

        // Assert
        v_Result.Should().Be(1);
        m_MockRepository.VerifyAll();
    }

    [Fact]
    public async Task SetAsync_ShouldDoNothing_IfCancellationRequestedAsync()
    {
        // Arrange
        CancellationTokenSource v_CancellationTokenSource = new CancellationTokenSource();
        await v_CancellationTokenSource.CancelAsync();


        // Act
        await m_CacheService.SetAsync("any", new TestObject(1, "A"), TimeSpan.Zero, v_CancellationTokenSource.Token);

        // Assert
        m_MockRepository.VerifyAll();
    }
}