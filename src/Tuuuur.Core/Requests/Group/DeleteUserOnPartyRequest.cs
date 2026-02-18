using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Group;

public record DeleteUserOnPartyRequest(int UserId) : IRequest<EmptyResponse>;
