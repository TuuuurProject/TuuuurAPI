using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests.Users;

/// <summary>
/// Request for update user avatar action
/// </summary>
public record UserNicknameApiRequest
{
    /// <summary>
    /// Avatar in base64
    /// </summary>
    public string Nickname { get; set; } = string.Empty;
}

/// <summary>
/// Validator for avatar update request
/// </summary>
public class UserNicknameApiRequestValidator : AbstractValidator<UserNicknameApiRequest>
{
    /// <summary>
    /// ctor
    /// </summary>
    public UserNicknameApiRequestValidator()
    {
        RuleFor(p_Request => p_Request.Nickname)
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