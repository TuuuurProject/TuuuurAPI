using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests;

/// <summary>
/// Request for account validation action
/// </summary>
public record ValidateAccountRequest
{
    /// <summary>
    /// Email (login) of the user
    /// </summary>
    public string Email { get; set; }
    /// <summary>
    /// Code of verification
    /// </summary>
    public string Code { get; set; }
}

/// <summary>
/// Validator for account validation action
/// </summary>
public class ValidateAccountValidator : AbstractValidator<ValidateAccountRequest>
{
    /// <summary>
    /// ctor containing validation rules
    /// </summary>
    public ValidateAccountValidator()
    {
        RuleFor(m => m.Email)
            .NotEmpty().WithErrorCode(DomainErrors.Authentication.Login.Empty)
            .EmailAddress().WithErrorCode(DomainErrors.Authentication.Login.InvalidEmail);
        RuleFor(m => m.Code)
            .NotEmpty().WithErrorCode(DomainErrors.Authentication.Code.Empty)
            .Length(6).WithErrorCode(DomainErrors.Authentication.Code.InvalidLength);
    }
}