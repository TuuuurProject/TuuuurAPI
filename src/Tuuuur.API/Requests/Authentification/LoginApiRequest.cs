using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests
{
    /// <summary>
    ///     Request for login action
    /// </summary>
    public record LoginApiRequest
    {
        /// <summary>
        ///     Login (Email/Nickname) of the user
        /// </summary>
        public string Login { get; set; }
    }

    /// <summary>
    ///     Validator for login request
    /// </summary>
    public class LoginRequestValidator : AbstractValidator<LoginApiRequest>
    {
        /// <summary>
        ///     ctor containing validation rules
        /// </summary>
        public LoginRequestValidator()
        {
            RuleFor(m => m.Login)
                .EmailAddress().WithErrorCode(DomainErrors.Authentication.Login.InvalidEmail)
                .When(m => !string.IsNullOrEmpty(m.Login) && m.Login.Contains('@'));
            RuleFor(m => m.Login)
                .NotEmpty().WithErrorCode(DomainErrors.Authentication.Login.Empty)
                .When(m => string.IsNullOrEmpty(m.Login) || !m.Login.Contains('@'));
        }
    }
}