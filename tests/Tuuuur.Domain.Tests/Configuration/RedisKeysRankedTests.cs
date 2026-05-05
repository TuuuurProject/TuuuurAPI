using Tuuuur.Domain.Configuration;

namespace Tuuuur.Domain.Tests.Configuration;

public class RedisKeysRankedTests
{
    // ── Ranked Keys ──────────────────────────────────────────────────────────

    [Fact]
    public void Ranked_MatchmakingList_ShouldReturnCorrectKey()
    {
        string v_Result = RedisKeys.Ranked.MatchmakingList();
        v_Result.Should().Be("Ranked:Matchmaking");
    }

    [Fact]
    public void Ranked_MatchmakingLock_ShouldReturnCorrectKey()
    {
        string v_Result = RedisKeys.Ranked.MatchmakingLock();
        v_Result.Should().Be("Ranked:Matchmaking:Lock");
    }

    [Fact]
    public void Ranked_MatchmakingJoinedAt_ShouldReturnCorrectKey()
    {
        string v_Result = RedisKeys.Ranked.MatchmakingJoinedAt();
        v_Result.Should().Be("Ranked:Matchmaking:JoinedAt");
    }

    [Fact]
    public void Ranked_ById_ShouldReturnCorrectKey()
    {
        Guid v_Id = Guid.NewGuid();
        string v_Result = RedisKeys.Ranked.ById(v_Id);
        v_Result.Should().Be($"Ranked:{v_Id}");
    }

    [Fact]
    public void Ranked_CurrentQuestionIndex_ShouldReturnCorrectKey()
    {
        Guid v_Id = Guid.NewGuid();
        string v_Result = RedisKeys.Ranked.CurrentQuestionIndex(v_Id);
        v_Result.Should().Be($"Ranked:{v_Id}:CurrentQuestionIndex");
    }

    [Fact]
    public void Ranked_Scores_ShouldReturnCorrectKey()
    {
        Guid v_Id = Guid.NewGuid();
        string v_Result = RedisKeys.Ranked.Scores(v_Id);
        v_Result.Should().Be($"Ranked:{v_Id}:Scores");
    }

    [Fact]
    public void Ranked_Questions_ShouldReturnCorrectKey()
    {
        Guid v_Id = Guid.NewGuid();
        string v_Result = RedisKeys.Ranked.Questions(v_Id);
        v_Result.Should().Be($"Ranked:{v_Id}:Questions");
    }

    [Fact]
    public void Ranked_QuestionUserAnswer_ShouldReturnCorrectKey()
    {
        Guid v_PartyId = Guid.NewGuid();
        int v_QuestionId = 42;
        Guid v_UserId = Guid.NewGuid();
        string v_Result = RedisKeys.Ranked.QuestionUserAnswer(v_PartyId, v_QuestionId, v_UserId);
        v_Result.Should().Be($"Ranked:{v_PartyId}:Questions:{v_QuestionId}:Users:{v_UserId}:Answer");
    }

    [Fact]
    public void Ranked_PartyQuestionAnswered_ShouldReturnCorrectKey()
    {
        Guid v_PartyId = Guid.NewGuid();
        int v_QuestionId = 7;
        string v_Result = RedisKeys.Ranked.PartyQuestionAnswered(v_PartyId, v_QuestionId);
        v_Result.Should().Be($"Ranked:{v_PartyId}:Questions:{v_QuestionId}:Answered");
    }

    [Fact]
    public void Ranked_PartyQuestionAllAnsweredChannel_ShouldReturnCorrectKey()
    {
        Guid v_PartyId = Guid.NewGuid();
        int v_QuestionId = 3;
        string v_Result = RedisKeys.Ranked.PartyQuestionAllAnsweredChannel(v_PartyId, v_QuestionId);
        v_Result.Should().Be($"Ranked:{v_PartyId}:Questions:{v_QuestionId}:AllAnswered");
    }

    // ── User Ranked Key ──────────────────────────────────────────────────────

    [Fact]
    public void User_UserRanked_ShouldReturnCorrectKey()
    {
        Guid v_UserId = Guid.NewGuid();
        string v_Result = RedisKeys.User.UserRanked(v_UserId);
        v_Result.Should().Be($"User:{v_UserId}:Ranked");
    }
}
