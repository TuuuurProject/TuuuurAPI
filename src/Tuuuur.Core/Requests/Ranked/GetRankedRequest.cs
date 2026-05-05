using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Ranked;

public record GetRankedRequest(Guid PartyId): IRequest<GenericEntityResponse<RankedParty>>;