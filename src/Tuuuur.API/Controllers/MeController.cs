using Asp.Versioning;
using AutoMapper;
using FluentValidation.Results;
using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tuuuur.API.Configuration;
using Tuuuur.API.Presenters;
using Tuuuur.API.Presenters.Authentication;
using Tuuuur.API.Requests;
using Tuuuur.API.Requests.Users;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Authentication.Google;
using Tuuuur.Core.Requests.Users;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Security;
using Tuuuur.Domain.Token;

namespace Tuuuur.API.Controllers;

/// <summary>
/// Controller used to handle actions bound to authenticated user
/// </summary>
/// <remarks>
/// ctor
/// </remarks>
/// <param name="p_Logger"></param>
/// <param name="p_Mediator"></param>
/// <param name="p_ValidationPresenter"></param>
[ApiVersion("1")]
public class MeController(ILogger<MeController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
    : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
{
    /// <summary>
    /// Get current user
    /// </summary>
    /// <param name="p_Presenter"></param>
    /// <param name="p_CancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(User),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserAsync(
        [FromServices] GenericEntityPresenter<User> p_Presenter,
        CancellationToken p_CancellationToken)
    {
        p_Presenter.Handle(await m_Mediator.Send(new GetCurrentUserRequest(), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    /// <summary>
    /// Update current user avatar
    /// </summary>
    /// <param name="p_Request"></param>
    /// <param name="p_Validator"></param>
    /// <param name="p_Presenter"></param>
    /// <param name="p_CancellationToken"></param>
    /// <returns></returns>
    [HttpPut("avatar")]
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
    
    /// <summary>
    /// Update current user password
    /// </summary>
    /// <param name="p_Request"></param>
    /// <param name="p_Validator"></param>
    /// <param name="p_Presenter"></param>
    /// <param name="p_CancellationToken"></param>
    /// <returns></returns>
    [HttpPut("change-password")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(User),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUserPasswordAsync(
        [FromBody] ChangePasswordApiRequest p_Request,
        [FromServices] ChangePasswordRequestValidator p_Validator,
        [FromServices] GenericEntityPresenter<User> p_Presenter,
        CancellationToken p_CancellationToken)
    {
        ValidationResult v_Result = await p_Validator.ValidateAsync(p_Request, p_CancellationToken);

        if (!v_Result.IsValid)
        {
            m_ValidationPresenter.Handle(v_Result);
            return m_ValidationPresenter.ContentResult;
        }
        
        p_Presenter.Handle(await m_Mediator.Send(new ChangePasswordRequest(p_Request.CurrentPassword, p_Request.NewPassword), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    /// <summary>
    /// Delete current user
    /// </summary>
    /// <param name="p_Presenter"></param>
    /// <param name="p_CancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(User),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUserAsync(
        [FromServices] EmptyPresenter p_Presenter,
        CancellationToken p_CancellationToken)
    {
        p_Presenter.Handle(await m_Mediator.Send(new DeleteUserRequest(), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
}