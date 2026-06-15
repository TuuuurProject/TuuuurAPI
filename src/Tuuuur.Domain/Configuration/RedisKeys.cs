namespace Tuuuur.Domain.Configuration;
/// <summary>
/// Centralized Redis key patterns for consistent cache access across the application
/// </summary>
public static class RedisKeys
{
    /// <summary>
    /// Group-related keys
    /// </summary>
    public static class Group
    {
        /// <summary>
        /// Key pattern for accessing a Group by its join code
        /// Format: Group:{code}
        /// Example: Group:ABC123
        /// Value: Group object (JSON serialized)
        /// </summary>
        public static string ByCode(string p_Code) => $"Group:{p_Code}";
        /// <summary>
        /// Key pattern for the set of user IDs in a Group
        /// Format: Group:{string}:User
        /// Example: Group:123456:User
        /// Value: Redis SET of user IDs (integers)
        /// </summary>
        public static string Users(string p_Code) => $"Group:{p_Code}:User";
        /// <summary>
        /// Key pattern for the set of questions in a Group
        /// Format: Group:{string}:GroupQuestions
        /// Example: Group:123456:GroupQuestions
        /// Value: Redis SET of questions IDs (integers)
        /// </summary>
        public static string Questions(string p_Code) => $"Group:{p_Code}:Questions";
        /// <summary>
        /// Key pattern for the current question index in a Group
        /// Format: Group:{string}:GroupCurrentQuestionIndex
        /// Example: Group:123456:GroupCurrentQuestionIndex
        /// Value: Integer representing the current question index (0-based)
        /// </summary>
        public static string CurrentQuestionIndex(string p_Code) => $"Group:{p_Code}:CurrentQuestionIndex";
        /// <summary>
        /// Key pattern for the answering question in a Group
        /// </summary>
        /// <param name="p_Code"></param>
        /// <param name="p_QuestionId"></param>
        /// <param name="p_User"></param>
        /// <returns></returns>
        public static string QuestionUserAnswer(string p_Code, int p_QuestionId, Guid p_User) => $"Group:{p_Code}:Questions:{p_QuestionId}:Users:{p_User}:Answer";
        /// <summary>
        /// Key pattern for the sorted set of player scores in a Group
        /// Format: Group:{string}:Scores
        /// Example: Group:123456:Scores
        /// Value: Redis SORTED SET of User objects with scores as ranking values
        /// </summary>
        public static string Scores(string p_Code) => $"Group:{p_Code}:Scores";
        /// <summary>
        /// Key pattern for the set of users who have answered a specific question
        /// Format: Group:{string}:GroupQuestions:{int}:Answered
        /// Example: Group:ABC123:GroupQuestions:42:Answered
        /// Value: Redis SET of user IDs (integers) who have submitted an answer
        /// </summary>
        public static string QuestionAnswered(string p_Code, int p_QuestionId) => $"Group:{p_Code}:Questions:{p_QuestionId}:Answered";
        /// <summary>
        /// Pub/Sub channel for notifying when all players have answered a question
        /// Format: Group:{string}:GroupQuestions:{int}:AllAnswered
        /// Example: Group:ABC123:GroupQuestions:42:AllAnswered
        /// </summary>
        public static string QuestionAllAnsweredChannel(string p_Code, int p_QuestionId) => $"Group:{p_Code}:Questions:{p_QuestionId}:AllAnswered";
    }
    /// <summary>
    /// Ranked-related keys
    /// </summary>
    public static class Ranked
    {
        /// <summary>
        /// Sorted set of players waiting for a match.
        /// Format: Ranked:Matchmaking
        /// Value: Sorted set of User objects (JSON serialized), score = GlobalElo
        /// </summary>
        public static string MatchmakingList() => "Ranked:Matchmaking";
        /// <summary>
        /// Distributed lock key used to elect one matchmaking leader across all API replicas.
        /// Format: Ranked:Matchmaking:Lock
        /// Value: Instance ID (string) of the current lock owner
        /// </summary>
        public static string MatchmakingLock() => "Ranked:Matchmaking:Lock";
        /// <summary>
        /// Hash that stores the UTC timestamp (ticks) when each player joined the matchmaking queue.
        /// Format: Ranked:Matchmaking:JoinedAt
        /// Field: userId (string), Value: DateTime.UtcNow.Ticks (string)
        /// Used to progressively widen the Elo tolerance the longer a player waits.
        /// </summary>
        public static string MatchmakingJoinedAt() => "Ranked:Matchmaking:JoinedAt";
        public static string ById(Guid p_Id) => $"Ranked:{p_Id}";
        /// <summary>
        /// Key pattern for the current question index in a Group
        /// Format: Ranked:{string}:CurrentQuestionIndex
        /// Example: Ranked:123456:CurrentQuestionIndex
        /// Value: Integer representing the current question index (0-based)
        /// </summary>
        public static string CurrentQuestionIndex(Guid p_Id) => $"Ranked:{p_Id}:CurrentQuestionIndex";
        /// <summary>
        /// Key pattern for the sorted set of player scores in Ranked
        /// Format: Ranked:{string}:Scores
        /// Example: Ranked:123456:Scores
        /// Value: Redis SORTED SET of User objects with scores as ranking values
        /// </summary>
        public static string Scores(Guid p_Id) => $"Ranked:{p_Id}:Scores";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_Id"></param>
        /// <returns></returns>
        public static string Questions(Guid p_Id) => $"Ranked:{p_Id}:Questions";
        /// <summary>
        /// Key pattern for the answering question in a Group
        /// </summary>
        /// <param name="p_Id"></param>
        /// <param name="p_QuestionId"></param>
        /// <param name="p_User"></param>
        /// <returns></returns>
        public static string QuestionUserAnswer(Guid p_Id, int p_QuestionId, Guid p_User) => $"Ranked:{p_Id}:Questions:{p_QuestionId}:Users:{p_User}:Answer";
        /// <summary>
        /// Key pattern for the set of users who have answered a specific question
        /// Format: Group:{string}:GroupQuestions:{int}:Answered
        /// Example: Group:ABC123:GroupQuestions:42:Answered
        /// Value: Redis SET of user IDs (integers) who have submitted an answer
        /// </summary>
        public static string PartyQuestionAnswered(Guid p_Id, int p_QuestionId) => $"Ranked:{p_Id}:Questions:{p_QuestionId}:Answered";
        /// <summary>
        /// Pub/Sub channel for notifying when all players have answered a question
        /// Format: Group:{string}:GroupQuestions:{int}:AllAnswered
        /// Example: Group:ABC123:GroupQuestions:42:AllAnswered
        /// </summary>
        public static string PartyQuestionAllAnsweredChannel(Guid p_Id, int p_QuestionId) => $"Ranked:{p_Id}:Questions:{p_QuestionId}:AllAnswered";
        
