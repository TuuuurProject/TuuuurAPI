using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Group;

public record StartGroupPartyRequest(string UserEmail) : IRequest<EmptyResponse>;
