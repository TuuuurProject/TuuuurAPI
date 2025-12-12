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
            .NotEmpty().NotNull()
            .WithErrorCode(DomainErrors.User.Nickname.Empty)
            .WithMessage("Nickname cannot be empty.");
    }
}