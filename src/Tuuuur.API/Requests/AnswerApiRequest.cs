using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests;

/// <summary>
/// Request for Answer
/// </summary>
public record AnswerApiRequest
{
    /// <summary>
    /// Answer Id
    /// </summary>
    public int? AnswerId { get; set; }
}