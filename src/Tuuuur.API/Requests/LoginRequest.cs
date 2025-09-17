using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests
{
    /// <summary>
    /// Request for login action
    /// </summary>
    public record LoginRequest
    {
        /// <summary>
        /// Login (Email/Nickname) of the user
        /// </summary>
        public string Login { get; set; }
        /// <summary>
        /// Password of the user
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// Validator for login request
    /// </summary>
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        /// <summary>
        /// ctor containing validation rules
        /// </summary>
        public LoginRequestValidator()
        {
            RuleFor(m => m.Login)
                .EmailAddress().WithErrorCode(DomainErrors.Authentication.Login.InvalidEmail)
                .When(m => !string.IsNullOrEmpty(m.Login) && m.Login.Contains('@'));
            RuleFor(m => m.Login)
                .NotEmpty().WithErrorCode(DomainErrors.Authentication.Login.Empty)
                .When(m => string.IsNullOrEmpty(m.Login) || !m.Login.Contains('@'));
            
            RuleFor(m => m.Password)
                .NotEmpty().WithErrorCode(DomainErrors.Authentication.Password.Empty)
                .MinimumLength(8).WithErrorCode(DomainErrors.Authentication.Password.InvalidLength)
                .Matches("[A-Z]+").WithErrorCode(DomainErrors.Authentication.Password.InvalidUppercase)
                .Matches("[a-z]+").WithErrorCode(DomainErrors.Authentication.Password.InvalidLowercase)
                .Matches("[0-9]+").WithErrorCode(DomainErrors.Authentication.Password.InvalidNumber);
        }
    }
}