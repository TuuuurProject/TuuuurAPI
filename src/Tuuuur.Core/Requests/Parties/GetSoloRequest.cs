using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Parties;

public record GetSoloRequest(Guid PartyId): IRequest<GenericEntityResponse<Party>>;