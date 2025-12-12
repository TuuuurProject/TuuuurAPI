using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests;

/// <summary>
/// Request for Code
/// </summary>
public record CodeRequest
{
    /// <summary>
    /// Code
    /// </summary>
    public string Code { get; set; }
}