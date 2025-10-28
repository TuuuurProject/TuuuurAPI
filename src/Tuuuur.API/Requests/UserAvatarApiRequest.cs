using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests;

/// <summary>
/// Request for update user avatar action
/// </summary>
public record UserAvatarApiRequest
{
    /// <summary>
    /// Avatar in base64
    /// </summary>
    public string Avatar { get; set; } = string.Empty;
}

/// <summary>
/// Validator for avatar update request
/// </summary>
public class UserAvatarApiRequestValidator : AbstractValidator<UserAvatarApiRequest>
{
    public UserAvatarApiRequestValidator()
    {
        RuleFor(p_Request => p_Request.Avatar)
            .NotEmpty()
            .WithErrorCode(DomainErrors.User.Avatar.Empty)
            .WithMessage("Avatar cannot be empty.");

        RuleFor(p_Request => p_Request.Avatar)
            .Must(BeValidBase64)
            .WithErrorCode(DomainErrors.User.Avatar.InvalidFormat)
            .WithMessage("Avatar must be a valid Base64 string.");
    }

    private static bool BeValidBase64(string p_Base64)
    {
        if (string.IsNullOrWhiteSpace(p_Base64))
            return false;

        try
        {
            string[] v_Parts = p_Base64.Split(',');
            string v_Raw = v_Parts.Length > 1 ? v_Parts[1] : v_Parts[0];

            _ = Convert.FromBase64String(v_Raw);
            return true;
        }
        catch
        {
            return false;
        }
    }
}