using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Users;

public record UpdateUserNicknameRequest(string Nickname): IRequest<GenericEntityResponse<User>>;