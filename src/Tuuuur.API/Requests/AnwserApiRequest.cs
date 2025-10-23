using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests;

/// <summary>
/// Request for Anwser
/// </summary>
public record AnwserApiRequest
{
    /// <summary>
    /// Anwser Id
    /// </summary>
    public int AnwserId { get; set; }
}

/// <summary>
/// Validator for anwser request
/// </summary>
public class AnwserApiRequestValidator : AbstractValidator<AnwserApiRequest>
{
    /// <summary>
    /// ctor containing validation rules
    /// </summary>
    public AnwserApiRequestValidator()
    {
        RuleFor(p_Request => p_Request.AnwserId)
            .NotEmpty().NotNull().WithErrorCode(DomainErrors.Party.Anwser.Empty);
    }
}