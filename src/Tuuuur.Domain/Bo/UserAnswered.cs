namespace Tuuuur.Domain.Bo;

public record UserAnswered
{
    public bool RightAnswer { get; set; }
    public User User { get; set; }
}
