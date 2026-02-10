using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests;

public record GetHistoryRequest(Guid PartyId): IRequest<GenericEntityResponse<PartyBase>>;