using Tuuuur.Core.Responses.Authentication;
using MediatR;

namespace Tuuuur.Core.Requests.Authentication;
public record JwtAuthenticationRequest(string Login, string Password) : IRequest<JwtAuthenticationResponse>;