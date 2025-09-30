using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests;

/// <summary>
/// Request for account validation action
/// </summary>
public record ValidateAccountApiRequest
{
    /// <summary>
    /// Login (Email or Nickname) of the user
    /// </summary>
    public string Login { get; set; }
    /// <summary>
    /// Code of verification
    /// </summary>
    public string Code { get; set; }
}

/// <summary>
/// Validator for account validation action
/// </summary>
public class ValidateAccountValidator : AbstractValidator<ValidateAccountApiRequest>
{
    /// <summary>
    /// ctor containing validation rules
    /// </summary>
    public ValidateAccountValidator()
    {
        RuleFor(m => m.Login)
            .NotEmpty().WithErrorCode(DomainErrors.Authentication.Login.Empty);
        RuleFor(m => m.Code)
            .NotEmpty().WithErrorCode(DomainErrors.Authentication.Code.Empty)
            .Length(6).WithErrorCode(DomainErrors.Authentication.Code.InvalidLength);
    }
}