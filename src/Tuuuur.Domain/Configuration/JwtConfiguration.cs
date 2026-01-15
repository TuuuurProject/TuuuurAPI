namespace Tuuuur.Domain.Configuration;

public record JwtConfiguration
{
    public string Issuer { get; init; }
    public string Audience { get; init; }
    public string Key { get; init; }
    public int Validity { get; init; }
    public int RefreshTokenValidity { get; init; } = 30;

    public const string SectionName = "JwtSettings";
}