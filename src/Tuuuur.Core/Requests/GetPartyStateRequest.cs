using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests;

public record GetPartyStateRequest(Guid PartyId): IRequest<GenericEntityResponse<Party>>;