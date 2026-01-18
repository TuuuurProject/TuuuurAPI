using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Group;

public record GroupLogicRequest(string PartyCode) : IRequest<EmptyResponse>;
