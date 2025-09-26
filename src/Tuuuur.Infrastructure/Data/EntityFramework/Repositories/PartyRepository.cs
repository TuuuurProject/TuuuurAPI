
using AutoMapper;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Data.Repositories;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Data.Mapping;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Repositories;

internal class PartyRepository(DbContext p_DbContext, IMapper p_Mapper, ILogger<PartyRepository> p_Logger)
    : GenericRepository<PartyPty>(p_DbContext, p_Mapper, p_Logger), IPartyRepository
{
    public async Task<IMappingAddEntity<Party, IEntity>> CreatePartyAsync(Party p_Party, CancellationToken p_CancellationToken = default)
    {
        IMappingAddEntity<Party, PartyPty> v_Mapping =
            new MappingAddEntity<Party, PartyPty>(Mapper, p_Party);

        await AddAsync(v_Mapping.DtoEntity, p_CancellationToken);
        return v_Mapping;
    }
}