using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IUserPartyQuestionRepository : IGenericRepository
{
    Task UpdateGroupAsync(Guid p_PartyId, int p_UserId, int p_QuestionId, DateTime? p_DtAnsweredAt,
        DateTime p_DtPresentedAt, bool p_Correct, int? p_IdAnswer, int p_Score,
        CancellationToken p_CancellationToken = default);
    Task CreateUserQuestionAsync(Guid p_PartyId, int p_UserId, int p_QuestionId,
        CancellationToken p_CancellationToken = default);

    Task UpdateUserQuestionAsync(Guid p_PartyId, int p_UserId, int p_QuestionId, bool p_Correct, int? p_IdAnswer, int p_Score,
        CancellationToken p_CancellationToken = default);
}