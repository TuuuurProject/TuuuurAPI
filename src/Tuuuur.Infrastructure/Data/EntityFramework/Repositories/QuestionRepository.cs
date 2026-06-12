
using AutoMapper;
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
        IEnumerable<int> p_ThemesIds, IEnumerable<int> p_DifficultiesIds, int p_NbQuestions, CancellationToken p_CancellationToken = default)
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
                .Include(p_P => p_P.QuestionThemeQth)
                    .ThenInclude(p_P => p_P.IdThemeNavigation)
                .Include(p_P => p_P.IdDifficultyNavigation))
            .FirstOrDefaultAsync(p_CancellationToken);
        return Mapper.Map<Question>(v_QuestionQst);
    }

    public async Task<Question> GetRandomQuestionExcludingAsync(List<int> p_ExcludesQuestions, CancellationToken p_CancellationToken = default)
    {
        QuestionQst v_Entity = await FindBy(p_P => !p_ExcludesQuestions.Contains(p_P.Id))
            .OrderBy(p_P => Guid.NewGuid())
            .FirstOrDefaultAsync(cancellationToken: p_CancellationToken);

        return await GetQuestionByIdWithAnswerAsync(v_Entity.Id, p_CancellationToken);
    }
    
    public async Task<Question> GetRandomQuestionExcludingWithFiltersAsync(
        List<int> p_ExcludesQuestions,
        List<int> p_DifficultyIds,
        List<int> p_ThemeIds,
        CancellationToken p_CancellationToken = default)
    {
        bool v_FilterByTheme = p_ThemeIds is { Count: > 0 };

        IQueryable<QuestionQst> v_Query = FindBy(p_P =>
            !p_ExcludesQuestions.Contains(p_P.Id) &&
            p_DifficultyIds.Contains(p_P.IdDifficulty) &&
            (!v_FilterByTheme || p_P.QuestionThemeQth.Any(p_Qt => p_ThemeIds!.Contains(p_Qt.IdTheme)))
        ).OrderBy(p_P => Guid.NewGuid());

        QuestionQst v_Entity = await v_Query.FirstOrDefaultAsync(cancellationToken: p_CancellationToken);

        // Fallback: if no question matches the pool filters (e.g. pool exhausted),
        // fall back to any unseen question regardless of difficulty/theme.
        if (v_Entity is not null)
        {
            return await GetQuestionByIdWithAnswerAsync(v_Entity.Id, p_CancellationToken);
        }

        p_Logger.LogWarning(
            "No question found for DifficultyIds=[{Difficulties}] ThemeIds=[{Themes}] — falling back to unfiltered selection.",
            string.Join(",", p_DifficultyIds),
            p_ThemeIds is null ? "any" : string.Join(",", p_ThemeIds));

        return await GetRandomQuestionExcludingAsync(p_ExcludesQuestions, p_CancellationToken);

    }
}