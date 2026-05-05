using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Users;

public record UpdateUserAvatarRequest(string Avatar): IRequest<GenericEntityResponse<User>>;