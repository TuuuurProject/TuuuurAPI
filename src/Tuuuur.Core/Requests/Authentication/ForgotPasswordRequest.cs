using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Authentication;

/// <summary>
/// Request for forgot password
/// </summary>
/// <param name="Login"></param>
public record ForgotPasswordRequest(string Login) : IRequest<EmptyResponse>;