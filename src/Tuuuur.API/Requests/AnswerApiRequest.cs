using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests;

/// <summary>
/// Request for Answer
/// </summary>
public record AnswerApiRequest
{
    /// <summary>
    /// Answer Id
    /// </summary>
    public int AnswerId { get; set; }
}

/// <summary>
/// Validator for answer request
/// </summary>
public class AnswerApiRequestValidator : AbstractValidator<AnswerApiRequest>
{
    /// <summary>
    /// ctor containing validation rules
    /// </summary>
    public AnswerApiRequestValidator()
    {
        RuleFor(p_Request => p_Request.AnswerId)
            .NotEmpty().NotNull().WithErrorCode(DomainErrors.Party.Answer.Empty);
    }
}