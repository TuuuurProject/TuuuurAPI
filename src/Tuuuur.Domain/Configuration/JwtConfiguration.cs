namespace Tuuuur.Domain.Configuration;

public record JwtConfiguration
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
    public int Validity { get; set; }

    public const string SectionName = "JwtSettings";
}