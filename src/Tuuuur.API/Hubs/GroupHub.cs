using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Domain.Security;

namespace Tuuuur.API.Hubs;

/// <summary>
/// SignalR notification hub for INotificationClient
/// </summary>
public class GroupHub(IMediator p_Mediator) : Hub<IGroupClient>
{
    /// <summary>
    /// Called by the client to start a party
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "SignalR method name")]
    public async Task StartGroupParty()
    {
        try
        {
            if (Context.User != null)
            {
                string v_UserEmail = Context.User.Claims.FirstOrDefault(p_C => p_C.Type == ClaimTypes.Email)?.Value;

                // Create party
                StartGroupPartyRequest v_StartGroupPartyRequest = new(v_UserEmail);
                _ = await p_Mediator.Send(v_StartGroupPartyRequest);
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
                AnswerQuestionGroupPartyRequest v_AnswerQuestionGroupPartyRequest = new(p_AnswerId, v_Guid);
                _ = await p_Mediator.Send(v_AnswerQuestionGroupPartyRequest);
            }
        }
        catch (Exception v_Exception)
        {
            await Clients.Caller.OnError(v_Exception.Message);
        }
    }
}