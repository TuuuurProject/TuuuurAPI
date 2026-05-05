using Tuuuur.Domain.Configuration;

namespace Tuuuur.Infrastructure.Emails;

[ExcludeFromCodeCoverage]
internal class SmtpEmailConfiguration : IServiceConfiguration
{
    private const string Sectionname = "SmtpEmailConfiguration";

    public string SmtpAddress { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpLogin { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool EnableEncryption { get; set; } = false;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;

    public string GetSectionName() => Sectionname;
}

