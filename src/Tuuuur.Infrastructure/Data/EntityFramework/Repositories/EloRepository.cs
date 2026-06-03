using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Repositories;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Repositories;

internal class EloRepository(DbContext p_DbContext, IMapper p_Mapper, ILogger<EloRepository> p_Logger)
    : GenericRepository<EloElo>(p_DbContext, p_Mapper, p_Logger), IEloRepository
{
    public async Task<Elo> GetByUserAndThemeAsync(Guid p_UserId, int p_ThemeId, CancellationToken p_CancellationToken = default)
    {
        EloElo v_Entity = await FindBy(p_E => p_E.IdUser == p_UserId && p_E.IdTheme == p_ThemeId)
            .SingleOrDefaultAsync(p_CancellationToken);

        return Mapper.Map<Elo>(v_Entity);
    }

    public async Task UpdateValueAsync(Guid p_UserId, int p_ThemeId, int p_NewValue, CancellationToken p_CancellationToken = default)
    {
        EloElo v_Entity = await FindBy(
            p_E => p_E.IdUser == p_UserId && p_E.IdTheme == p_ThemeId,
            p_DisableTracking: false
        ).SingleOrDefaultAsync(p_CancellationToken);

        if (v_Entity is null) return;

        v_Entity.Value = p_NewValue;
        v_Entity.GamesPlayed += 1;
        await UpdateAsync(v_Entity);
    }
}
