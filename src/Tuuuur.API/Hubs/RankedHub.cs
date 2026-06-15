using MediatR;
using Microsoft.AspNetCore.SignalR;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Security;
namespace Tuuuur.API.Hubs;
/// <summary>
/// SignalR notification hub for INotificationClient
/// </summary>
public class RankedHub(IMediator p_Mediator, ICacheService p_CacheService) : Hub<IRankedClient>
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
    
    
    /// <summary>
    /// Called by the client to give up ranked party
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "SignalR method name")]
    public async Task GiveUp()
    {
        try
        {
            if (Context.User != null)
            {
                Guid v_Guid = Guid.TryParse(Context.User.Claims.FirstOrDefault(p_Claim => p_Claim.Type == ClaimNames.Id)
                    ?.Value ?? string.Empty, out Guid v_UserId) ? v_UserId : Guid.Empty;
                // Create party
                GiveUpRankedRequest v_Request = new(v_Guid);
                _ = await p_Mediator.Send(v_Request);
            }
        }
        catch (Exception v_Exception)
        {
            await Clients.Caller.OnError(v_Exception.Message);
        }
    }
    /// <summary>
    /// Called when a new connection is established with the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        if (Context.User != null)
        {
            Guid v_Guid = Guid.TryParse(Context.User.Claims.FirstOrDefault(p_Claim => p_Claim.Type == ClaimNames.Id)
                ?.Value ?? string.Empty, out Guid v_UserId) ? v_UserId : Guid.Empty;
            if (v_Guid != Guid.Empty)
            {
                await p_CacheService.SetAsync(RedisKeys.User.UserConnected(v_Guid), true);
            }
        }
        await base.OnConnectedAsync();
    }
    /// <summary>
    /// Called when a connection with the hub is terminated.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception p_Exception)
    {
        if (Context.User != null)
        {
            Guid v_Guid = Guid.TryParse(Context.User.Claims.FirstOrDefault(p_Claim => p_Claim.Type == ClaimNames.Id)
                ?.Value ?? string.Empty, out Guid v_UserId) ? v_UserId : Guid.Empty;
            if (v_Guid != Guid.Empty)
            {
                await p_CacheService.SetAsync(RedisKeys.User.UserConnected(v_Guid), false);
            }
        }
        await base.OnDisconnectedAsync(p_Exception);
    }
}