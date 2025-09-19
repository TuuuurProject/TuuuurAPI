using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tuuuur.API.Presenters;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.API.Controllers;

/// <summary>
/// Controller used to handle actions bound to theme
/// </summary>
/// <remarks>
/// ctor
/// </remarks>
/// <param name="p_Logger"></param>
/// <param name="p_Mediator"></param>
/// <param name="p_ValidationPresenter"></param>
[ApiVersion("1")]
public class ThemeController(ILogger<ThemeController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
    : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
{
    /// <summary>
    /// Get themes
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(GenericEntityListResponse<Theme>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>),StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetThemeListAsync(
        [FromServices] GenericEntityListPresenter<Theme> p_Presenter,
        CancellationToken p_CancellationToken = default)
    {
        p_Presenter.Handle(await m_Mediator.Send(new GenericEntityListRequest<Theme>(), p_CancellationToken));
        return p_Presenter.ContentResult;
    }
}