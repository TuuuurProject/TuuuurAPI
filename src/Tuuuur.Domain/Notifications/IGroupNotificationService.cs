using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Notifications;

/// <summary>
/// Service for group party hub notifications
/// </summary>
public interface IGroupNotificationService
{
    /// <summary>
    /// Notify party members that a player joined
    /// </summary>
    Task NotifyPlayerJoinedAsync(string p_PartyId, User p_User);

    /// <summary>
    /// Notify party members that a player left
    /// </summary>
    Task NotifyPlayerLeftAsync(string p_PartyId, User p_User);

    /// <summary>
    /// Notify party members that the party was deleted
    /// </summary>
    Task NotifyPartyDeletedAsync(string p_PartyId, User p_User);
    Task NotifyPartyUpdatedAsync(string p_PartyId, Party p_Party);
}
