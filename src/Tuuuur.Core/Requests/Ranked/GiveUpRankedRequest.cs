using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Ranked;

/// <summary>
/// Request to give up ranked party
/// </summary>
public record GiveUpRankedRequest(Guid UserId) : IRequest<EmptyResponse>;