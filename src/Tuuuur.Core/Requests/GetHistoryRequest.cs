using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests;

public record GetHistoryRequest(int Page, int Size): IRequest<GenericEntityResponse<HistoryPage>>;