using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests;

/// <summary>
/// Request for Code
/// </summary>
public record CodeRequest
{
    /// <summary>
    /// Code
    /// </summary>
    public string Code { get; set; }
}
/// <summary>
/// Validator for code request
/// </summary>
public class CodeRequestValidator : AbstractValidator<CodeRequest>
{
    /// <summary>
    /// ctor containing validation rules
    /// </summary>
    public CodeRequestValidator()
    {
        RuleFor(p_Request => p_Request.Code)
            .NotEmpty()
            .WithErrorCode(DomainErrors.Party.Code.Empty)
            .Matches(@"^\d{6}$")
            .WithErrorCode(DomainErrors.Party.Code.Invalid);
    }
}