using Asp.Versioning;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests.Users;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.API.Controllers;

/// <summary>
/// Controller used to handle actions bound to user
/// </summary>
/// <remarks>
/// ctor
/// </remarks>
/// <param name="p_Logger"></param>
/// <param name="p_Mediator"></param>
/// <param name="p_ValidationPresenter"></param>
[ApiVersion("1")]
public class UsersController(ILogger<UsersController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
    : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
{
    /// <summary>
    /// Update user avatar
    /// </summary>
    /// <param name="p_Request"></param>
    /// <param name="p_Validator"></param>
    /// <param name="p_Presenter"></param>
    /// <param name="p_CancellationToken"></param>
    /// <returns></returns>
    [HttpPut("Avatar")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(GenericEntityListResponse<User>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>),StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUserAvatarAsync(
        [FromBody] UserAvatarApiRequest p_Request,
        [FromServices] UserAvatarApiRequestValidator p_Validator,
        [FromServices] GenericEntityPresenter<User> p_Presenter,
        CancellationToken p_CancellationToken = default)
    {
        ValidationResult v_Result = await p_Validator.ValidateAsync(p_Request, p_CancellationToken);

        if (!v_Result.IsValid)
        {
            m_ValidationPresenter.Handle(v_Result);
            return m_ValidationPresenter.ContentResult;
        }

        p_Presenter.Handle(await m_Mediator.Send(new UpdateUserAvatarRequest(p_Request.Avatar), p_CancellationToken));
        return p_Presenter.ContentResult;
    }
}