using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;

namespace Tuuuur.Core.UseCases;

internal class DifficultiesListUseCase(IUnitOfWork p_UnitOfWork, 
    ILogger<DifficultiesListUseCase> p_Logger): 
    AUseCase(p_UnitOfWork, p_Logger), IRequestHandler<GenericEntityListRequest<Difficulty>, GenericEntityListResponse<Difficulty>>
{
    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task<GenericEntityListResponse<Difficulty>> Handle(GenericEntityListRequest<Difficulty> request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Difficulty> v_Difficulties = await m_UnitOfWork.DifficultyRepository.GetAllDifficultiesAsync(cancellationToken);
            return new GenericEntityListResponse<Difficulty>(v_Difficulties);
        }
        catch (Exception v_Ex)
        {
            m_Logger.LogError(v_Ex, "An error was thrown");
            return new GenericEntityListResponse<Difficulty>([v_Ex.ToError()]);
        }
    }
}