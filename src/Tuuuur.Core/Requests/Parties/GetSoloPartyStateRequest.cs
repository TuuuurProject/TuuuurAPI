using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Parties;

public record GetSoloPartyStateRequest(Guid PartyId): IRequest<GenericEntityResponse<Party>>;