using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Ranked;

public record RankedLogicRequest(Guid PartyId) : IRequest<EmptyResponse>;
