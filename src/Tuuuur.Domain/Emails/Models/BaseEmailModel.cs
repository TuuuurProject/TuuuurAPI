using System.Diagnostics.CodeAnalysis;

namespace Tuuuur.Domain.Emails.Models;

/// <summary>
/// Class of Email Model
/// </summary>
[ExcludeFromCodeCoverage]
public abstract class BaseEmailModel
{
    /// <summary>
    /// SiteLink for Email Model
    /// </summary>
    public string SiteLink { get; set; } = string.Empty;
}