using MediatR;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Requests.Ranked;

/// <summary>
/// Request to create a ranked duel party between two matched players.
/// Emitted by the MatchmakingWorker once a valid Elo pair is found.
/// </summary>
public record CreateRankedPartyRequest(User Player1, User Player2) : IRequest<EmptyResponse>;
