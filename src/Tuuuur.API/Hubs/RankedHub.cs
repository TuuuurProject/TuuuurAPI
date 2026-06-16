using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Security;
namespace Tuuuur.API.Hubs;
/// <summary>
/// SignalR notification hub for INotificationClient
/// </summary>
public class RankedHub(IMediator p_Mediator, ICacheService p_CacheService, ILogger<RankedHub> p_Logger) : Hub<IRankedClient>
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
    /// Automatically triggers a forfeit if the disconnected user was in an active ranked party.
    /// This covers browser closes, tab crashes, and network loss — anything that drops the WebSocket.
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

                // If the user was in an active ranked party, trigger forfeit automatically
                string v_RankedPartyId = await p_CacheService.GetAsync<string>(RedisKeys.User.UserRanked(v_Guid));
                if (!string.IsNullOrEmpty(v_RankedPartyId) && Guid.TryParse(v_RankedPartyId, out Guid v_PartyId))
                {
                    Party v_Party = await p_CacheService.GetAsync<Party>(RedisKeys.Ranked.ById(v_PartyId));
                    if (v_Party is { InProgress: true })
                    {
                        try
                        {
                            GiveUpRankedRequest v_GiveUpRequest = new(v_Guid);
                            _ = await p_Mediator.Send(v_GiveUpRequest);
                        }
                        catch (Exception v_Ex)
                        {
                            // Log but don't rethrow — disconnection handler must not throw
                            p_Logger.LogError(v_Ex, "[RankedHub] Auto-forfeit failed for user {UserId}", v_Guid);
                        }
                    }
                }
            }
        }
        await base.OnDisconnectedAsync(p_Exception);
    }
}