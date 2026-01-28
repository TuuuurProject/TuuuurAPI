using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Data.Repositories;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Data.Mapping;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Repositories;

internal class UserPartyQuestionRepository(DbContext p_DbContext, IMapper p_Mapper, ILogger<UserPartyQuestionRepository> p_Logger)
    : GenericRepository<UserPartyQuestionUpq>(p_DbContext, p_Mapper, p_Logger), IUserPartyQuestionRepository
{
    public async Task UpdateGroupAsync(Guid p_PartyId, int p_UserId, int p_QuestionId, DateTime? p_DtAnsweredAt, DateTime p_DtPresentedAt, bool p_Correct,  int? p_IdAnswer, int p_Score,
        CancellationToken p_CancellationToken = default)
    {
        UserPartyQuestionUpq v_UserPartyQuestionUpqs = await FindBy(null,
                p_Include: p_Include => p_Include
                    .Include(p_P => p_P.IdPartyQuestionNavigation)
            ).Where(p_P => p_P.IdPartyQuestionNavigation.IdQuestion == p_QuestionId 
                           && p_P.IdPartyQuestionNavigation.IdParty == p_PartyId 
                           && p_P.IdUser == p_UserId)
            .FirstOrDefaultAsync(p_CancellationToken);

        v_UserPartyQuestionUpqs.DtPresentedAt = p_DtPresentedAt;
        v_UserPartyQuestionUpqs.DtAnsweredAt = p_DtAnsweredAt;
        v_UserPartyQuestionUpqs.IdAnswer = p_IdAnswer;
        v_UserPartyQuestionUpqs.Score = p_Score;
        v_UserPartyQuestionUpqs.Correct = p_Correct;
        await UpdateAsync(v_UserPartyQuestionUpqs);
    }
    public async Task CreateUserQuestionAsync(Guid p_PartyId, int p_UserId, int p_QuestionId,
        CancellationToken p_CancellationToken = default)
    {
        UserPartyQuestionUpq v_UserPartyQuestionUpqs = await FindBy(null,
                p_Include: p_Include => p_Include
                    .Include(p_P => p_P.IdPartyQuestionNavigation)
            ).Where(p_P => p_P.IdPartyQuestionNavigation.IdQuestion == p_QuestionId 
                           && p_P.IdPartyQuestionNavigation.IdParty == p_PartyId 
                           && p_P.IdUser == p_UserId)
            .FirstOrDefaultAsync(p_CancellationToken);
        
        v_UserPartyQuestionUpqs.DtPresentedAt = DateTime.UtcNow;
        await UpdateAsync(v_UserPartyQuestionUpqs);
    }

    public async Task UpdateUserQuestionAsync(Guid p_PartyId, int p_UserId, int p_QuestionId, bool p_Correct,  int? p_IdAnswer, int p_Score,
        CancellationToken p_CancellationToken = default)
    {
        UserPartyQuestionUpq v_UserPartyQuestionUpqs = await FindBy(null,
                p_Include: p_Include => p_Include
                    .Include(p_P => p_P.IdPartyQuestionNavigation)
            ).Where(p_P => p_P.IdPartyQuestionNavigation.IdQuestion == p_QuestionId 
                           && p_P.IdPartyQuestionNavigation.IdParty == p_PartyId 
                           && p_P.IdUser == p_UserId)
            .FirstOrDefaultAsync(p_CancellationToken);
        
        v_UserPartyQuestionUpqs.DtAnsweredAt = DateTime.UtcNow;
        v_UserPartyQuestionUpqs.IdAnswer = p_IdAnswer;
        v_UserPartyQuestionUpqs.Score = p_Score;
        v_UserPartyQuestionUpqs.Correct = p_Correct;
        await UpdateAsync(v_UserPartyQuestionUpqs);
    }
}