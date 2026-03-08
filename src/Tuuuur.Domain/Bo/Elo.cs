namespace Tuuuur.Domain.Bo;

public record Elo : IBOEntity
{
    public int IdTheme { get; init; }
    public Theme Theme { get; init; }
    public int Value { get; init; }

    /// <summary>Total ranked games played for this user/theme pair. Drives the placement K-factor.</summary>
    public int GamesPlayed { get; init; }
}
