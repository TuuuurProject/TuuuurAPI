using MediatR;
using Tuuuur.Core.Responses.Authentication;

namespace Tuuuur.Core.Requests.Authentication.Google;

public record GoogleAuthentificationRequest(string Email) : IRequest<JwtAuthenticationResponse>;