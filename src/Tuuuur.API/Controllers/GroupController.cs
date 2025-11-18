using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tuuuur.API.Extensions;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Tuuuur.API.Controllers;

/// <summary>
/// Controller used to handle actions bound to group parties
/// </summary>§
/// <param name="p_Logger"></param>
/// <param name="p_Mediator"></param>
/// <param name="p_ValidationPresenter"></param>
[ApiVersion("1")]
public class GroupController(ILogger<SoloController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
    : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
{
    /// <summary>
    /// Create group party
    /// </summary>
    /// <returns></returns>
    [HttpPost("create")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(Guid),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateGroupPartyAsync(
        [FromServices] GenericEntityPresenter<Party> p_Presenter,
        CancellationToken p_CancellationToken)
    {
        p_Presenter.Handle(await m_Mediator.Send(new CreateGroupPartyRequest(), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    /// <summary>
    /// Join group party
    /// </summary>
    /// <returns></returns>
    [HttpPost("join")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(Guid),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> JoinGroupPartyAsync(
        [FromBody] CodeRequest p_Request,
        [FromServices] GenericEntityPresenter<Party> p_Presenter,
        CancellationToken p_CancellationToken)
    {
        p_Presenter.Handle(await m_Mediator.Send(new JoinGroupPartyRequest(p_Request.Code), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    /// <summary>
    /// Leave group party
    /// </summary>
    /// <returns></returns>
    [HttpPost("leave")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(Guid),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LeaveGroupPartyAsync(
        [FromServices] EmptyPresenter p_Presenter,
        CancellationToken p_CancellationToken)
    {
        p_Presenter.Handle(await m_Mediator.Send(new LeaveGroupPartyRequest(), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    /// <summary>
    /// Start group party
    /// </summary>
    /// <returns></returns>
    [HttpPost("{p_GroupeId}/start")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(Guid),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StartGroupPartyAsync(
        [FromRoute] Guid p_GroupeId,
        CancellationToken p_CancellationToken)
    {
        return Ok();
    }
    
    /// <summary>
    /// Submit answer for group party
    /// </summary>
    /// <returns></returns>
    [HttpPost("{p_GroupeId}/answer")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(Guid),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SubmitAnswerGroupPartyAsync(
        [FromRoute] Guid p_GroupeId,
        CancellationToken p_CancellationToken)
    {
        return Ok();
    }
}