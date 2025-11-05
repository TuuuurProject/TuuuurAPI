using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Users;

public record DeleteUserRequest(): IRequest<EmptyResponse>;