using MediatR;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Authentication;

public record VerifyAccountRequest(string Email, string Code) : IRequest<JwtAuthenticationResponse>;