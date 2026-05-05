using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;

namespace Tuuuur.Domain.Tests.Bo;

public class RankedPartyTests
{
    [Fact]
    public void RankedParty_DefaultValues_ShouldInitializeCollections()
    {
        // Arrange
        RankedParty v_Party = new();

        // Assert
        v_Party.PartyUsers.Should().BeEmpty();
        v_Party.UserScores.Should().BeEmpty();
        v_Party.IsWinner.Should().BeFalse();
        v_Party.Elo.Should().Be(0);
        v_Party.FinalScore.Should().Be(0);
    }

    [Fact]
    public void RankedParty_ShouldStoreProperties()
    {
        // Arrange
        Guid v_PartyId = Guid.NewGuid();
        Guid v_UserId = Guid.NewGuid();

        RankedParty v_Party = new()
        {
            Id = v_PartyId,
            Dt = new DateTime(2024, 1, 1),
            IdPartyType = (int)PartyTypeType.Ranked,
            IdUserHost = v_UserId,
            Active = true,
            Finish = false,
            NbQuestions = 3,
            PartyUsers = [new PartyUser { IdUser = v_UserId, IdParty = v_PartyId }],
            UserScores = [new UserScore { Score = 12, User = new User { Id = v_UserId } }],
            IsWinner = true,
            Elo = 1240,
            FinalScore = 42
        };

        // Assert
        v_Party.Id.Should().Be(v_PartyId);
        v_Party.Dt.Should().Be(new DateTime(2024, 1, 1));
        v_Party.IdPartyType.Should().Be((int)PartyTypeType.Ranked);
        v_Party.IdUserHost.Should().Be(v_UserId);
        v_Party.Active.Should().BeTrue();
        v_Party.Finish.Should().BeFalse();
        v_Party.NbQuestions.Should().Be(3);
        v_Party.PartyUsers.Should().ContainSingle();
        v_Party.UserScores.Should().ContainSingle();
        v_Party.IsWinner.Should().BeTrue();
        v_Party.Elo.Should().Be(1240);
        v_Party.FinalScore.Should().Be(42);
    }
}