using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Users;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword): IRequest<GenericEntityResponse<User>>;