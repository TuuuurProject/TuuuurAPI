using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests;

public record UpdatePartyStateRequest(Guid PartyId, int AnwserId): IRequest<GenericEntityResponse<Party>>;