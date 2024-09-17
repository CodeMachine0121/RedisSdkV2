using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace RedisSdkV2;

public static class InjectionConfig
{
    public static void UseRedisSdkForSingleNode(this IServiceCollection services, string redisHostAddress,
        bool isRedLock = false)
    {
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisHostAddress));

        if (isRedLock)
        {
            services.AddTransient<IRedisService, RedisRedLockService>();
        }
        else
        {
            services.AddTransient<IRedisService, RedisService>();
        }
    }

    public static void UseRedisSdkForMultipleNodes(this IServiceCollection services, List<string> redisHostAddressList, bool isRedLock = false)
    {
        var connectionMultiplexers = redisHostAddressList.Select(x => ConnectionMultiplexer.Connect(x)).ToList();
        if (isRedLock)
        {
            connectionMultiplexers.ForEach(x=> 
                services.AddSingleton<IConnectionMultiplexer>(x));
        }
        else
        {
            connectionMultiplexers.ForEach(x => 
                services.AddSingleton<IConnectionMultiplexer>(x));
        }
    }
}