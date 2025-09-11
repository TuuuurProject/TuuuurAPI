using Tuuuur.Domain.Configuration;

namespace Tuuuur.Core.Configuration;

public class WebsiteConfiguration : AServiceConfiguration
{
    public const string SECTIONNAME = "Website";
    public string BaseUri { get; set; } = string.Empty;

    public override string GetSectionName() => SECTIONNAME;
}