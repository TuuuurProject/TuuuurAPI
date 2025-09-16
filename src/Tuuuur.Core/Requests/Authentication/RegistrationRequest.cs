using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Authentication;

public record RegistrationRequest(User User) : IRequest<EmptyResponse>;