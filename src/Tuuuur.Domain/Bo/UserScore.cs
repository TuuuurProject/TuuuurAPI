namespace Tuuuur.Domain.Bo;

public record UserScore
{
    public int Score { get; set; }
    public User User { get; set; }
}
