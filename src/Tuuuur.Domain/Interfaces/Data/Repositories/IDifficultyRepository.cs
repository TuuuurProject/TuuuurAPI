
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IDifficultyRepository : IGenericRepository
{
    Task<IEnumerable<Difficulty>> GetAllDifficultiesAsync(CancellationToken p_CancellationToken = default);
}