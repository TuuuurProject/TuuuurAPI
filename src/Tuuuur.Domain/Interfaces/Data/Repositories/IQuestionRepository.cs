using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IQuestionRepository : IGenericRepository
{
    Task<IEnumerable<Question>> GetQuestionsByThemesIdsAndDifficultiesIdsAndNumberOfQuestionsAsync(int[] p_ThemesIds,
        int[] p_DifficultiesIds, int p_NbQuestions, CancellationToken p_CancellationToken = default);

    Task<Question> GetQuestionByIdWithAnswerAsync(int p_Id, CancellationToken p_CancellationToken = default);
}