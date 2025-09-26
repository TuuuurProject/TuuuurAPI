using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IPartyRepository : IGenericRepository
{
    Task<IMappingAddEntity<Party, IEntity>> CreatePartyAsync(Party p_Party,
        CancellationToken p_CancellationToken = default);
}
