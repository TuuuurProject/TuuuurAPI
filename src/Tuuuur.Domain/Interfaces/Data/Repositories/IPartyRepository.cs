using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IPartyRepository : IGenericRepository
{
    Task<IMappingAddEntity<PartyBase, IEntity>> CreatePartyAsync(PartyBase p_Party,
        CancellationToken p_CancellationToken = default);
    Task<Party> GetPartyByIdAsync(Guid p_PartyId, Guid p_UserId, CancellationToken p_CancellationToken = default);
    Task<GroupParty> GetGroupByIdAsync(Guid p_PartyId, Guid p_UserId, CancellationToken p_CancellationToken = default);
    Task<RankedParty> GetRankedByIdAsync(Guid p_PartyId, Guid p_UserId, CancellationToken p_CancellationToken = default);
    Task UpdateAsync(Party p_Party);
    Task<HistoryPage> GetUserHistoryAsync(Guid p_UserId, int p_Page, int p_Size, CancellationToken p_CancellationToken = default);
    
}
