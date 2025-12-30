using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
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
                    .Include(p_P => p_P.PartyThemePth)
                        .ThenInclude(p_P => p_P.IdThemeNavigation)
                    .Include(p_P => p_P.PartyDifficultyPdf)
                        .ThenInclude(p_P => p_P.IdDifficultyNavigation)
                    .Include(p_P => p_P.PartyQuestionPqt)
                        .ThenInclude(p_P => p_P.IdQuestionNavigation)
                        .ThenInclude(p_P => p_P.AnswerAns)
                    .Include(p_P => p_P.PartyQuestionPqt)
                        .ThenInclude(p_P => p_P.UserPartyQuestionUpq.Where(p_UserPartyQuestionUpq =>  p_UserPartyQuestionUpq.IdUser == p_UserId))
                            .ThenInclude(p_P => p_P.IdAnswerNavigation)
                    .Include(p_P => p_P.PartyUserPus)
                    .Include(p_P => p_P.IdPartyTypeNavigation)
                    )
                    .AsNoTracking() 
                    .AsSplitQuery()
            .FirstOrDefaultAsync(p_CancellationToken);
        
        return v_PartyPty == null ? null : Mapper.Map<Party>(v_PartyPty);
    }
    
    public async Task UpdateAsync(Party p_Party)
    {
        PartyPty v_Entity = Mapper.Map<PartyPty>(p_Party);
        await base.UpdateAsync(v_Entity);
    }

    public async Task<HistoryPage> GetUserHistoryAsync(
        int p_UserId, 
        int p_Page, 
        int p_Size,
        CancellationToken p_CancellationToken = default)
    {
        int v_Skip = (p_Page - 1) * p_Size;
        
        long v_TotalCount = await CountAsync(p_P => p_P.IdUserHost == p_UserId, p_CancellationToken);
        
        List<PartyPty> v_Parties = await FindBy(
            p_P => p_P.IdUserHost == p_UserId,
            p_Include: p_Includes => p_Includes
                .Include(p_P => p_P.PartyThemePth)
                    .ThenInclude(p_P => p_P.IdThemeNavigation)
                .Include(p_P => p_P.PartyDifficultyPdf)
                    .ThenInclude(p_P => p_P.IdDifficultyNavigation)
                .Include(p_P => p_P.PartyQuestionPqt)
                    .ThenInclude(p_P => p_P.UserPartyQuestionUpq.Where(p_UserPartyQuestionUpq =>  p_UserPartyQuestionUpq.IdUser == p_UserId))
                .Include(p_P => p_P.PartyUserPus)
                .Include(p_P => p_P.IdPartyTypeNavigation))
            .OrderByDescending(p_P => p_P.Dt)
            .AsNoTracking() 
            .AsSplitQuery()
            .Skip(v_Skip)
            .Take(p_Size)
            .ToListAsync(p_CancellationToken);
        
        IEnumerable<History> v_History = Mapper.Map<IEnumerable<History>>(v_Parties);
        HistoryPage v_HistoryPage = new()
        {
            History = v_History,
            CurrentPage = p_Page,
            TotalPages = (int)Math.Ceiling((double)v_TotalCount / p_Size),
            TotalParties = (int)v_TotalCount
        };
        return v_HistoryPage;
    }
}