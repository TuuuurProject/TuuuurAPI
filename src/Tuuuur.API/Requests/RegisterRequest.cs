using FluentValidation;

namespace Tuuuur.API.Requests;

/// <summary>
/// Request for register action
/// </summary>
public record RegisterRequest : LoginRequest
{
    /// <summary>
    /// Firstname of the user to register
    /// </summary>
    public string FirstName { get; set; }
    /// <summary>
    /// Lastname of the user to register
    /// </summary>
    public string LastName { get; set; }
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
        RuleFor(m => m)
            .SetInheritanceValidator(v => v.Add<RegisterRequest>(new LoginRequestValidator()));
        RuleFor(m => m.FirstName).NotEmpty();
        RuleFor(m => m.LastName).NotEmpty();
    }
}