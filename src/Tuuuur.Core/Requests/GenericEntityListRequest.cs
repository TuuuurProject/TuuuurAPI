using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests;

public record GenericEntityListRequest<T> : IRequest<GenericEntityListResponse<T>> where T : IBOEntity;