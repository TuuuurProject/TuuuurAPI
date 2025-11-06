using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tuuuur.API.Extensions;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests;
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
public class GroupController(ILogger<PartyController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
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
        CancellationToken p_CancellationToken)
    {
        return Ok();
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
        CancellationToken p_CancellationToken)
    {
        return Ok();
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