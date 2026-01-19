using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests;

public record GetAllHistoryRequest(int Page, int Size): IRequest<GenericEntityResponse<History>>;