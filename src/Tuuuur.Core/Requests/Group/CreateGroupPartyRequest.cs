using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Group;

public record CreateGroupPartyRequest() : IRequest<GenericEntityResponse<Party>>;
