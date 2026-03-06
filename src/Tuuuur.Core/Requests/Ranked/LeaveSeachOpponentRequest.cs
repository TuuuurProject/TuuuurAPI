using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Ranked;

public record LeaveSeachOpponentRequest(Guid UserId): IRequest<EmptyResponse>;
