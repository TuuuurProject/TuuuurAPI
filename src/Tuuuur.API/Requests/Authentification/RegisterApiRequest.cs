using FluentValidation;
using Tuuuur.API.Requests.Users;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests.Authentification
{
    /// <summary>
    ///     Request for register action
    /// </summary>
    public record RegisterApiRequest : EmailApiRequest
    {
        /// <summary>
        ///     Nickname of the user to register
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        ///     Password of the user
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    ///     Validator for registration request
    /// </summary>
    public class RegisterRequestValidator : AbstractValidator<RegisterApiRequest>
    {
        /// <summary>
        ///     ctor containing validation rules
        /// </summary>
        public RegisterRequestValidator()
        {
            RuleFor(p_RegisterRequest => p_RegisterRequest)
                .SetInheritanceValidator(p_PolymorphicValidator =>
                    p_PolymorphicValidator.Add<RegisterApiRequest>(new EmailRequestValidator()));

            RuleFor(p_Request => p_Request.Password)
                .NotEmpty().WithErrorCode(DomainErrors.Authentication.Password.Empty)
                .MinimumLength(8).WithErrorCode(DomainErrors.Authentication.Password.InvalidLength)
                .Matches("[A-Z]+").WithErrorCode(DomainErrors.Authentication.Password.InvalidUppercase)
                .Matches("[a-z]+").WithErrorCode(DomainErrors.Authentication.Password.InvalidLowercase)
                .Matches("[0-9]+").WithErrorCode(DomainErrors.Authentication.Password.InvalidNumber);
            
            RuleFor(p_Request => p_Request.NickName)
                .NotEmpty()
                .WithErrorCode(DomainErrors.User.Nickname.Empty)
                .WithMessage("Nickname cannot be empty.")
            
                .MaximumLength(50)
                .WithErrorCode(DomainErrors.User.Nickname.TooLong)
                .WithMessage("Nickname must not exceed 50 characters.")

                .Matches("^[a-zA-Z0-9\\- ]+$")
                .WithErrorCode(DomainErrors.User.Nickname.Invalid)
                .WithMessage("Nickname must contain only letters, numbers, spaces, and hyphens.");
        }
    }
}