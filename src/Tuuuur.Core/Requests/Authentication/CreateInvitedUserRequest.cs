using MediatR;
using Tuuuur.Core.Responses.Authentication;

namespace Tuuuur.Core.Requests.Authentication;

public record CreateInvitedUserRequest(string NickName) : IRequest<JwtAuthenticationResponse>;
