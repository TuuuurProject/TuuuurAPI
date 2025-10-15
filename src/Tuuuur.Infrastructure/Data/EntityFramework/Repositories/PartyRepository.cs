
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

    public async Task<Party> GetByIdAsync(Guid p_PartyId, int p_UserId, CancellationToken p_CancellationToken = default)
    {
        PartyPty v_PartyPty = await FindBy(p_P => p_P.Id == p_PartyId, 
                p_Include: p_Includes => p_Includes
                    .Include(p_P => p_P.PartyQuestionPqt)
                    .ThenInclude(p_P => p_P.IdQuestionNavigation)
                    .ThenInclude(p_P => p_P.AnswerAns)
                    .Include(p_P => p_P.PartyQuestionPqt)
                    .ThenInclude(p_P => p_P.UserPartyQuestionUpq.Where(p_UserPartyQuestionUpq =>  p_UserPartyQuestionUpq.IdUser == p_UserId))
                    .Include(p_P => p_P.PartyUserPus)
                    .Include(p_P => p_P.IdPartyTypeNavigation)
                    )
            .FirstOrDefaultAsync(p_CancellationToken);
        return Mapper.Map<Party>(v_PartyPty);
    }

    public async Task UpdateAsync(Party p_Party)
    {
        PartyPty v_Entity = Mapper.Map<PartyPty>(p_Party);
        await base.UpdateAsync(v_Entity);
    }
}