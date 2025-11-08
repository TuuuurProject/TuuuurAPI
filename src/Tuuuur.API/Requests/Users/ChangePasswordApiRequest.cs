using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests.Users;

/// <summary>
/// Request for change password
/// </summary>
public record ChangePasswordApiRequest
{
    /// <summary>
    /// Old password
    /// </summary>
    public string CurrentPassword { get; set; }
    /// <summary>
    /// New Password
    /// </summary>
    public string NewPassword { get; set; }
}

/// <summary>
/// Validator for changing password request
/// </summary>
public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordApiRequest>
{
    /// <summary>
    /// ctor containing validation rules
    /// </summary>
    public ChangePasswordRequestValidator()
    {
        RuleFor(p_Request => p_Request.NewPassword)
            .NotEmpty().WithErrorCode(DomainErrors.Authentication.Password.Empty)
            .MinimumLength(8).WithErrorCode(DomainErrors.Authentication.Password.InvalidLength)
            .Matches("[A-Z]+").WithErrorCode(DomainErrors.Authentication.Password.InvalidUppercase)
            .Matches("[a-z]+").WithErrorCode(DomainErrors.Authentication.Password.InvalidLowercase)
            .Matches("[0-9]+").WithErrorCode(DomainErrors.Authentication.Password.InvalidNumber);
    }
}