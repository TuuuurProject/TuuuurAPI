using System.Text.Json;
using StackExchange.Redis;
using Tuuuur.Domain.Interfaces;

namespace Tuuuur.Infrastructure.Services;

public class CacheService(IConnectionMultiplexer p_Redis) : ICacheService
{
    private readonly IDatabase m_Database = p_Redis.GetDatabase();

    public string CreateKey(params string[] p_Parts) => string.Join(":", p_Parts);

    public async Task SetAsync<T>(string p_Key, T p_Value, TimeSpan p_Expiration = default, CancellationToken p_CancellationToken = default)
    {
        if (p_CancellationToken.IsCancellationRequested) return;
        string v_Json = JsonSerializer.Serialize(p_Value);
        
        if (p_Expiration != TimeSpan.Zero)
            await m_Database.StringSetAsync(p_Key, v_Json, p_Expiration);
        else
            await m_Database.StringSetAsync(p_Key, v_Json);
    }

    public async Task<T> GetAsync<T>(string p_Key, CancellationToken p_CancellationToken = default)
    {
        if (p_CancellationToken.IsCancellationRequested) return default;
        RedisValue v_Value = await m_Database.StringGetAsync(p_Key);
        return v_Value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(v_Value!);
    }

    public async Task<bool> RemoveAsync(string p_Key, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.KeyDeleteAsync(p_Key);
    }
    
    public async Task<bool> ExistsAsync(string p_Key, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.KeyExistsAsync(p_Key);
    }

    public async Task HashSetAsync<T>(string p_MasterKey, string p_FieldKey, T p_Value, CancellationToken p_CancellationToken = default)
    {
        if (p_CancellationToken.IsCancellationRequested) return;
        await m_Database.HashSetAsync(p_MasterKey, p_FieldKey, JsonSerializer.Serialize(p_Value));
    }

    public async Task<T> HashGetAsync<T>(string p_MasterKey, string p_FieldKey, CancellationToken p_CancellationToken = default)
    {
        RedisValue v_Value = await m_Database.HashGetAsync(p_MasterKey, p_FieldKey);
        return v_Value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(v_Value!);
    }

    public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string p_MasterKey, CancellationToken p_CancellationToken = default)
    {
        HashEntry[] v_Entries = await m_Database.HashGetAllAsync(p_MasterKey);
        Dictionary<string, T> v_Result = [];
        foreach (HashEntry v_Entry in v_Entries)
        {
            if (v_Entry.Value.HasValue)
                v_Result[v_Entry.Name!] = JsonSerializer.Deserialize<T>(v_Entry.Value!)!;
        }
        return v_Result;
    }

    public async Task<bool> HashDeleteAsync(string p_MasterKey, string p_FieldKey, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.HashDeleteAsync(p_MasterKey, p_FieldKey);
    }

    public async Task<long> ListRightPushAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.ListRightPushAsync(p_Key, JsonSerializer.Serialize(p_Value));
    }

    public async Task<T> ListLeftPopAsync<T>(string p_Key, CancellationToken p_CancellationToken = default)
    {
        RedisValue v_Value = await m_Database.ListLeftPopAsync(p_Key);
        return v_Value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(v_Value!);
    }

    public async Task<List<T>> ListRangeAsync<T>(string p_Key, CancellationToken p_CancellationToken = default)
    {
        RedisValue[] v_Values = await m_Database.ListRangeAsync(p_Key);
        return v_Values.Select(v => JsonSerializer.Deserialize<T>(v.ToString())!).ToList();
    }

    public async Task<bool> SetAddAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.SetAddAsync(p_Key, JsonSerializer.Serialize(p_Value));
    }

    public async Task<bool> SetRemoveAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.SetRemoveAsync(p_Key, JsonSerializer.Serialize(p_Value));
    }

    public async Task<List<T>> SetMembersAsync<T>(string p_Key, CancellationToken p_CancellationToken = default)
    {
        RedisValue[] v_Members = await m_Database.SetMembersAsync(p_Key);
        return v_Members.Select(v => JsonSerializer.Deserialize<T>(v.ToString())!).ToList();
    }

    public async Task<bool> SetContainsAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.SetContainsAsync(p_Key, JsonSerializer.Serialize(p_Value));
    }

    public async Task<bool> SortedSetAddAsync<T>(string p_Key, T p_Value, double p_Score, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.SortedSetAddAsync(p_Key, JsonSerializer.Serialize(p_Value), p_Score);
    }

    public async Task<List<T>> SortedSetRangeByRankAsync<T>(string p_Key, long p_Start = 0, long p_Stop = -1, bool p_Descending = false, CancellationToken p_CancellationToken = default)
    {
        Order v_Order = p_Descending ? Order.Descending : Order.Ascending;
        RedisValue[] v_Values = await m_Database.SortedSetRangeByRankAsync(p_Key, p_Start, p_Stop, v_Order);
        return v_Values.Select(p_Value => JsonSerializer.Deserialize<T>(p_Value.ToString())!).ToList();
    }

    public async Task<bool> SortedSetRemoveAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.SortedSetRemoveAsync(p_Key, JsonSerializer.Serialize(p_Value));
    }

    public async Task<string> StreamAddAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
        NameValueEntry[] v_Entry = [ new NameValueEntry("payload", JsonSerializer.Serialize(p_Value)) ];
        RedisValue v_Id = await m_Database.StreamAddAsync(p_Key, v_Entry);
        return v_Id.ToString();
    }

    public async Task<List<T>> StreamReadAsync<T>(string p_Key, int p_Count = 10, CancellationToken p_CancellationToken = default)
    {
        StreamEntry[] v_Streams = await m_Database.StreamReadAsync(p_Key, "0-0", count: p_Count);
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