using Asp.Versioning;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Security;

namespace Tuuuur.API.Controllers;

/// <summary>
/// Controller used to handle actions bound to ranked parties
/// </summary>§
/// <param name="p_Logger"></param>
/// <param name="p_Mediator"></param>
/// <param name="p_ValidationPresenter"></param>
[ApiVersion("1")]
public class RankedController(ILogger<RankedController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
    : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
{
    
    /// <summary>
    /// Fetch ranked party
    /// </summary>
    /// <returns></returns>
    [HttpGet("{p_PartyId:guid}")]
    [MapToApiVersion("1")]
    [Authorize(Roles = RolesType.User)]
    [ProducesResponseType(typeof(PartyBase),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRankedPartyAsync(
        [FromRoute] Guid p_PartyId,
        [FromServices] GenericEntityPresenter<RankedParty> p_Presenter,
        CancellationToken p_CancellationToken)
    {
        p_Presenter.Handle(await m_Mediator.Send(new GetRankedRequest(p_PartyId), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
}