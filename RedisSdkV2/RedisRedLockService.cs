using System.Text.Json;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace RedisSdkV2;

public class RedisRedLockService(IConnectionMultiplexer connectionMultiplexer) : IRedisRedLockService
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();
    private readonly RedLockFactory _redLockFactory = RedLockFactory.Create(new List<RedLockMultiplexer>
    {
        new(connectionMultiplexer)
    });

    public async Task<T> GetOrCreate<T>(string key, Func<T> func, TimeSpan timeout)
    {
        var redisValue = await _database.StringGetAsync(key);
        if (!redisValue.IsNullOrEmpty)
        {
            return JsonSerializer.Deserialize<T>(redisValue)!;
        }


        var redLock = await _redLockFactory.CreateLockAsync($"lock-{key}", timeout, TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(5));

        if (redLock.IsAcquired)
        {
            redisValue = await _database.StringGetAsync(key);
            if (!redisValue.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<T>(redisValue)!;
            }

            var value = func.Invoke();
            await _database.StringSetAsync(key, JsonSerializer.Serialize(value), TimeSpan.FromSeconds(5));

            return value;
        }
        else
        {
            throw new RedisException("Could not acquire a lock");
        }
    }

    public void Update<T>(string key, T value)
    {
        throw new NotImplementedException();
    }
}

public interface IRedisRedLockService
{
    void Update<T>(string key, T value);
    Task<T> GetOrCreate<T>(string key, Func<T> func, TimeSpan timeout);
}