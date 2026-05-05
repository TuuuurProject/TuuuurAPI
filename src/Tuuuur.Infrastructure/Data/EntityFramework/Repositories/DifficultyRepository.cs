
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Data.Repositories;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Data.Mapping;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Repositories;

internal class DifficultyRepository(DbContext p_DbContext, IMapper p_Mapper, ILogger<DifficultyRepository> p_Logger)
    : GenericRepository<DifficultyDft>(p_DbContext, p_Mapper, p_Logger), IDifficultyRepository
{
    public async Task<IEnumerable<Difficulty>> GetAllDifficultiesAsync(CancellationToken p_CancellationToken = default)
    {
        IEnumerable<DifficultyDft> v_Entities = await GetAllAsync(p_CancellationToken : p_CancellationToken);
        return Mapper.Map<IEnumerable<Difficulty>>(v_Entities);
    }
}