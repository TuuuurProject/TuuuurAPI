namespace Tuuuur.Domain.Interfaces;

public interface ICacheService
{
    string CreateKey(params string[] p_Parts);

    Task SetAsync<T>(string p_Key, T p_Value, TimeSpan p_Expiration = default, CancellationToken p_CancellationToken = default);
    Task<T> GetAsync<T>(string p_Key, CancellationToken p_CancellationToken = default);
    Task<bool> RemoveAsync(string p_Key, CancellationToken p_CancellationToken = default);
    Task<bool> ExistsAsync(string p_Key, CancellationToken p_CancellationToken = default);
    Task<long> IncrementAsync(string p_Key, long p_Value = 1, CancellationToken p_CancellationToken = default);

    Task HashSetAsync<T>(string p_MasterKey, string p_FieldKey, T p_Value, CancellationToken p_CancellationToken = default);
    Task<T> HashGetAsync<T>(string p_MasterKey, string p_FieldKey, CancellationToken p_CancellationToken = default);
    Task<Dictionary<string, T>> HashGetAllAsync<T>(string p_MasterKey, CancellationToken p_CancellationToken = default);
    Task<bool> HashDeleteAsync(string p_MasterKey, string p_FieldKey, CancellationToken p_CancellationToken = default);

    Task<long> ListRightPushAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default);
    Task<T> ListLeftPopAsync<T>(string p_Key, CancellationToken p_CancellationToken = default);
    Task<List<T>> ListRangeAsync<T>(string p_Key, CancellationToken p_CancellationToken = default);

    Task<bool> SetAddAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default);
    Task<long> SetAddRangeAsync<T>(string p_Key, IEnumerable<T> p_Values, CancellationToken p_CancellationToken = default);
    Task<bool> SetRemoveAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default);
    Task<List<T>> SetMembersAsync<T>(string p_Key, CancellationToken p_CancellationToken = default);
    Task<bool> SetContainsAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default);

    Task<bool> SortedSetAddAsync<T>(string p_Key, T p_Value, int p_Score, CancellationToken p_CancellationToken = default);
    Task<List<T>> SortedSetRangeByRankAsync<T>(string p_Key, long p_Start = 0, long p_Stop = -1, bool p_Descending = false, CancellationToken p_CancellationToken = default);
    Task<List<(T Value, int Score)>> SortedSetRangeByRankWithScoresAsync<T>(string p_Key, long p_Start = 0, long p_Stop = -1, bool p_Descending = false, CancellationToken p_CancellationToken = default);
    Task<T> SortedSetGetByIndexAsync<T>(string p_Key, long p_Index, CancellationToken p_CancellationToken = default);
    Task<bool> SortedSetRemoveAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default);
    Task<List<(T Value, int Score)>> SortedSetGetAllWithScoresAsync<T>(string p_Key, bool p_Descending = false, CancellationToken p_CancellationToken = default);

    Task<string> StreamAddAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default);
    Task<List<T>> StreamReadAsync<T>(string p_Key, int p_Count = 10, CancellationToken p_CancellationToken = default);

    Task RemoveByPatternAsync(string p_Pattern, IEnumerable<string> p_KeysToKeep = null, CancellationToken p_CancellationToken = default);
}