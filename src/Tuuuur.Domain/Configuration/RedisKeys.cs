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
        /// Key pattern for accessing a party by its join code
        /// Format: Party:{code}
        /// Example: Party:ABC123
        /// Value: Party object (JSON serialized)
        /// </summary>
        public static string ByCode(string p_Code) => $"Party:{p_Code}";

        /// <summary>
        /// Key pattern for the set of user IDs in a party
        /// Format: Party:{string}:User
        /// Example: Party:123456:User
        /// Value: Redis SET of user IDs (integers)
        /// </summary>
        public static string Users(string p_Code) => $"Party:{p_Code}:User";

        /// <summary>
        /// Key pattern for the set of questions in a party
        /// Format: Party:{string}:Questions
        /// Example: Party:123456:Questions
        /// Value: Redis SET of questions IDs (integers)
        /// </summary>
        public static string Questions(string p_Code) => $"Party:{p_Code}:Questions";

        /// <summary>
        /// Key pattern for the current question index in a party
        /// Format: Party:{string}:CurrentQuestionIndex
        /// Example: Party:123456:CurrentQuestionIndex
        /// Value: Integer representing the current question index (0-based)
        /// </summary>
        public static string CurrentQuestionIndex(string p_Code) => $"Party:{p_Code}:CurrentQuestionIndex";

        /// <summary>
        /// Key pattern for the answering question in a party
        /// </summary>
        /// <param name="p_Code"></param>
        /// <param name="p_QuestionId"></param>
        /// <param name="p_User"></param>
        /// <returns></returns>
        public static string PartyQuestionUserAnswer(string p_Code, int p_QuestionId, int p_User) => $"Party:{p_Code}:Questions:{p_QuestionId}:Users:{p_User}:Answer";

        /// <summary>
        /// Key pattern for the sorted set of player scores in a party
        /// Format: Party:{string}:Scores
        /// Example: Party:123456:Scores
        /// Value: Redis SORTED SET of User objects with scores as ranking values
        /// </summary>
        public static string Scores(string p_Code) => $"Party:{p_Code}:Scores";
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
