using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Authentication;

/// <summary>
/// Request for reset password
/// </summary>
/// <param name="Login"></param>
/// <param name="Password"></param>
/// <param name="Code"></param>
public record ResetPasswordRequest(string Login, string Password, string Code) : IRequest<EmptyResponse>;