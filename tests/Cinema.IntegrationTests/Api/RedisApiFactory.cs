using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Cinema.IntegrationTests.Api;

/// <summary>Hosts the real API in memory with nothing replaced, pointed at the given Redis.</summary>
public sealed class RedisApiFactory(string redisConnectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:Redis", redisConnectionString);
}
