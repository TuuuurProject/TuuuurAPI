using System.Text.Json;
using StackExchange.Redis;
using Tuuuur.Domain.Interfaces;

namespace Tuuuur.Infrastructure.Cache.Services;

public class CacheService<T> : ICacheService<T> where T : class
{
    private readonly IDatabase m_Database;
    private readonly string m_KeyPrefix;

    public CacheService(IConnectionMultiplexer p_Redis)
    {
        m_Database = p_Redis.GetDatabase();
        m_KeyPrefix = typeof(T).Name;
    }

    private string BuildKey(string p_Key) => $"{m_KeyPrefix}:{p_Key}";

    public async Task SetAsync(string p_Key, T p_Value, TimeSpan? p_Expiration = null, CancellationToken p_CancellationToken = default)
    {
        if (p_CancellationToken.IsCancellationRequested) return;
        string v_Key = BuildKey(p_Key);
        string v_Json = JsonSerializer.Serialize(p_Value);
        
        if (p_Expiration.HasValue)
            await m_Database.StringSetAsync(v_Key, v_Json, p_Expiration.Value);
        else
            await m_Database.StringSetAsync(v_Key, v_Json);
    }

    public async Task<T> GetAsync(string p_Key, CancellationToken p_CancellationToken = default)
    {
        if (p_CancellationToken.IsCancellationRequested) return default;
        RedisValue v_Value = await m_Database.StringGetAsync(BuildKey(p_Key));
        return v_Value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(v_Value!);
    }

    public async Task<bool> RemoveAsync(string p_Key, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.KeyDeleteAsync(BuildKey(p_Key));
    }
    
    public async Task<bool> ExistsAsync(string p_Key, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.KeyExistsAsync(BuildKey(p_Key));
    }
    
    public async Task HashSetAsync(string p_MasterKey, string p_FieldKey, T p_Value, CancellationToken p_CancellationToken = default)
    {
        if (p_CancellationToken.IsCancellationRequested) return;
        await m_Database.HashSetAsync(BuildKey(p_MasterKey), p_FieldKey, JsonSerializer.Serialize(p_Value));
    }

    public async Task<T> HashGetAsync(string p_MasterKey, string p_FieldKey, CancellationToken p_CancellationToken = default)
    {
        RedisValue v_Value = await m_Database.HashGetAsync(BuildKey(p_MasterKey), p_FieldKey);
        return v_Value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(v_Value!);
    }

    public async Task<Dictionary<string, T>> HashGetAllAsync(string p_MasterKey, CancellationToken p_CancellationToken = default)
    {
        HashEntry[] v_Entries = await m_Database.HashGetAllAsync(BuildKey(p_MasterKey));
        Dictionary<string, T> v_Result = new();
        foreach (HashEntry v_Entry in v_Entries)
        {
            if (v_Entry.Value.HasValue)
                v_Result[v_Entry.Name!] = JsonSerializer.Deserialize<T>(v_Entry.Value!)!;
        }
        return v_Result;
    }

    public async Task<bool> HashDeleteAsync(string p_MasterKey, string p_FieldKey, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.HashDeleteAsync(BuildKey(p_MasterKey), p_FieldKey);
    }
    
    public async Task<long> ListRightPushAsync(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.ListRightPushAsync(BuildKey(p_Key), JsonSerializer.Serialize(p_Value));
    }

    public async Task<T> ListLeftPopAsync(string p_Key, CancellationToken p_CancellationToken = default)
    {
        RedisValue v_Value = await m_Database.ListLeftPopAsync(BuildKey(p_Key));
        return v_Value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(v_Value!);
    }

    public async Task<List<T>> ListRangeAsync(string p_Key, CancellationToken p_CancellationToken = default)
    {
        RedisValue[] v_Values = await m_Database.ListRangeAsync(BuildKey(p_Key));
        return v_Values.Select(p_Value => JsonSerializer.Deserialize<T>(p_Value.ToString())!).ToList();
    }
    
    public async Task<bool> SetAddAsync(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.SetAddAsync(BuildKey(p_Key), JsonSerializer.Serialize(p_Value));
    }

    public async Task<bool> SetRemoveAsync(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.SetRemoveAsync(BuildKey(p_Key), JsonSerializer.Serialize(p_Value));
    }

    public async Task<List<T>> SetMembersAsync(string p_Key, CancellationToken p_CancellationToken = default)
    {
        RedisValue[] v_Members = await m_Database.SetMembersAsync(BuildKey(p_Key));
        return v_Members.Select(p_Value => JsonSerializer.Deserialize<T>(p_Value.ToString())!).ToList();
    }

    public async Task<bool> SetContainsAsync(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.SetContainsAsync(BuildKey(p_Key), JsonSerializer.Serialize(p_Value));
    }
    
    public async Task<bool> SortedSetAddAsync(string p_Key, T p_Value, double p_Score, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.SortedSetAddAsync(BuildKey(p_Key), JsonSerializer.Serialize(p_Value), p_Score);
    }

    public async Task<List<T>> SortedSetRangeByRankAsync(string p_Key, long p_Start = 0, long p_Stop = -1, bool p_Descending = false, CancellationToken p_CancellationToken = default)
    {
        Order v_Order = p_Descending ? Order.Descending : Order.Ascending;
        RedisValue[] v_Values = await m_Database.SortedSetRangeByRankAsync(BuildKey(p_Key), p_Start, p_Stop, v_Order);
        return v_Values.Select(p_Value => JsonSerializer.Deserialize<T>(p_Value.ToString())!).ToList();
    }
    
    public async Task<bool> SortedSetRemoveAsync(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
         return await m_Database.SortedSetRemoveAsync(BuildKey(p_Key), JsonSerializer.Serialize(p_Value));
    }
    
    public async Task<string> StreamAddAsync(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
        NameValueEntry[] v_Entry =
        [
            new NameValueEntry("payload", JsonSerializer.Serialize(p_Value))
        ];
        
        RedisValue v_Id = await m_Database.StreamAddAsync(BuildKey(p_Key), v_Entry);
        return v_Id.ToString();
    }

    public async Task<List<T>> StreamReadAsync(string p_Key, int p_Count = 10, CancellationToken p_CancellationToken = default)
    {
        StreamEntry[] v_Streams = await m_Database.StreamReadAsync(BuildKey(p_Key), "0-0", count: p_Count);
        List<T> v_Result = [];

        foreach (StreamEntry v_StreamEntry in v_Streams)
        {
            NameValueEntry v_Payload = v_StreamEntry.Values.FirstOrDefault(x => x.Name == "payload");
            if (!v_Payload.Value.IsNullOrEmpty)
            {
                v_Result.Add(JsonSerializer.Deserialize<T>(v_Payload.Value!)!);
            }
        }
        return v_Result;
    }
}