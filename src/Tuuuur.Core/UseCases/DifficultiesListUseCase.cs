using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;

namespace Tuuuur.Core.UseCases;

internal class DifficultiesListUseCase(IUnitOfWork p_UnitOfWork, 
    ILogger<DifficultiesListUseCase> p_Logger): 
    ADbUseCase<GenericEntityListRequest<Difficulty>, GenericEntityListResponse<Difficulty>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityListResponse<Difficulty>> HandleLogic(GenericEntityListRequest<Difficulty> p_Request, CancellationToken p_CancellationToken)
    {
        IEnumerable<Difficulty> v_Difficulties = await m_UnitOfWork.DifficultyRepository.GetAllDifficultiesAsync(p_CancellationToken);
        return new GenericEntityListResponse<Difficulty>(v_Difficulties);
    }
}