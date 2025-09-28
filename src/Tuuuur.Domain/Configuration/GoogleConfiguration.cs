namespace Tuuuur.Domain.Configuration;

public class GoogleConfiguration : AServiceConfiguration
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    private const string SectionName = "Authentification:Google";
    public override string GetSectionName() => SectionName;
}