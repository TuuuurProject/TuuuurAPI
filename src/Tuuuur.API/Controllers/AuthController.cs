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
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Authentication.Google;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Security;
using Tuuuur.Domain.Token;

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
public class AuthController(ILogger<AuthController> p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
    : BaseController(p_Logger, p_Mediator, p_ValidationPresenter)
{
    /// <summary>
    /// Display user infos
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(void),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Get()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            m_Logger.LogInformation("User is logged");
            return Ok(new { User = User.Identity.Name, Claims = User.Claims.Select(p_C => new { p_C.Type, p_C.Value }) });
        }

        m_Logger.LogInformation("User is not authorized");
        return Unauthorized();
    }

    /// <summary>
    /// Display user info (Admin only)
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = RolesType.Admin)]
    [HttpGet("[action]")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(void),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult AdminOnly()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            if (!User.IsInRole(RolesType.Admin)) return Forbid();

            m_Logger.LogInformation("User is logged");
            
            return Ok(new { User = User.Identity.Name, Claims = User.Claims.Select(p_C => new { p_C.Type, p_C.Value }) });
        }

        m_Logger.LogInformation("User is not authorized");
        return Unauthorized();
    }

    /// <summary>
    /// Login an existing user
    /// </summary>
    /// <param name="p_AuthenticateApiRequest"></param>
    /// <param name="p_Validator"></param>
    /// <param name="p_Presenter"></param>
    /// <param name="p_CancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(void),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LoginAsync(
        [FromBody] AuthenticateApiRequest p_AuthenticateApiRequest,
        [FromServices] AuthenticateRequestValidator p_Validator,
        [FromServices] EmptyPresenter p_Presenter,
        CancellationToken p_CancellationToken = default)
    {
        ValidationResult v_Result = await p_Validator.ValidateAsync(p_AuthenticateApiRequest, p_CancellationToken);

        if (!v_Result.IsValid)
        {
            m_ValidationPresenter.Handle(v_Result);
            return m_ValidationPresenter.ContentResult;
        }

        p_Presenter.Handle(await m_Mediator.Send(new LoginRequest(p_AuthenticateApiRequest.Login, p_AuthenticateApiRequest.Password), p_CancellationToken));

        return p_Presenter.ContentResult;
    }

    /// <summary>
    /// Create an account
    /// </summary>
    /// <param name="p_RegisterApiRequest"></param>
    /// <param name="p_Mapper"></param>
    /// <param name="p_Validator"></param>
    /// <param name="p_Presenter"></param>
    /// <param name="p_CancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("register")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(void),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterApiRequest p_RegisterApiRequest,
        [FromServices] IMapper p_Mapper,
        [FromServices] RegisterRequestValidator p_Validator,
        [FromServices] EmptyPresenter p_Presenter,
        CancellationToken p_CancellationToken = default)
    {
        ValidationResult v_Result = await p_Validator.ValidateAsync(p_RegisterApiRequest, p_CancellationToken);

        if (!v_Result.IsValid)
        {
            m_ValidationPresenter.Handle(v_Result);
            return m_ValidationPresenter.ContentResult;
        }

        p_Presenter.Handle(await m_Mediator.Send(new RegistrationRequest(p_Mapper.Map<User>(p_RegisterApiRequest)), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    
    /// <summary>
    /// Validate account 2FA
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("2fa/verify")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(UserToken), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyAccount2FaAsync(
        [FromBody] ValidateAccountApiRequest p_RegisterApiRequest,
        [FromServices] ValidateAccountValidator p_Validator,
        [FromServices] JwtAuthenticationPresenter p_Presenter,
        CancellationToken p_CancellationToken = default)
    {
        ValidationResult v_Result = await p_Validator.ValidateAsync(p_RegisterApiRequest, p_CancellationToken);

        if (!v_Result.IsValid)
        {
            m_ValidationPresenter.Handle(v_Result);
            return m_ValidationPresenter.ContentResult;
        }

        p_Presenter.Handle(await m_Mediator.Send(new VerifyAccountRequest(p_RegisterApiRequest.Login, p_RegisterApiRequest.Code), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    /// <summary>
    /// Forgot password
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("password/forgot")]
    [MapToApiVersion("1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPasswordAsync(
        [FromBody] LoginApiRequest p_ApiRequest,
        [FromServices] LoginRequestValidator p_Validator,
        [FromServices] EmptyPresenter p_Presenter,
        CancellationToken p_CancellationToken = default)
    {
        ValidationResult v_Result = await p_Validator.ValidateAsync(p_ApiRequest, p_CancellationToken);

        if (!v_Result.IsValid)
        {
            m_ValidationPresenter.Handle(v_Result);
            return m_ValidationPresenter.ContentResult;
        }

        p_Presenter.Handle(await m_Mediator.Send(new ForgotPasswordRequest(p_ApiRequest.Login), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    /// <summary>
    /// Reset password
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("password/reset")]
    [MapToApiVersion("1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPasswordAsync(
        [FromBody] ResetPasswordApiRequest p_ApiRequest,
        [FromServices] ResetPasswordRequestValidator p_Validator,
        [FromServices] EmptyPresenter p_Presenter,
        CancellationToken p_CancellationToken = default)
    {
        ValidationResult v_Result = await p_Validator.ValidateAsync(p_ApiRequest, p_CancellationToken);

        if (!v_Result.IsValid)
        {
            m_ValidationPresenter.Handle(v_Result);
            return m_ValidationPresenter.ContentResult;
        }

        p_Presenter.Handle(
            await m_Mediator.Send(
                new ResetPasswordRequest(p_ApiRequest.Login, p_ApiRequest.Password, p_ApiRequest.Code), 
                p_CancellationToken)
            );

        return p_Presenter.ContentResult;
    }

    /// <summary>
    /// Authentification with Google
    /// </summary>
    /// <param name="p_Request"></param>
    /// <param name="p_Configuration"></param>
    /// <param name="p_Presenter"></param>
    /// <param name="p_CancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserToken), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(IEnumerable<ErrorDto>), StatusCodes.Status500InternalServerError)]    
    [HttpPost("google")]
    public async Task<IActionResult> GoogleAuthentificationAsync(
        [FromBody] TokenRequest p_Request,
        [FromServices] GoogleConfiguration p_Configuration,
        [FromServices] JwtAuthenticationPresenter p_Presenter,
        CancellationToken p_CancellationToken = default)
    {
        try
        {
            GoogleJsonWebSignature.Payload v_Payload = await GoogleJsonWebSignature.ValidateAsync(
                p_Request.Token,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [p_Configuration.ClientId]
                }
            );

            p_Presenter.Handle(await m_Mediator.Send(new GoogleAuthentificationRequest(v_Payload.Email), p_CancellationToken));
            
            return p_Presenter.ContentResult;
        }
        catch (Exception v_Ex)
        {
            return Unauthorized(new ErrorDto(DomainErrors.Authentication.Invalid,v_Ex.Message ));
        }
    }
}