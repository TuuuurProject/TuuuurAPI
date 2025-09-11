using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Tools;

public record HashRequest(string Value) : IRequest<StringResponse>;