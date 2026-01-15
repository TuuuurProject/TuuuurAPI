using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Interfaces.Data.Repositories;

public interface IQuestionRepository : IGenericRepository
{
    Task<IEnumerable<Question>> GetQuestionsByThemesIdsAndDifficultiesIdsAndNumberOfQuestionsAsync(IEnumerable<int> p_ThemesIds,
        IEnumerable<int> p_DifficultiesIds, int p_NbQuestions, CancellationToken p_CancellationToken = default);

    Task<GroupQuestion> GetQuestionByIdWithAnswerAsync(int p_Id, CancellationToken p_CancellationToken = default);
}