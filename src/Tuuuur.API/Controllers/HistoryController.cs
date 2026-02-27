using Asp.Versioning;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Security;

namespace Tuuuur.API.Controllers;

/// <summary>
/// Controller used to handle actions bound history
/// </summary>§
/// <param name="p_Logger"></param>
/// <param name="p_Mediator"></param>
/// <param name="p_ValidationPresenter"></param>
[ApiVersion("1")]
public class HistoryController(
    ILogger<HistoryController> p_Logger,
    IMediator p_Mediator,
    ValidationPresenter p_ValidationPresenter)
    : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
{
    /// <summary>
    /// Get history
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1")]
    [Authorize(Roles = RolesType.User)]
    [ProducesResponseType(typeof(IEnumerable<HistoryPage>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHistoryAsync(
        [FromQuery] PaginationRequest p_Query,
        [FromServices] PaginationRequestValidator p_Validator,
        [FromServices] GenericEntityPresenter<HistoryPage> p_Presenter,
        CancellationToken p_CancellationToken)
    {
        ValidationResult v_Result = await p_Validator.ValidateAsync(p_Query, p_CancellationToken);

        if (!v_Result.IsValid)
        {
            m_ValidationPresenter.Handle(v_Result);
            return m_ValidationPresenter.ContentResult;
        }
        
        p_Presenter.Handle(await m_Mediator.Send(new GetHistoryRequest(p_Query.Page, p_Query.Size), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
}