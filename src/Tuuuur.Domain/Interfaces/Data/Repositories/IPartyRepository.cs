using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IPartyRepository : IGenericRepository
{
    Task<IMappingAddEntity<Party, IEntity>> CreatePartyAsync(Party p_Party,
        CancellationToken p_CancellationToken = default);

    Task<Party> GetByIdAsync(Guid p_PartyId, int p_UserId, CancellationToken p_CancellationToken = default);
    Task UpdateAsync(Party p_Party);

    Task<IEnumerable<History>> GetUserHistoryAsync(int p_UserId, int p_Page, int p_Size,
        CancellationToken p_CancellationToken = default);
}
