using System.Text.Json;
using StackExchange.Redis;

namespace RedisSdkV2;

public interface IRedisService
{
    Task<T> GetOrCreate<T>(string key, Func<T> func, TimeSpan timeout);
    Task Update<T>(string key, T value);
}

public class RedisService(IConnectionMultiplexer connectionMultiplexer) : IRedisService
{
    private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();

    public async Task<T> GetOrCreate<T>(string key, Func<T> func, TimeSpan timeout)
    {
        var redisValue = await _redis.StringGetAsync(key);
        if (redisValue.IsNullOrEmpty)
        {
            var invoke = func.Invoke();
            await _redis.StringSetAsync(key, JsonSerializer.Serialize(invoke),  timeout);
        }
        
        return  JsonSerializer.Deserialize<T>(await _redis.StringGetAsync(key))!;
    }

    public async Task Update<T>(string key, T value)
    {
        var redisValue = await _redis.StringGetAsync(key);
        if (redisValue.IsNullOrEmpty)
        {
            throw new KeyNotFoundException("Key not found in Redis");
        }
        
        await _redis.StringSetAsync(key, JsonSerializer.Serialize(value), TimeSpan.FromSeconds(10));
    }



}