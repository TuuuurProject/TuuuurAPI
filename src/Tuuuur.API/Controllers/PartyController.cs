using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tuuuur.API.Extensions;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Tuuuur.API.Controllers;

/// <summary>
/// Controller used to handle actions bound to authentication and users
/// </summary>
/// <remarks>
/// ctor
/// </remarks>
/// <param name="p_Logger"></param>
/// <param name="p_Mediator"></param>
/// <param name="p_ValidationPresenter"></param>
[ApiVersion("1")]
public class PartyController(ILogger<PartyController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
    : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
{
    /// <summary>
    /// Create solo party
    /// </summary>
    /// <returns></returns>
    [HttpPost("Solo")]
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
}