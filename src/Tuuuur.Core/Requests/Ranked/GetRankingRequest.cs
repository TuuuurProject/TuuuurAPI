using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Ranked;

public record GetRankingRequest(int Page, int Size): IRequest<GenericEntityResponse<RankingPage>>;