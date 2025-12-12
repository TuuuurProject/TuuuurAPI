
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IThemeRepository : IGenericRepository
{
    Task<IEnumerable<Theme>> GetAllThemesAsync(CancellationToken p_CancellationToken = default);
}