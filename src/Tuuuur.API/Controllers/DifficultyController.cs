using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tuuuur.API.Presenters;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Security;

namespace Tuuuur.API.Controllers;

/// <summary>
/// Controller used to handle actions bound difficulty
/// </summary>
/// <remarks>
/// ctor
/// </remarks>
/// <param name="p_Logger"></param>
/// <param name="p_Mediator"></param>
/// <param name="p_ValidationPresenter"></param>
[ApiVersion("1")]
public class DifficultyController(ILogger<DifficultyController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
    : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
{
    /// <summary>
    /// Get difficulties
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1")]
    [Authorize(Roles = RolesType.User)]
    [ProducesResponseType(typeof(GenericEntityListResponse<Difficulty>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>),StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDifficultiesListAsync(
        [FromServices] GenericEntityListPresenter<Difficulty> p_Presenter,
        CancellationToken p_CancellationToken = default)
    {
        p_Presenter.Handle(await m_Mediator.Send(new GenericEntityListRequest<Difficulty>(), p_CancellationToken));
        return p_Presenter.ContentResult;
    }
}