using Tuuuur.Domain.Bo;

namespace Tuuuur.API.Hubs;

/// <summary>
/// Client-side methods that can be invoked from the server
/// </summary>
public interface IGroupClient
{
    /// <summary>
    /// Notified when a player joins the party
    /// </summary>
    Task OnPlayerJoined(User p_User);

    /// <summary>
    /// Notified when a player leaves the party
    /// </summary>
    Task OnPlayerLeft(User p_User);

    /// <summary>
    /// Notified when the party is deleted
    /// </summary>
    /// <param name="p_User"></param>
    /// <returns></returns>
    Task OnPartyDeleted(User p_User);
    
    /// <summary>
    /// Notified when the party is updated
    /// </summary>
    /// <param name="p_User"></param>
    /// <returns></returns>
    Task OnPartyUpdated(Party p_User);

    /// <summary>
    /// Notified when an error occurs
    /// </summary>
    Task OnError(string p_ErrorMessage);
}
