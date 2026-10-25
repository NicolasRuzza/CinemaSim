using Cinema.Api.Features.Seats.HoldSeat;
using Cinema.BddTests.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cinema.IntegrationTests.Api;

/// <summary>Hosts the real API in memory, with the Redis seat lock swapped for the in-memory fake.</summary>
public sealed class CinemaApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ISeatLockStore>();
            services.TryAddSingleton(TimeProvider.System);
            services.AddSingleton<ISeatLockStore, InMemorySeatLockStore>();
        });
}
