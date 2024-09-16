using System.Text.Json;
using StackExchange.Redis;

namespace RedisSdkV2;

public interface IRedisService
{
    T GetOrCreate<T>(string key, Func<T> func, TimeSpan timeout);
    void Update<T>(string key, T value);
}

public class RedisService(IConnectionMultiplexer connectionMultiplexer) : IRedisService
{
    private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();

    public T GetOrCreate<T>(string key, Func<T> func, TimeSpan timeout)
    {
        var redisValue = _redis.StringGet(key);
        if (redisValue.IsNullOrEmpty)
        {
            var invoke = func.Invoke();
            _redis.StringSet(key, JsonSerializer.Serialize(invoke),  timeout);
        }
        
        return  JsonSerializer.Deserialize<T>(_redis.StringGet(key))!;
    }

    public void Update<T>(string key, T value)
    {
        var redisValue = _redis.StringGet(key);
        if (redisValue.IsNullOrEmpty)
        {
            throw new KeyNotFoundException("Key not found in Redis");
        }
        
        _redis.StringSet(key, JsonSerializer.Serialize(value), TimeSpan.FromSeconds(10));
    }



}