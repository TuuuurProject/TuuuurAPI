namespace Tuuuur.Domain.Configuration;

/// <summary>
/// Centralized Redis key patterns for consistent cache access across the application
/// </summary>
public static class RedisKeys
{
    /// <summary>
    /// Party-related keys
    /// </summary>
    public static class Party
    {
        /// <summary>
        /// Key pattern for accessing a party by its unique identifier
        /// Format: Party:{guid}
        /// Example: Party:123e4567-e89b-12d3-a456-426614174000
        /// Value: Party object (JSON serialized)
        /// </summary>
        public static string ById(Guid p_PartyId) => $"Party:{p_PartyId}";

        /// <summary>
        /// Key pattern for accessing a party by its join code
        /// Format: Party:{code}
        /// Example: Party:ABC123
        /// Value: Party object (JSON serialized)
        /// </summary>
        public static string ByCode(string p_Code) => $"Party:{p_Code}";

        /// <summary>
        /// Key pattern for the set of user IDs in a party
        /// Format: Party:{guid}:User
        /// Example: Party:123e4567-e89b-12d3-a456-426614174000:User
        /// Value: Redis SET of user IDs (integers)
        /// </summary>
        public static string Users(Guid p_PartyId) => $"Party:{p_PartyId}:User";
    }

    /// <summary>
    /// User-related keys
    /// </summary>
    public static class User
    {
        /// <summary>
        /// Key pattern for accessing the current party of a user
        /// Format: User:{userId}:Party
        /// Example: User:42:Party
        /// Value: Party GUID
        /// </summary>
        public static string UserParty(int p_UserId) => $"User:{p_UserId}:Party";
    }
}