        /// <summary>
        /// Key pattern for save user forfeit
        /// </summary>
        /// <param name="p_PartyId"></param>
        /// <returns></returns>
        public static string PlayerForfeited(Guid p_PartyId) => $"Ranked:{p_PartyId}:PlayerForfeited";
    }
    /// <summary>
    /// User-related keys
    /// </summary>
    public static class User
    {
        /// <summary>
        /// Key pattern for accessing a group party by its join code
        /// Format: Group:{code}
        /// Example: Group:ABC123
        /// Value: Group object (JSON serialized)
        /// </summary>
        public static string GroupById(Guid p_Id) => $"User:{p_Id}";
        /// <summary>
        /// Key pattern for accessing the current Group party of a user
        /// Format: User:{userId}:Group
        /// Example: User:42:Group
        /// Value: Group GUID
        /// </summary>
        public static string UserGroup(Guid p_UserId) => $"User:{p_UserId}:Group";
        /// <summary>
        /// Key pattern for accessing the current ranked party of a user
        /// Format: User:{userId}:Ranked
        /// Example: User:42:Ranked
        /// Value: Group GUID
        /// </summary>
        public static string UserRanked(Guid p_UserId) => $"User:{p_UserId}:Ranked";
        /// <summary>
        /// Key pattern for accessing the connection status of a user
        /// Format: User:{userId}:Connected
        /// Example: User:42:Connected
        /// Value: Boolean
        /// </summary>
        public static string UserConnected(Guid p_UserId) => $"User:{p_UserId}:Connected";
    }
}
