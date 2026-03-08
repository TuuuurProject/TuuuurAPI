using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IQuestionRepository : IGenericRepository
{
    Task<IEnumerable<Question>> GetQuestionsByThemesIdsAndDifficultiesIdsAndNumberOfQuestionsAsync(IEnumerable<int> p_ThemesIds,
        IEnumerable<int> p_DifficultiesIds, int p_NbQuestions, CancellationToken p_CancellationToken = default);

    Task<Question> GetQuestionByIdWithAnswerAsync(int p_Id, CancellationToken p_CancellationToken = default);

    /// <summary>Returns a random question not in the exclude list, with optional theme and difficulty filters.</summary>
    Task<Question> GetRandomQuestionExcludingAsync(
        List<int> p_ExcludesQuestions,
        IEnumerable<int>? p_ThemeIds,
        IEnumerable<int>? p_DifficultyIds,
        CancellationToken p_CancellationToken = default);
}