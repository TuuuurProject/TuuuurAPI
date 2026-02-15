using System.Net;
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

        TimeSpan v_Expiration = p_Expiration == TimeSpan.Zero ? TimeSpan.FromHours(24) : p_Expiration;
        await m_Database.StringSetAsync(p_Key, v_Json, v_Expiration);
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

    public async Task<long> IncrementAsync(string p_Key, long p_Value = 1, CancellationToken p_CancellationToken = default)
    {
        if (p_CancellationToken.IsCancellationRequested) return 0;
        return await m_Database.StringIncrementAsync(p_Key, p_Value);
    }

    public async Task HashSetAsync<T>(string p_MasterKey, string p_FieldKey, T p_Value, TimeSpan p_Expiration = default, CancellationToken p_CancellationToken = default)
    {
        if (p_CancellationToken.IsCancellationRequested) return;
        await m_Database.HashSetAsync(p_MasterKey, p_FieldKey, JsonSerializer.Serialize(p_Value));
        TimeSpan v_Expiration = p_Expiration == TimeSpan.Zero ? TimeSpan.FromHours(24) : p_Expiration;
        await m_Database.KeyExpireAsync(p_MasterKey, v_Expiration);
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

    public async Task<long> ListRightPushAsync<T>(string p_Key, T p_Value, TimeSpan p_Expiration = default, CancellationToken p_CancellationToken = default)
    {
        long v_Result = await m_Database.ListRightPushAsync(p_Key, JsonSerializer.Serialize(p_Value));
        TimeSpan v_Expiration = p_Expiration == TimeSpan.Zero ? TimeSpan.FromHours(24) : p_Expiration;
        await m_Database.KeyExpireAsync(p_Key, v_Expiration);
        return v_Result;
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

    public async Task<bool> SetAddAsync<T>(string p_Key, T p_Value, TimeSpan p_Expiration = default, CancellationToken p_CancellationToken = default)
    {
        bool v_Result = await m_Database.SetAddAsync(p_Key, JsonSerializer.Serialize(p_Value));
        TimeSpan v_Expiration = p_Expiration == TimeSpan.Zero ? TimeSpan.FromHours(24) : p_Expiration;
        await m_Database.KeyExpireAsync(p_Key, v_Expiration);
        return v_Result;
    }

    public async Task<long> SetAddRangeAsync<T>(string p_Key, IEnumerable<T> p_Values, TimeSpan p_Expiration = default, CancellationToken p_CancellationToken = default)
    {
        if (p_CancellationToken.IsCancellationRequested) return 0;
        RedisValue[] v_RedisValues = p_Values.Select(p_Value => (RedisValue)JsonSerializer.Serialize(p_Value)).ToArray();
        long v_Result = await m_Database.SetAddAsync(p_Key, v_RedisValues);
        TimeSpan v_Expiration = p_Expiration == TimeSpan.Zero ? TimeSpan.FromHours(24) : p_Expiration;
        await m_Database.KeyExpireAsync(p_Key, v_Expiration);
        return v_Result;
    }

    public async Task<bool> SetRemoveAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.SetRemoveAsync(p_Key, JsonSerializer.Serialize(p_Value));
    }

    public async Task<List<T>> SetMembersAsync<T>(string p_Key, CancellationToken p_CancellationToken = default)
    {
        RedisValue[] v_Members = await m_Database.SetMembersAsync(p_Key);
        return v_Members.Select(p_Value => JsonSerializer.Deserialize<T>(p_Value.ToString())!).ToList();
    }

    public async Task<bool> SetContainsAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.SetContainsAsync(p_Key, JsonSerializer.Serialize(p_Value));
    }

    public async Task<long> SetLengthAsync(string p_Key, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.SetLengthAsync(p_Key);
    }

    public async Task<bool> SortedSetAddAsync<T>(string p_Key, T p_Value, int p_Score, TimeSpan p_Expiration = default, CancellationToken p_CancellationToken = default)
    {
        bool v_Result = await m_Database.SortedSetAddAsync(p_Key, JsonSerializer.Serialize(p_Value), p_Score);
        TimeSpan v_Expiration = p_Expiration == TimeSpan.Zero ? TimeSpan.FromHours(24) : p_Expiration;
        await m_Database.KeyExpireAsync(p_Key, v_Expiration);
        return v_Result;
    }

    public async Task<List<T>> SortedSetRangeByRankAsync<T>(string p_Key, long p_Start = 0, long p_Stop = -1, bool p_Descending = false, CancellationToken p_CancellationToken = default)
    {
        Order v_Order = p_Descending ? Order.Descending : Order.Ascending;
        RedisValue[] v_Values = await m_Database.SortedSetRangeByRankAsync(p_Key, p_Start, p_Stop, v_Order);
        return v_Values.Select(p_Value => JsonSerializer.Deserialize<T>(p_Value.ToString())!).ToList();
    }

    public async Task<List<(T Value, int Score)>> SortedSetRangeByRankWithScoresAsync<T>(string p_Key, long p_Start = 0, long p_Stop = -1, bool p_Descending = false, CancellationToken p_CancellationToken = default)
    {
        Order v_Order = p_Descending ? Order.Descending : Order.Ascending;
        SortedSetEntry[] v_Entries = await m_Database.SortedSetRangeByRankWithScoresAsync(p_Key, p_Start, p_Stop, v_Order);
        return v_Entries.Select(p_Entry =>
            (JsonSerializer.Deserialize<T>(p_Entry.Element.ToString())!, (int)p_Entry.Score)
        ).ToList();
    }

    public async Task<T> SortedSetGetByIndexAsync<T>(string p_Key, long p_Index, CancellationToken p_CancellationToken = default)
    {
        if (p_CancellationToken.IsCancellationRequested) return default;
        RedisValue[] v_Values = await m_Database.SortedSetRangeByRankAsync(p_Key, p_Index, p_Index);
        return v_Values.Length > 0 ? JsonSerializer.Deserialize<T>(v_Values[0].ToString())! : default;
    }

    public async Task<bool> SortedSetRemoveAsync<T>(string p_Key, T p_Value, CancellationToken p_CancellationToken = default)
    {
        return await m_Database.SortedSetRemoveAsync(p_Key, JsonSerializer.Serialize(p_Value));
    }

    public async Task<List<(T Value, int Score)>> SortedSetGetAllWithScoresAsync<T>(string p_Key, bool p_Descending = false, CancellationToken p_CancellationToken = default)
    {
        return await SortedSetRangeByRankWithScoresAsync<T>(p_Key, 0, -1, p_Descending, p_CancellationToken);
    }

    public async Task<string> StreamAddAsync<T>(string p_Key, T p_Value, TimeSpan p_Expiration = default, CancellationToken p_CancellationToken = default)
    {
        NameValueEntry[] v_Entry = [new NameValueEntry("payload", JsonSerializer.Serialize(p_Value))];
        RedisValue v_Id = await m_Database.StreamAddAsync(p_Key, v_Entry);
        TimeSpan v_Expiration = p_Expiration == TimeSpan.Zero ? TimeSpan.FromHours(24) : p_Expiration;
        await m_Database.KeyExpireAsync(p_Key, v_Expiration);
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

    public async Task RemoveByPatternAsync(string p_Pattern, IEnumerable<string> p_KeysToKeep = null, CancellationToken p_CancellationToken = default)
    {
        HashSet<string> v_KeysToKeepSet = p_KeysToKeep == null ? [] : [.. p_KeysToKeep];
        EndPoint[] v_Endpoints = p_Redis.GetEndPoints();

        foreach (EndPoint v_Endpoint in v_Endpoints)
        {
            if (p_CancellationToken.IsCancellationRequested) return;
            IServer v_Server = p_Redis.GetServer(v_Endpoint);
            if (v_Server.IsReplica) continue;

            await DeleteKeysForServerAsync(v_Server, p_Pattern, v_KeysToKeepSet, p_CancellationToken);
        }
    }

    private async Task DeleteKeysForServerAsync(IServer p_Server, string p_Pattern, HashSet<string> p_KeysToKeep, CancellationToken p_CancellationToken)
    {
        IEnumerable<RedisKey> v_Keys = p_Server.Keys(database: m_Database.Database, pattern: p_Pattern);
        List<RedisKey> v_KeysBatch = [];
        const int v_BatchSize = 1000;

        foreach (RedisKey v_Key in v_Keys)
        {
            if (p_CancellationToken.IsCancellationRequested) return;
            if (p_KeysToKeep.Contains((string)v_Key!)) continue;

            v_KeysBatch.Add(v_Key);
            if (v_KeysBatch.Count >= v_BatchSize)
            {
                await m_Database.KeyDeleteAsync(v_KeysBatch.ToArray());
                v_KeysBatch.Clear();
            }
        }

        if (v_KeysBatch.Count > 0)
            await m_Database.KeyDeleteAsync(v_KeysBatch.ToArray());
    }

    public async Task PublishAsync<T>(string p_Channel, T p_Message, CancellationToken p_CancellationToken = default)
    {
        ISubscriber v_Subscriber = p_Redis.GetSubscriber();
        await v_Subscriber.PublishAsync(RedisChannel.Literal(p_Channel), JsonSerializer.Serialize(p_Message));
    }

    public async Task<T> SubscribeAndWaitAsync<T>(string p_Channel, TimeSpan p_Timeout, CancellationToken p_CancellationToken = default)
    {
        ISubscriber v_Subscriber = p_Redis.GetSubscriber();
        TaskCompletionSource<T> v_Tcs = new();

        using CancellationTokenSource v_TimeoutCts = new(p_Timeout);
        using CancellationTokenSource v_LinkedCts = CancellationTokenSource.CreateLinkedTokenSource(p_CancellationToken, v_TimeoutCts.Token);

        await v_Subscriber.SubscribeAsync(RedisChannel.Literal(p_Channel), (p_Ch, p_Msg) =>
        {
            if (!p_Msg.IsNullOrEmpty)
            {
                T v_Message = JsonSerializer.Deserialize<T>(p_Msg!);
                v_Tcs.TrySetResult(v_Message);
            }
        });

        v_LinkedCts.Token.Register(() => v_Tcs.TrySetCanceled());

        try
        {
            return await v_Tcs.Task;
        }
        finally
        {
            await v_Subscriber.UnsubscribeAsync(RedisChannel.Literal(p_Channel));
        }
    }
}