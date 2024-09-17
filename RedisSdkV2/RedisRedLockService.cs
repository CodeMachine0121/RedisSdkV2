using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace RedisSdkV2;

public class RedisRedLockService(IConnectionMultiplexer connectionMultiplexer, IRedisService redisService)
    : IRedisRedLockService
{
    private readonly RedLockFactory _redLockFactory = RedLockFactory.Create(new List<RedLockMultiplexer>
    {
        new(connectionMultiplexer)
    });


    public async Task<T> GetOrCreate<T>(string key, Func<T> func, TimeSpan timeout)
    {
        using (var redLock = await _redLockFactory.CreateLockAsync($"lock-{key}", timeout, TimeSpan.FromSeconds(10),
                   TimeSpan.FromSeconds(5)))
        {
            if (redLock.IsAcquired) return await redisService.GetOrCreate(key, func, timeout);

            throw new RedisException("Could not acquire a lock");
        }
    }

    public async Task Update<T>(string key, T value)
    {
        using (var redLock = await _redLockFactory.CreateLockAsync($"lock-{key}", TimeSpan.FromSeconds(10),
                   TimeSpan.FromSeconds(10),
                   TimeSpan.FromSeconds(5)))
        {
            if (redLock.IsAcquired)
            {
                await redisService.Update(key, value);
                return;
            }

            throw new RedisException("Could not acquire a lock");
        }
    }
}

public interface IRedisRedLockService
{
    Task<T> GetOrCreate<T>(string key, Func<T> func, TimeSpan timeout);
    Task Update<T>(string key, T value);
}