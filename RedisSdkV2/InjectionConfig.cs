using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace RedisSdkV2;

public static class InjectionConfig
{

    public static void UseRedisSdk(this IServiceCollection services, string redisHostAddress)
    {
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisHostAddress));
        
        services.AddTransient<IRedisService, RedisService>();
    }
    
    
}