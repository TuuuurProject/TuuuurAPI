using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IUserPartyQuestionRepository : IGenericRepository
{
    Task<IMappingAddEntity<UserPartyQuestion, IEntity>> CreateUserPartyQuestionAsync(
        UserPartyQuestion p_Party,
        CancellationToken p_CancellationToken = default);
    Task UpdateAsync(UserPartyQuestion p_UserPartyQuestion);
    Task<IEnumerable<UserPartyQuestion>> GetUserScoresByProjectIdAsync(Guid p_ProjectId, CancellationToken p_CancellationToken = default);
}