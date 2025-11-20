using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests;

/// <summary>
/// Request containing a GUID Id
/// </summary>
public record GuidRequest
{
    /// <summary>
    /// GUID identifier
    /// </summary>
    public Guid Id { get; init; }
}
