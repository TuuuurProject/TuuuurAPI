using System.Net;
using System.Net.Mime;
using System.Text.Json;

namespace Tuuuur.API.Middlewares;

/// <summary>
/// Render an uncatch exception into an HTTP response.
/// </summary>
internal sealed class HandleExceptionMiddleware
{
    private readonly ILogger m_Logger;
    private readonly RequestDelegate m_Next;

    public HandleExceptionMiddleware(RequestDelegate p_Next, ILogger<HandleExceptionMiddleware> p_Logger)
    {
        m_Next = p_Next;
        m_Logger = p_Logger;
    }

    public async Task InvokeAsync(HttpContext p_Context)
    {
        try
        {
            await m_Next(p_Context);
        }
        catch (Exception v_Ex)
        {
            try
            {
                m_Logger.LogError(v_Ex, "An uncatched error occured");

                p_Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                p_Context.Response.ContentType = MediaTypeNames.Application.Json;
                await p_Context.Response.WriteAsync(JsonSerializer.Serialize(v_Ex));
            }
            catch (Exception v_HandleErrorException)
            {
                m_Logger.LogCritical(v_HandleErrorException,
                    "An error occured while trying to print error in HandleExceptionMiddleware");
            }
        }
    }
}