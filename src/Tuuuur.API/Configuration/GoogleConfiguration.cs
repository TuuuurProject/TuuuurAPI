using Tuuuur.Domain.Configuration;

namespace Tuuuur.API.Configuration;

/// <summary>
/// 
/// </summary>
public class GoogleConfiguration : AServiceConfiguration
{
    /// <summary>
    /// Client Id
    /// </summary>
    public string ClientId { get; set; }
    /// <summary>
    /// Client Secret
    /// </summary>
    public string ClientSecret { get; set; }
    private const string SectionName = "Authentification:Google";

    /// <inheritdoc />
    public override string GetSectionName() => SectionName;
}