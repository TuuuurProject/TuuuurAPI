using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Ranked;

public record AnswerQuestionRankedRequest(int AnswerId, Guid UserId) : IRequest<EmptyResponse>;
