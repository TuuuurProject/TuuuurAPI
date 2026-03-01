using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Domain.Security;

namespace Tuuuur.API.Hubs;

/// <summary>
/// SignalR notification hub for INotificationClient
/// </summary>
public class RankedHub(IMediator p_Mediator) : Hub<IRankedClient>
{
    /// <summary>
    /// Called by the client to join opponent searching
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "SignalR method name")]
    public async Task JoinSearchOpponent()
    {
        try
        {
            if (Context.User != null)
            {
                Guid v_Guid = Guid.TryParse(Context.User.Claims.FirstOrDefault(p_Claim => p_Claim.Type == ClaimNames.Id)
                    ?.Value ?? string.Empty, out Guid v_UserId) ? v_UserId : Guid.Empty;

                // Create party
                JoinSearchOpponentRequest v_Request = new(v_Guid);
                _ = await p_Mediator.Send(v_Request);
            }
        }
        catch (Exception v_Exception)
        {
            await Clients.Caller.OnError(v_Exception.Message);
        }
    }

    /// <summary>
    /// Called by the client to leave opponent searching
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "SignalR method name")]
    public async Task LeaveSearchOpponent()
    {
        try
        {
            if (Context.User != null)
            {
                Guid v_Guid = Guid.TryParse(Context.User.Claims.FirstOrDefault(p_Claim => p_Claim.Type == ClaimNames.Id)
                    ?.Value ?? string.Empty, out Guid v_UserId) ? v_UserId : Guid.Empty;

                // Create party
                LeaveSeachOpponentRequest v_Request = new(v_Guid);
                _ = await p_Mediator.Send(v_Request);
            }
        }
        catch (Exception v_Exception)
        {
            await Clients.Caller.OnError(v_Exception.Message);
        }
    }
    
    /// <summary>
    /// Called by the client to send answer
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "SignalR method name")]
    public async Task SendAnswer(int p_AnswerId)
    {
        try
        {
            if (Context.User != null)
            {
                Guid v_Guid = Guid.TryParse(Context.User.Claims.FirstOrDefault(p_Claim => p_Claim.Type == ClaimNames.Id)
                    ?.Value ?? string.Empty, out Guid v_UserId) ? v_UserId : Guid.Empty;

                // Create party
                AnswerQuestionRankedRequest v_AnswerQuestionGroupPartyRequest = new(p_AnswerId, v_Guid);
                _ = await p_Mediator.Send(v_AnswerQuestionGroupPartyRequest);
            }
        }
        catch (Exception v_Exception)
        {
            await Clients.Caller.OnError(v_Exception.Message);
        }
    }
}