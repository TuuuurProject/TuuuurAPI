using MediatR;
using Tuuuur.Core.Responses.Authentication;

namespace Tuuuur.Core.Requests.Authentication.Google;

public record GoogleLoginRequest(string Email) : IRequest<JwtAuthenticationResponse>;