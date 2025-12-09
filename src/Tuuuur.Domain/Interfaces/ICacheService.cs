using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Interfaces;

public interface ICacheService<T> where T : class
{
    Task SetAsync(string p_Key, T p_Value, TimeSpan? p_Expiration = null, CancellationToken p_CancellationToken = default);
    Task<T> GetAsync(string p_Key, CancellationToken p_CancellationToken = default);
    Task<bool> RemoveAsync(string p_Key, CancellationToken p_CancellationToken = default);
    Task<bool> ExistsAsync(string p_Key, CancellationToken p_CancellationToken = default);
    Task HashSetAsync(string p_MasterKey, string p_FieldKey, T p_Value, CancellationToken p_CancellationToken = default);
    Task<T> HashGetAsync(string p_MasterKey, string p_FieldKey, CancellationToken p_CancellationToken = default);
    Task<Dictionary<string, T>> HashGetAllAsync(string p_MasterKey, CancellationToken p_CancellationToken = default);
    Task<bool> HashDeleteAsync(string p_MasterKey, string p_FieldKey, CancellationToken p_CancellationToken = default);
    Task<long> ListRightPushAsync(string p_Key, T p_Value, CancellationToken p_CancellationToken = default);
    Task<T> ListLeftPopAsync(string p_Key, CancellationToken p_CancellationToken = default);
    Task<List<T>> ListRangeAsync(string p_Key, CancellationToken p_CancellationToken = default);
    Task<bool> SetAddAsync(string p_Key, T p_Value, CancellationToken p_CancellationToken = default);
    Task<bool> SetRemoveAsync(string p_Key, T p_Value, CancellationToken p_CancellationToken = default);
    Task<List<T>> SetMembersAsync(string p_Key, CancellationToken p_CancellationToken = default);
    Task<bool> SetContainsAsync(string p_Key, T p_Value, CancellationToken p_CancellationToken = default);
    Task<bool> SortedSetAddAsync(string p_Key, T p_Value, double p_Score, CancellationToken p_CancellationToken = default);
    Task<List<T>> SortedSetRangeByRankAsync(string p_Key, long p_Start = 0, long p_Stop = -1, bool p_Descending = false, CancellationToken p_CancellationToken = default);
    Task<bool> SortedSetRemoveAsync(string p_Key, T p_Value, CancellationToken p_CancellationToken = default);
    Task<string> StreamAddAsync(string p_Key, T p_Value, CancellationToken p_CancellationToken = default);
    Task<List<T>> StreamReadAsync(string p_Key, int p_Count = 10, CancellationToken p_CancellationToken = default);
}