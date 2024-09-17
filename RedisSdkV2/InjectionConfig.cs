using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace RedisSdkV2;

public static class InjectionConfig
{
    public static void UseRedisSdkForSingleNode(this IServiceCollection services, string redisHostAddress)
    {
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisHostAddress));

        services.AddTransient<IRedisRedLockService, RedisRedLockService>();
        services.AddTransient<IRedisService, RedisService>();
    }

    public static void UseRedisSdkForMultipleNodes(this IServiceCollection services, List<string> redisHostAddressList)
    {
        var connectionMultiplexers = redisHostAddressList.Select(x => ConnectionMultiplexer.Connect(x)).ToList();
        connectionMultiplexers.ForEach(x =>
        {
            services.AddSingleton<IConnectionMultiplexer>(x);
            services.AddSingleton<IConnectionMultiplexer>(x);
        });
    }
}