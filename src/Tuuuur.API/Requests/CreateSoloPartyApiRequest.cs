using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests;

/// <summary>
/// Request for creating solo party
/// </summary>
public record CreateSoloPartyApiRequest
{
    /// <summary>
    /// Selected themes
    /// </summary>
    public int[] Themes { get; init; }
    /// <summary>
    /// Selected difficulties
    /// </summary>
    public int[] Difficulties { get; init; }
    /// <summary>
    /// Number of questions
    /// </summary>
    public int NbQuestions { get; init; }
}
/// <summary>
/// Validator for createsoloparty request
/// </summary>
public class CreateSoloPartyRequestValidator : AbstractValidator<CreateSoloPartyApiRequest>
{
    /// <summary>
    /// ctor containing validation rules
    /// </summary>
    public CreateSoloPartyRequestValidator()
    {
        RuleFor(p_Request => p_Request.Themes).NotEmpty().WithErrorCode(DomainErrors.Authentication.Login.InvalidEmail);
        RuleFor(p_Request => p_Request.Difficulties).NotEmpty().WithErrorCode(DomainErrors.Authentication.Login.InvalidEmail);
        RuleFor(p_Request => p_Request.NbQuestions).GreaterThan(0).WithErrorCode(DomainErrors.Authentication.Login.InvalidEmail);
    }
}
