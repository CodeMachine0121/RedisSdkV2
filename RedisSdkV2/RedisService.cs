using System.Text.Json;
using StackExchange.Redis;

namespace RedisSdkV2;

public interface IRedisService
{
    T GetOrCreate<T>(string key, Func<T> func);
    void Update<T>(string key, T value);
}

public class RedisService(IConnectionMultiplexer connectionMultiplexer) : IRedisService
{
    private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();

    public T GetOrCreate<T>(string key, Func<T> func)
    {
        var redisValue = _redis.StringGet(key);
        if (redisValue.IsNullOrEmpty)
        {
            _redis.StringSet(key, JsonSerializer.Serialize(func.Invoke()),  TimeSpan.FromSeconds(10));
        }
        
        return  JsonSerializer.Deserialize<T>(_redis.StringGet(key).ToString())!;
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