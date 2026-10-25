using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Cinema.Infrastructure.Redis;

public static class RedisServiceCollectionExtensions
{
    /// <summary>Registers one shared Redis connection, read from the "Redis" connection string.</summary>
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("The connection string 'Redis' is not configured.");

        var options = ConfigurationOptions.Parse(connectionString);

        // Let the API start even if Redis is down; the connection keeps retrying in the background.
        options.AbortOnConnectFail = false;

        // The factory runs on first use, so nothing connects until a request needs the lock.
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(options));

        return services;
    }
}
