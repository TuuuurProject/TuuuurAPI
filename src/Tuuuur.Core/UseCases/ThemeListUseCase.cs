using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;

namespace Tuuuur.Core.UseCases;

internal class ThemeListUseCase(IUnitOfWork p_UnitOfWork, 
    ILogger<ThemeListUseCase> p_Logger): AUseCase(p_UnitOfWork, p_Logger), IRequestHandler<GenericEntityListRequest<Theme>, GenericEntityListResponse<Theme>>
{
    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task<GenericEntityListResponse<Theme>> Handle(GenericEntityListRequest<Theme> request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Theme> v_Themes = await m_UnitOfWork.ThemeRepository.GetAllThemesAsync(cancellationToken);
            return new GenericEntityListResponse<Theme>(v_Themes);
        }
        catch (Exception v_Ex)
        {
            m_Logger.LogError(v_Ex, "An error was thrown");
            return new GenericEntityListResponse<Theme>([v_Ex.ToError()]);
        }
    }
}