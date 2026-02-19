using Asp.Versioning;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.API.Controllers;

/// <summary>
/// Controller used to handle actions bound to group parties
/// </summary>§
/// <param name="p_Logger"></param>
/// <param name="p_Mediator"></param>
/// <param name="p_ValidationPresenter"></param>
[ApiVersion("1")]
public class GroupController(ILogger<GroupController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
    : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
{
    /// <summary>
    /// Create group party
    /// </summary>
    /// <returns></returns>
    [HttpPost("create")]
    [ProducesResponseType(typeof(PartyBase),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePartyAsync(
        [FromServices] GenericEntityPresenter<GroupParty> p_Presenter,
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
    [ProducesResponseType(typeof(PartyBase),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> JoinPartyAsync(
        [FromBody] CodeRequest p_Request,
        [FromServices] GenericEntityPresenter<GroupParty> p_Presenter,
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
    [ProducesResponseType(typeof(void),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LeavePartyAsync(
        [FromServices] EmptyPresenter p_Presenter,
        CancellationToken p_CancellationToken)
    {
        p_Presenter.Handle(await m_Mediator.Send(new LeaveGroupPartyRequest(), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    /// <summary>
    /// Group settings
    /// </summary>
    /// <returns></returns>
    [HttpPost("settings")]
    [ProducesResponseType(typeof(PartyBase),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePartySettingsAsync(
        [FromBody] GroupSettingsRequest p_Request,
        [FromServices] EmptyPresenter p_Presenter,
        CancellationToken p_CancellationToken)
    {
        p_Presenter.Handle(await m_Mediator.Send(new EditGroupSettingsRequest(p_Request.Themes, p_Request.Difficulties, p_Request.NbQuestions, p_Request.ScoreEachRound), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    
    /// <summary>
    /// Remove user on party
    /// </summary>
    /// <returns></returns>
    [HttpDelete("user/{p_UserId}")]
    [ProducesResponseType(typeof(bool),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExpelUserOnPartyAsync(
        [FromRoute] int p_UserId,
        [FromServices] EmptyPresenter p_Presenter,
        CancellationToken p_CancellationToken)
    {
        p_Presenter.Handle(await m_Mediator.Send(new ExpelUserOnPartyRequest(p_UserId), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
}