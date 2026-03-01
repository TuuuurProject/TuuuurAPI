using Tuuuur.Domain.Configuration;

namespace Tuuuur.Domain.Tests.Configuration;

public class RedisKeysTests
{
    [Fact]
    public void Party_ByCode_ShouldReturnCorrectKey()
    {
        // Arrange
        string v_Code = "ABC123";

        // Act
        string v_Result = RedisKeys.Group.ByCode(v_Code);

        // Assert
        v_Result.Should().Be("Group:ABC123");
    }

    [Fact]
    public void Party_Users_WithGuid_ShouldReturnCorrectKey()
    {
        // Arrange
        const string v_PartyCode = "123456";

        // Act
        string v_Result = RedisKeys.Group.Users(v_PartyCode);

        // Assert
        v_Result.Should().Be($"Group:{v_PartyCode}:User");
    }

    [Fact]
    public void Party_Users_WithString_ShouldReturnCorrectKey()
    {
        // Arrange
        const string v_PartyCode = "123456";

        // Act
        string v_Result = RedisKeys.Group.Users(v_PartyCode);

        // Assert
        v_Result.Should().Be($"Group:{v_PartyCode}:User");
    }

    [Fact]
    public void User_Party_ShouldReturnCorrectKey()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();

        // Act
        string v_Result = RedisKeys.User.UserGroup(v_UserId);

        // Assert
        v_Result.Should().Be($"User:{v_UserId}:Group");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Party_ByCode_WithInvalidCode_ShouldHandleGracefully(string p_Code)
    {
        // Act
        string v_Result = RedisKeys.Group.ByCode(p_Code);

        // Assert
        v_Result.Should().Be($"Group:{p_Code}");
    }

    [Fact]
    public void Party_ById_WithEmptyGuid_ShouldReturnKeyWithEmptyGuid()
    {
        // Arrange
        string v_PartyCode = string.Empty;
        // Act
        string v_Result = RedisKeys.Group.ByCode(v_PartyCode);

        // Assert
        v_Result.Should().Be($"Group:{v_PartyCode}");
    }

    [Fact]
    public void User_Party_WithZeroUserId_ShouldReturnCorrectKey()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();

        // Act
        string v_Result = RedisKeys.User.UserGroup(v_UserId);

        // Assert
        v_Result.Should().Be($"User:{v_UserId}:Group");
    }

    [Fact]
    public void User_Party_WithNegativeUserId_ShouldReturnCorrectKey()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();

        // Act
        string v_Result = RedisKeys.User.UserGroup(v_UserId);

        // Assert
        v_Result.Should().Be($"User:{v_UserId}:Group");
    }
}
