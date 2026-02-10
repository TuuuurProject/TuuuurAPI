using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Group;

public record AnswerQuestionGroupPartyRequest(int AnswerId, string UserEmail) : IRequest<EmptyResponse>;
