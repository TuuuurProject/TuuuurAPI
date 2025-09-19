using MediatR;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests;

public record GenericEntityRequest<T> : IRequest<GenericEntityRequest<T>>;