
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

internal class QuestionRepository(DbContext p_DbContext, IMapper p_Mapper, ILogger<QuestionRepository> p_Logger)
    : GenericRepository<QuestionQst>(p_DbContext, p_Mapper, p_Logger), IQuestionRepository
{
    public async Task<IEnumerable<Question>> GetQuestionsByThemesIdsAndDifficultiesIdsAndNumberOfQuestionsAsync(
        int[] p_ThemesIds, int[] p_DifficultiesIds, int p_NbQuestions, CancellationToken p_CancellationToken = default)
    {
        List<QuestionQst> v_Query = await FindBy(p_P => 
            p_P.QuestionThemeQth.Any(p_Theme => p_ThemesIds.Contains(p_Theme.IdTheme)) && 
            p_DifficultiesIds.Contains(p_P.IdDifficulty))
            .OrderBy(p_P => Guid.NewGuid())
            .Take(p_NbQuestions)
            .ToListAsync(cancellationToken: p_CancellationToken);

        return Mapper.Map<IEnumerable<Question>>(v_Query);
    }

    public async Task<Question> GetQuestionByIdWithAnswerAsync(int p_Id, CancellationToken p_CancellationToken = default)
    {
        QuestionQst v_QuestionQst = await FindBy(
            p_P => p_P.Id == p_Id, 
            p_Include: p_Include => p_Include
                .Include(p_P => p_P.AnswerAns)
                .Include(p_P => p_P.PartyQuestionPqt))
            .FirstOrDefaultAsync(p_CancellationToken);
        return Mapper.Map<Question>(v_QuestionQst);
    }
}