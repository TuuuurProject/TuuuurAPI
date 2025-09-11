using Tuuuur.API.Presenters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Tuuuur.API.Controllers;

/// <summary>
/// Base controller used for API controllers
/// </summary>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Logger
    /// </summary>
    protected readonly ILogger m_Logger;

    /// <summary>
    /// Mediator Service
    /// </summary>
    protected readonly IMediator m_Mediator;

    /// <summary>
    /// ValidationPresenter used to return validation errors if any
    /// </summary>
    protected readonly ValidationPresenter m_ValidationPresenter;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="p_Logger"></param>
    /// <param name="p_Mediator"></param>
    /// <param name="p_ValidationPresenter"></param>
    protected BaseController(ILogger p_Logger, IMediator p_Mediator, ValidationPresenter p_ValidationPresenter)
    {
        m_Logger = p_Logger;
        m_Mediator = p_Mediator;
        m_ValidationPresenter = p_ValidationPresenter;
    }
}