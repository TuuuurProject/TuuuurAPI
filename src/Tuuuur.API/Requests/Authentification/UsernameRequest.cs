using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests.Authentification;

/// <summary>
///     Request for register action
/// </summary>
public record NickNameRequest
{
    /// <summary>
    ///     Nickname of the user to register
    /// </summary>
    public string NickName { get; set; }
}

/// <summary>
///     Validator for registration request
/// </summary>
public class NickNameRequestValidator : AbstractValidator<NickNameRequest>
{
    /// <summary>
    ///     ctor containing validation rules
    /// </summary>
    public NickNameRequestValidator()
    {
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
