using System.Text.Json.Serialization;
using FluentValidation;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Requests;

/// <summary>
/// Request for paginated results
/// </summary>
public record PaginationRequest
{
    /// <summary>
    /// Page number (1 = first page)
    /// </summary>
    [JsonRequired]
    public int Page { get; init; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    [JsonRequired]
    public int Size { get; init; }
}

/// <summary>
/// Validator for pagination request
/// </summary>
public class PaginationRequestValidator : AbstractValidator<PaginationRequest>
{
    public PaginationRequestValidator()
    {
        RuleFor(p_Request => p_Request.Page)
            .GreaterThan(0)
            .WithErrorCode(DomainErrors.Pagination.Invalid);

        RuleFor(p_Request => p_Request.Size)
            .GreaterThan(0)
            .WithErrorCode(DomainErrors.Pagination.Invalid);
    }
}