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
    Task NotifyPlayerJoinedAsync(Guid p_PartyId, User p_User);

    /// <summary>
    /// Notify party members that a player left
    /// </summary>
    Task NotifyPlayerLeftAsync(Guid p_PartyId, User p_User);

    /// <summary>
    /// Notify party members that the party was deleted
    /// </summary>
    Task NotifyPartyDeletedAsync(Guid p_PartyId, User p_User);
    Task NotifyPartyUpdatedAsync(Guid p_PartyId, Party p_Party);
}
