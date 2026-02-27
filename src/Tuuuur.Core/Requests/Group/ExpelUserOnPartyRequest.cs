using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Group;

public record ExpelUserOnPartyRequest(Guid UserId) : IRequest<EmptyResponse>;
