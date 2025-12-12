using Tuuuur.Domain.Configuration;

namespace Tuuuur.Core.Configuration;

public class WebsiteConfiguration : IServiceConfiguration
{
    private const string Sectionname = "Website";
    public string BaseUri { get; set; } = string.Empty;

    public string GetSectionName() => Sectionname;
}