using Tuuuur.Domain.Configuration;

namespace Tuuuur.Domain.Tests.Configuration;

public class RedisKeysTests
{
    [Fact]
    public void Party_ById_ShouldReturnCorrectKey()
    {
        // Arrange
        Guid v_PartyId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000");

        // Act
        string v_Result = RedisKeys.Party.ById(v_PartyId);

        // Assert
        v_Result.Should().Be("Party:123e4567-e89b-12d3-a456-426614174000");
    }

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
        Guid v_PartyId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000");

        // Act
        string v_Result = RedisKeys.Party.Users(v_PartyId);

        // Assert
        v_Result.Should().Be("Party:123e4567-e89b-12d3-a456-426614174000:User");
    }

    [Fact]
    public void Party_Users_WithString_ShouldReturnCorrectKey()
    {
        // Arrange
        Guid v_PartyId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000");

        // Act
        string v_Result = RedisKeys.Party.Users(v_PartyId);

        // Assert
        v_Result.Should().Be("Party:123e4567-e89b-12d3-a456-426614174000:User");
    }

    [Fact]
    public void User_Party_ShouldReturnCorrectKey()
    {
        // Arrange
        int v_UserId = 42;

        // Act
        string v_Result = RedisKeys.User.UserParty(v_UserId);

        // Assert
        v_Result.Should().Be("User:42:Party");
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
        Guid v_PartyId = Guid.Empty;

        // Act
        string v_Result = RedisKeys.Party.ById(v_PartyId);

        // Assert
        v_Result.Should().Be("Party:00000000-0000-0000-0000-000000000000");
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
