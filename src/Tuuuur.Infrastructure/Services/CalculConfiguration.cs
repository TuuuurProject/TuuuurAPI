using Tuuuur.Domain.Configuration;

namespace Tuuuur.Infrastructure.Services;

public class CalculConfiguration() : IServiceConfiguration
{
    private const string Sectionname = "CalculConfiguration";

    public int MaxDurationInSeconds { get; init; }
    public int MaxScore { get; init; }

    public string GetSectionName() => Sectionname;
}
