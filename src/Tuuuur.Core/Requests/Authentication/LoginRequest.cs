using Tuuuur.Core.Responses.Authentication;
using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Authentication;
public record LoginRequest(string Login, string Password) : IRequest<EmptyResponse>;