using MediatR;
using Tuuuur.Core.Responses.Authentication;

namespace Tuuuur.Core.Requests.Authentication;

public record RefreshTokenRequest(string RefreshToken) : IRequest<JwtAuthenticationResponse>;
