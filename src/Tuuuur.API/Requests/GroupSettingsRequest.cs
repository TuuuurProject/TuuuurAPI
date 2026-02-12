using System.Text.Json.Serialization;
using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests;

/// <summary>
/// Request for group settings
/// </summary>
public record GroupSettingsRequest
{
    /// <summary>
    /// Selected themes
    /// </summary>
    [JsonRequired]
    public int[] Themes { get; init; }
    /// <summary>
    /// Selected difficulties
    /// </summary>
    [JsonRequired]
    public int[] Difficulties { get; init; }
    /// <summary>
    /// Number of questions
    /// </summary>
    [JsonRequired]
    public int NbQuestions { get; init; }
    
    /// <summary>
    /// If the score is sent each time
    /// </summary>
    [JsonRequired]
    public bool ScoreEachRound { get; init; }
}
/// <summary>
/// Validator for request
/// </summary>
public class GroupSettingsRequestValidator : AbstractValidator<GroupSettingsRequest>
{
    /// <summary>
    /// ctor containing validation rules
    /// </summary>
    public GroupSettingsRequestValidator()
    {
        RuleFor(p_Request => p_Request.Themes).NotEmpty().WithErrorCode(DomainErrors.Theme.Invalid);
        RuleFor(p_Request => p_Request.Difficulties).NotEmpty().WithErrorCode(DomainErrors.Difficulty.Invalid);
        RuleFor(p_Request => p_Request.NbQuestions)
            .Must(p_Nb => p_Nb is 5 or 10 or 15 or 20)
            .WithErrorCode(DomainErrors.Party.NbQuestions.Invalid);
    }
}
