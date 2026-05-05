using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Ranked;

public record JoinSearchOpponentRequest(Guid UserId): IRequest<EmptyResponse>;
