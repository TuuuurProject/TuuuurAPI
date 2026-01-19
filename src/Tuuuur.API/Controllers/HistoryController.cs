using Asp.Versioning;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

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
    /// Get all history
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(IEnumerable<History>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHistoryListAsync(
        [FromQuery] PaginationRequest p_Query,
        [FromServices] PaginationRequestValidator p_Validator,
        [FromServices] GenericEntityPresenter<History> p_Presenter,
        CancellationToken p_CancellationToken)
    {
        ValidationResult v_Result = await p_Validator.ValidateAsync(p_Query, p_CancellationToken);

        if (!v_Result.IsValid)
        {
            m_ValidationPresenter.Handle(v_Result);
            return m_ValidationPresenter.ContentResult;
        }
        
        p_Presenter.Handle(await m_Mediator.Send(new GetAllHistoryRequest(p_Query.Page, p_Query.Size), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    /// <summary>
    /// Get history
    /// </summary>
    /// <returns></returns>
    [HttpGet("{p_PartyId:guid}")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(PartyBase),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHistoryAsync(
        [FromRoute] Guid p_PartyId,
        [FromServices] GenericEntityPresenter<PartyBase> p_Presenter,
        CancellationToken p_CancellationToken)
    {
        p_Presenter.Handle(await m_Mediator.Send(new GetHistoryRequest(p_PartyId), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
}