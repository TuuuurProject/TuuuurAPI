using Asp.Versioning;
using AutoMapper;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tuuuur.API.Presenters;
using Tuuuur.API.Presenters.Authentication;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Security;
using LoginRequest = Tuuuur.Core.Requests.Authentication.LoginRequest;

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
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Get()
    {
        if (User?.Identity?.IsAuthenticated ?? false)
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
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult AdminOnly()
    {
        if (User?.Identity?.IsAuthenticated ?? false)
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
    /// <param name="p_LoginRequest"></param>
    /// <param name="p_Validator"></param>
    /// <param name="p_Presenter"></param>
    /// <param name="p_CancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [MapToApiVersion("1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync(
        [FromBody] Requests.LoginRequest p_LoginRequest,
        [FromServices] LoginRequestValidator p_Validator,
        [FromServices] EmptyPresenter p_Presenter,
        CancellationToken p_CancellationToken = default)
    {
        ValidationResult v_Result = await p_Validator.ValidateAsync(p_LoginRequest, p_CancellationToken);

        if (!v_Result.IsValid)
        {
            m_ValidationPresenter.Handle(v_Result);
            return m_ValidationPresenter.ContentResult;
        }

        p_Presenter.Handle(await m_Mediator.Send(new LoginRequest(p_LoginRequest.Login, p_LoginRequest.Password), p_CancellationToken));

        return p_Presenter.ContentResult;
    }

    /// <summary>
    /// Create an account
    /// </summary>
    /// <param name="p_RegisterRequest"></param>
    /// <param name="p_Mapper"></param>
    /// <param name="p_Validator"></param>
    /// <param name="p_Presenter"></param>
    /// <param name="p_CancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("register")]
    [MapToApiVersion("1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterRequest p_RegisterRequest,
        [FromServices] IMapper p_Mapper,
        [FromServices] RegisterRequestValidator p_Validator,
        [FromServices] EmptyPresenter p_Presenter,
        CancellationToken p_CancellationToken = default)
    {
        ValidationResult v_Result = await p_Validator.ValidateAsync(p_RegisterRequest, p_CancellationToken);

        if (!v_Result.IsValid)
        {
            return BadRequest(v_Result.ToDictionary());
        }

        p_Presenter.Handle(await m_Mediator.Send(new RegistrationRequest(p_Mapper.Map<User>(p_RegisterRequest)), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
    
    
    /// <summary>
    /// Validate account 2FA
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("2fa/verify")]
    [MapToApiVersion("1")]
    [ProducesResponseType(typeof(JwtAuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyAccount2FaAsync(
        [FromBody] ValidateAccountRequest p_RegisterRequest,
        [FromServices] ValidateAccountValidator p_Validator,
        [FromServices] JwtAuthenticationPresenter p_Presenter,
        CancellationToken p_CancellationToken = default)
    {
        ValidationResult v_Result = await p_Validator.ValidateAsync(p_RegisterRequest, p_CancellationToken);

        if (!v_Result.IsValid)
        {
            return BadRequest(v_Result.ToDictionary());
        }

        p_Presenter.Handle(await m_Mediator.Send(new VerifyAccountRequest(p_RegisterRequest.Login, p_RegisterRequest.Code), p_CancellationToken));

        return p_Presenter.ContentResult;
    }
}