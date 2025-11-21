using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Requests.Parties;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Tuuuur.API.Controllers;

/// <summary>
/// Controller used to handle actions bound to party
/// </summary>§
/// <param name="p_Logger"></param>
/// <param name="p_Mediator"></param>
/// <param name="p_ValidationPresenter"></param>
[ApiVersion("1")]
public class SoloController(ILogger<SoloController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
    : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
{
    /// <summary>
    /// Create solo party
    /// </summary>
    /// <returns></returns>
    [HttpPost()]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(Guid),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSoloPartyAsync(
        [FromBody] CreateSoloPartyApiRequest p_ApiRequest,
        [FromServices] CreateSoloPartyRequestValidator p_Validator,
        [FromServices] GuidPresenter p_Presenter,
        CancellationToken p_CancellationToken)
    {
        ValidationResult v_Result = await p_Validator.ValidateAsync(p_ApiRequest, p_CancellationToken);

        if (!v_Result.IsValid)
        {
            m_ValidationPresenter.Handle(v_Result);
            return m_ValidationPresenter.ContentResult;
        }

        p_Presenter.Handle(await m_Mediator.Send(new CreateSoloPartyRequest(p_ApiRequest.Themes, p_ApiRequest.Difficulties, p_ApiRequest.NbQuestions), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    /// <summary>
    /// Fetch the solo party
    /// </summary>
    /// <returns></returns>
    [HttpGet("{p_PartyId:guid}")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(Party),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPartyStateAsync(
        [FromRoute] Guid p_PartyId,
        [FromServices] GenericEntityPresenter<Party> p_Presenter,
        CancellationToken p_CancellationToken)
    {
        p_Presenter.Handle(await m_Mediator.Send(new GetSoloPartyStateRequest(p_PartyId), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    /// <summary>
    /// Answer to a solo question
    /// </summary>
    /// <param name="p_PartyId"></param>
    /// <param name="p_Request"></param>
    /// <param name="p_Presenter"></param>
    /// <param name="p_CancellationToken"></param>
    /// <returns></returns>
    [HttpPost("{p_PartyId:guid}")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(Party),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePartyStateAsync(
        [FromRoute] Guid p_PartyId,
        [FromBody] AnswerApiRequest p_Request,
        [FromServices] GenericEntityPresenter<Party> p_Presenter,
        CancellationToken p_CancellationToken)
    {
        p_Presenter.Handle(await m_Mediator.Send(new UpdateSoloPartyStateRequest(p_PartyId, p_Request.AnswerId), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    /// <summary>
    /// Get history
    /// </summary>
    /// <returns></returns>
    [HttpGet("history")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(IEnumerable<Party>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHistoryAsync(
        [FromQuery] PaginationRequest p_Query,
        [FromServices] PaginationRequestValidator p_Validator,
        [FromServices] GenericEntityListPresenter<History> p_Presenter,
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