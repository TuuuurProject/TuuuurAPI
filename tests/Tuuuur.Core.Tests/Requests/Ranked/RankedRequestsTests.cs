using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Tests.Requests.Ranked;

public class RankedRequestsTests
{
    // ── AnswerQuestionRankedRequest ───────────────────────────────────────────

    [Fact]
    public void AnswerQuestionRankedRequest_ShouldStoreProperties()
    {
        // Arrange
        int v_AnswerId = 42;
        Guid v_UserId = Guid.NewGuid();

        // Act
        AnswerQuestionRankedRequest v_Request = new(v_AnswerId, v_UserId);

        // Assert
        v_Request.AnswerId.Should().Be(v_AnswerId);
        v_Request.UserId.Should().Be(v_UserId);
    }

    [Fact]
    public void AnswerQuestionRankedRequest_EqualRecords_ShouldBeEqual()
    {
        Guid v_UserId = Guid.NewGuid();
        AnswerQuestionRankedRequest v_R1 = new(1, v_UserId);
        AnswerQuestionRankedRequest v_R2 = new(1, v_UserId);

        v_R1.Should().Be(v_R2);
    }

    // ── CreateRankedPartyRequest ──────────────────────────────────────────────

    [Fact]
    public void CreateRankedPartyRequest_ShouldStoreProperties()
    {
        // Arrange
        User v_Player1 = new() { Id = Guid.NewGuid(), NickName = "P1" };
        User v_Player2 = new() { Id = Guid.NewGuid(), NickName = "P2" };

        // Act
        CreateRankedPartyRequest v_Request = new(v_Player1, v_Player2);

        // Assert
        v_Request.Player1.Should().Be(v_Player1);
        v_Request.Player2.Should().Be(v_Player2);
    }

    [Fact]
    public void CreateRankedPartyRequest_EqualRecords_ShouldBeEqual()
    {
        User v_P1 = new() { Id = Guid.NewGuid() };
        User v_P2 = new() { Id = Guid.NewGuid() };
        CreateRankedPartyRequest v_R1 = new(v_P1, v_P2);
        CreateRankedPartyRequest v_R2 = new(v_P1, v_P2);

        v_R1.Should().Be(v_R2);
    }

    // ── JoinSearchOpponentRequest ─────────────────────────────────────────────

    [Fact]
    public void JoinSearchOpponentRequest_ShouldStoreUserId()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();

        // Act
        JoinSearchOpponentRequest v_Request = new(v_UserId);

        // Assert
        v_Request.UserId.Should().Be(v_UserId);
    }

    [Fact]
    public void JoinSearchOpponentRequest_EqualRecords_ShouldBeEqual()
    {
        Guid v_UserId = Guid.NewGuid();
        JoinSearchOpponentRequest v_R1 = new(v_UserId);
        JoinSearchOpponentRequest v_R2 = new(v_UserId);

        v_R1.Should().Be(v_R2);
    }

    // ── LeaveSeachOpponentRequest ─────────────────────────────────────────────

    [Fact]
    public void LeaveSeachOpponentRequest_ShouldStoreUserId()
    {
        // Arrange
        Guid v_UserId = Guid.NewGuid();

        // Act
        LeaveSeachOpponentRequest v_Request = new(v_UserId);

        // Assert
        v_Request.UserId.Should().Be(v_UserId);
    }

    [Fact]
    public void LeaveSeachOpponentRequest_EqualRecords_ShouldBeEqual()
    {
        Guid v_UserId = Guid.NewGuid();
        LeaveSeachOpponentRequest v_R1 = new(v_UserId);
        LeaveSeachOpponentRequest v_R2 = new(v_UserId);

        v_R1.Should().Be(v_R2);
    }

    // ── RankedLogicRequest ────────────────────────────────────────────────────

    [Fact]
    public void RankedLogicRequest_ShouldStorePartyId()
    {
        // Arrange
        Guid v_PartyId = Guid.NewGuid();

        // Act
        RankedLogicRequest v_Request = new(v_PartyId);

        // Assert
        v_Request.PartyId.Should().Be(v_PartyId);
    }

    [Fact]
    public void RankedLogicRequest_EqualRecords_ShouldBeEqual()
    {
        Guid v_PartyId = Guid.NewGuid();
        RankedLogicRequest v_R1 = new(v_PartyId);
        RankedLogicRequest v_R2 = new(v_PartyId);

        v_R1.Should().Be(v_R2);
    }

    // ── GetRankedRequest ─────────────────────────────────────────────────────

    [Fact]
    public void GetRankedRequest_ShouldStorePartyId()
    {
        // Arrange
        Guid v_PartyId = Guid.NewGuid();

        // Act
        GetRankedRequest v_Request = new(v_PartyId);

        // Assert
        v_Request.PartyId.Should().Be(v_PartyId);
    }

    [Fact]
    public void GetRankedRequest_EqualRecords_ShouldBeEqual()
    {
        Guid v_PartyId = Guid.NewGuid();
        GetRankedRequest v_R1 = new(v_PartyId);
        GetRankedRequest v_R2 = new(v_PartyId);

        v_R1.Should().Be(v_R2);
    }
}
