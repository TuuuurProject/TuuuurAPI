namespace Tuuuur.Domain.Bo;

public record Elo : IBOEntity
{
    public int IdTheme { get; init; }
    public Theme Theme { get; init; }
    public int Value { get; init; }
    public int GamesPlayed { get; set; }
}
