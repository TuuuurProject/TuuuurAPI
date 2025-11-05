using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Parties;

public record GetPartyStateRequest(Guid PartyId): IRequest<GenericEntityResponse<Party>>;