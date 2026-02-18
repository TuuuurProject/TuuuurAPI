namespace Tuuuur.Domain.Bo;

public record UserAnswered
{
    public bool Correct { get; set; }
    public User User { get; set; }
}
