using MediatR;
using Microsoft.AspNetCore.SignalR;
using Tuuuur.Core.Requests.Group;

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
            // Create party
            StartGroupPartyRequest v_StartGroupPartyRequest = new();
            _ = await p_Mediator.Send(v_StartGroupPartyRequest);
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
            // Create party
            AnswerQuestionGroupPartyRequest v_AnswerQuestionGroupPartyRequest = new(p_AnswerId);
            _ = await p_Mediator.Send(v_AnswerQuestionGroupPartyRequest);
        }
        catch (Exception v_Exception)
        {
            await Clients.Caller.OnError(v_Exception.Message);
        }
    }
}