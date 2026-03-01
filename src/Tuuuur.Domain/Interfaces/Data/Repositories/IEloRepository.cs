using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IEloRepository : IGenericRepository
{
    /// <summary>
    /// Returns the Elo record for the given user and theme, or null if not found.
    /// </summary>
    Task<Elo> GetByUserAndThemeAsync(Guid p_UserId, int p_ThemeId, CancellationToken p_CancellationToken = default);

    /// <summary>
    /// Sets a new Elo value for the given user/theme combination.
    /// </summary>
    Task UpdateValueAsync(Guid p_UserId, int p_ThemeId, int p_NewValue, CancellationToken p_CancellationToken = default);
}
