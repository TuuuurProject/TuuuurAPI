using Tuuuur.Domain.Configuration;

namespace Tuuuur.API.Configuration;

/// <summary>
/// 
/// </summary>
public class GoogleConfiguration : IServiceConfiguration
{
    /// <summary>
    /// Client Id
    /// </summary>
    public string ClientId { get; set; }
    
    private const string SectionName = "Authentification:Google";

    /// <inheritdoc />
    public string GetSectionName() => SectionName;
}