using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests.Authentification
{
    /// <summary>
    ///     Request for Email
    /// </summary>
    public abstract record EmailApiRequest
    {
        /// <summary>
        ///     Email of the user
        /// </summary>
        public string Email { get; set; }
    }

    /// <summary>
    ///     Validator for email request
    /// </summary>
    public class EmailRequestValidator : AbstractValidator<EmailApiRequest>
    {
        /// <summary>
        ///     ctor containing validation rules
        /// </summary>
        public EmailRequestValidator()
        {
            RuleFor(p_Request => p_Request.Email)
                .EmailAddress().WithErrorCode(DomainErrors.Authentication.Login.InvalidEmail);
        }
    }
}