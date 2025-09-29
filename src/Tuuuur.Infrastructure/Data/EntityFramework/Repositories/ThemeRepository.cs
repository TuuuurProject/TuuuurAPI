
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Data.Repositories;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Data.Mapping;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Repositories;

internal class ThemeRepository(DbContext p_DbContext, IMapper p_Mapper, ILogger<ThemeRepository> p_Logger)
    : GenericRepository<ThemeThm>(p_DbContext, p_Mapper, p_Logger), IThemeRepository
{
    public async Task<IEnumerable<Theme>> GetAllThemesAsync(CancellationToken p_CancellationToken = default)
    {
        IEnumerable<ThemeThm> v_Entities = await GetAllAsync(p_CancellationToken : p_CancellationToken);
        return Mapper.Map<IEnumerable<Theme>>(v_Entities);
    }
}