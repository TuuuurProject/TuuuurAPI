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

/// <summary>
/// Validator for GuidRequest
/// </summary>
public class GuidRequestValidator : AbstractValidator<GuidRequest>
{
    public GuidRequestValidator()
    {
        RuleFor(r => r.Id)
            .NotEmpty()
            .WithErrorCode(DomainErrors.Party.Id.Empty);
    }
}
