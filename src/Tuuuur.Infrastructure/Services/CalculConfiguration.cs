using Tuuuur.Domain.Configuration;

namespace Tuuuur.Infrastructure.Services;

public class CalculConfiguration() : AServiceConfiguration
{
    private const string SECTIONNAME = "CalculConfiguration";

    public int MaxDurationInSeconds { get; set; }
    public int MaxScore { get; set; }

    public override string GetSectionName() => SECTIONNAME;
}
