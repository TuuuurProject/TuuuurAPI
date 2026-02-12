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
        string v_Result = RedisKeys.Party.ByCode(v_Code);

        // Assert
        v_Result.Should().Be("Party:ABC123");
    }

    [Fact]
    public void Party_Users_WithGuid_ShouldReturnCorrectKey()
    {
        // Arrange
        const string v_PartyCode = "123456";

        // Act
        string v_Result = RedisKeys.Party.Users(v_PartyCode);

        // Assert
        v_Result.Should().Be($"Party:{v_PartyCode}:User");
    }

    [Fact]
    public void Party_Users_WithString_ShouldReturnCorrectKey()
    {
        // Arrange
        const string v_PartyCode = "123456";

        // Act
        string v_Result = RedisKeys.Party.Users(v_PartyCode);

        // Assert
        v_Result.Should().Be($"Party:{v_PartyCode}:User");
    }

    [Fact]
    public void User_Party_ShouldReturnCorrectKey()
    {
        // Arrange
        const int v_UserId = 42;

        // Act
        string v_Result = RedisKeys.User.UserParty(v_UserId);

        // Assert
        v_Result.Should().Be($"User:{v_UserId}:Party");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Party_ByCode_WithInvalidCode_ShouldHandleGracefully(string p_Code)
    {
        // Act
        string v_Result = RedisKeys.Party.ByCode(p_Code);

        // Assert
        v_Result.Should().Be($"Party:{p_Code}");
    }

    [Fact]
    public void Party_ById_WithEmptyGuid_ShouldReturnKeyWithEmptyGuid()
    {
        // Arrange
        string v_PartyCode = string.Empty;
        // Act
        string v_Result = RedisKeys.Party.ByCode(v_PartyCode);

        // Assert
        v_Result.Should().Be($"Party:{v_PartyCode}");
    }

    [Fact]
    public void User_Party_WithZeroUserId_ShouldReturnCorrectKey()
    {
        // Arrange
        int v_UserId = 0;

        // Act
        string v_Result = RedisKeys.User.UserParty(v_UserId);

        // Assert
        v_Result.Should().Be("User:0:Party");
    }

    [Fact]
    public void User_Party_WithNegativeUserId_ShouldReturnCorrectKey()
    {
        // Arrange
        int v_UserId = -1;

        // Act
        string v_Result = RedisKeys.User.UserParty(v_UserId);

        // Assert
        v_Result.Should().Be("User:-1:Party");
    }
}
