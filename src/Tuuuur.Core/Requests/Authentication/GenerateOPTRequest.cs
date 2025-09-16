using MediatR;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Authentication;

/// <summary>
/// Request for OPT
/// </summary>
/// <param name="User"></param>
public record GenerateOPTRequest(User User) : IRequest<UserAuthResponse>;