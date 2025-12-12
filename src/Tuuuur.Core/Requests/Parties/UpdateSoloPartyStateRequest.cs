using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Parties;

public record UpdateSoloPartyStateRequest(Guid PartyId, int? AnswerId): IRequest<GenericEntityResponse<Party>>;