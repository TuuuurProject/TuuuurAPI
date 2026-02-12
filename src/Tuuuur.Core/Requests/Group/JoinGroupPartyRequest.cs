using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Group;

public record JoinGroupPartyRequest(string Code) : IRequest<GenericEntityResponse<GroupParty>>;
