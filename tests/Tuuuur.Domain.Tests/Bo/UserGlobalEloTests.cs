using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Tests.Bo;

public class UserGlobalEloTests
{
    [Fact]
    public void GlobalElo_WhenEloListIsEmpty_ShouldReturnZero()
    {
        // Arrange
        User v_User = new() { Id = Guid.NewGuid(), Elo = [] };

        // Act
        int v_GlobalElo = v_User.GlobalElo;

        // Assert
        v_GlobalElo.Should().Be(0);
    }

    [Fact]
    public void GlobalElo_WhenEloListHasOneEntry_ShouldReturnThatValue()
    {
        // Arrange
        User v_User = new()
        {
            Id = Guid.NewGuid(),
            Elo = [new Elo { IdTheme = 1, Value = 1200 }]
        };

        // Act
        int v_GlobalElo = v_User.GlobalElo;

        // Assert
        v_GlobalElo.Should().Be(1200);
    }

    [Fact]
    public void GlobalElo_WhenEloListHasMultipleEntries_ShouldReturnAverage()
    {
        // Arrange
        User v_User = new()
        {
            Id = Guid.NewGuid(),
            Elo =
            [
                new Elo { IdTheme = 1, Value = 1000 },
                new Elo { IdTheme = 2, Value = 2000 }
            ]
        };

        // Act
        int v_GlobalElo = v_User.GlobalElo;

        // Assert
        v_GlobalElo.Should().Be(1500);
    }

    [Fact]
    public void GlobalElo_WhenAllEloAreIdentical_ShouldReturnThatValue()
    {
        // Arrange
        User v_User = new()
        {
            Id = Guid.NewGuid(),
            Elo =
            [
                new Elo { IdTheme = 1, Value = 1000 },
                new Elo { IdTheme = 2, Value = 1000 },
                new Elo { IdTheme = 3, Value = 1000 }
            ]
        };

        // Act
        int v_GlobalElo = v_User.GlobalElo;

        // Assert
        v_GlobalElo.Should().Be(1000);
    }
}
