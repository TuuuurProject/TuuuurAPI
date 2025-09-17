using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests;

/// <summary>
/// Request for register action
/// </summary>
public record RegisterRequest : LoginRequest
{
    /// <summary>
    /// Nickname of the user to register
    /// </summary>
    public string NickName { get; set; }
}

/// <summary>
/// Validator for registration request
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    /// <summary>
    /// ctor containing validation rules
    /// </summary>
    public RegisterRequestValidator()
    {
        RuleFor(p_RegisterRequest => p_RegisterRequest)
            .SetInheritanceValidator(p_PolymorphicValidator => p_PolymorphicValidator.Add<RegisterRequest>(new LoginRequestValidator()));
        RuleFor(p => p.NickName)
            .NotEmpty()
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage(DomainErrors.Authentication.NickName.Invalid_NickName);
    }
}