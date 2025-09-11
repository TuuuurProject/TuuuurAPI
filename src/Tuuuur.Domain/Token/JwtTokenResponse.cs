namespace Tuuuur.Domain.Token;

public record JwtTokenResponse
{
    public string Token { get; init; }
    public DateTime ValidTo { get; init; }
    public DateTime ValidFrom { get; init; }
}