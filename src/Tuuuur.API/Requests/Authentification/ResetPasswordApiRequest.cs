using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests.Authentification
{
    /// <summary>
    ///     Request for reset password
    /// </summary>
    public record ResetPasswordApiRequest
    {
        /// <summary>
        ///     Login (Email/Nickname) of the user
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        ///     Password of the user
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Code 2FA
        /// </summary>
        public string Code { get; set; }
    }

    /// <summary>
    ///     Validator for reset password request
    /// </summary>
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordApiRequest>
    {
        /// <summary>
        ///     ctor containing validation rules
        /// </summary>
        public ResetPasswordRequestValidator()
        {
            RuleFor(m => m.Login)
                .EmailAddress().WithErrorCode(DomainErrors.Authentication.Login.InvalidEmail)
                .When(m => !string.IsNullOrEmpty(m.Login) && m.Login.Contains('@'));
            RuleFor(m => m.Login)
                .NotEmpty().WithErrorCode(DomainErrors.Authentication.Login.Empty)
                .When(m => string.IsNullOrEmpty(m.Login) || !m.Login.Contains('@'));

            RuleFor(m => m.Code)
                .NotEmpty().WithErrorCode(DomainErrors.Authentication.Code.Empty)
                .Length(6).WithErrorCode(DomainErrors.Authentication.Code.InvalidLength);

            RuleFor(m => m.Password)
                .NotEmpty().WithErrorCode(DomainErrors.Authentication.Password.Empty)
                .MinimumLength(8).WithErrorCode(DomainErrors.Authentication.Password.InvalidLength)
                .Matches("[A-Z]+").WithErrorCode(DomainErrors.Authentication.Password.InvalidUppercase)
                .Matches("[a-z]+").WithErrorCode(DomainErrors.Authentication.Password.InvalidLowercase)
                .Matches("[0-9]+").WithErrorCode(DomainErrors.Authentication.Password.InvalidNumber);
        }
    }
}