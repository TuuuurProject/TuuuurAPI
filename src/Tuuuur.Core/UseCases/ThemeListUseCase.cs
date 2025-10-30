using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;

namespace Tuuuur.Core.UseCases;

internal class ThemeListUseCase(IUnitOfWork p_UnitOfWork, 
    ILogger<ThemeListUseCase> p_Logger): ADbUseCase<GenericEntityListRequest<Theme>, GenericEntityListResponse<Theme>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityListResponse<Theme>> HandleLogic(GenericEntityListRequest<Theme> p_Request, CancellationToken p_CancellationToken)
    {
        IEnumerable<Theme> v_Themes = await m_UnitOfWork.ThemeRepository.GetAllThemesAsync(p_CancellationToken);
        return new GenericEntityListResponse<Theme>(v_Themes);
    }
}