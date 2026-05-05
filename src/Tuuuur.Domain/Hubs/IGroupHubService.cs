namespace Tuuuur.Domain.Hubs;

/// <summary>
/// Service interface for GroupHub operations
/// Handles business logic for SignalR group operations
/// </summary>
public interface IGroupHubService
{
    /// <summary>
    /// Handle a player joining a party
    /// </summary>
    /// <param name="p_PartyCode">The party code to join</param>
    /// <param name="p_UserId">The user ID joining</param>
    Task JoinPartyAsync(string p_PartyCode, int p_UserId);

    /// <summary>
    /// Handle a player leaving a party
    /// </summary>
    /// <param name="p_UserId">The user ID leaving</param>
    Task LeavePartyAsync(int p_UserId);
}
