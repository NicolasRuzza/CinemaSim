namespace Cinema.Api.Features.Seats.HoldSeat;

public static class HoldSeatServiceCollectionExtensions
{
    /// <summary>Registers everything the hold seat use case needs, so Program.cs stays small.</summary>
    public static IServiceCollection AddHoldSeat(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediator();
        services.AddOptions<HoldSeatOptions>()
            .Bind(configuration.GetSection("HoldSeat"))
            .Validate(options => options.HoldDuration > TimeSpan.Zero, "HoldSeat:HoldDuration must be greater than zero.")
            .ValidateOnStart();
        services.AddSingleton<ISeatLockStore, RedisSeatLockStore>();
        services.AddSingleton<ISessionSchedule, MockSessionSchedule>();

        return services;
    }
}
