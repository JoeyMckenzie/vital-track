using Microsoft.Extensions.DependencyInjection;

using VitalTrack.Core.Services;
using VitalTrack.Infrastructure.Services;

namespace VitalTrack.Infrastructure;

public static class VitalTrackInfrastructureProvider
{
    /// <summary>
    ///     Adds the infrastructure dependencies for Vital Track.
    /// </summary>
    /// <param name="services">Dependency collection.</param>
    public static void AddVitalTrackInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IHitPointManager, HitPointManager>();

        // In the real world, this would be a layer on top of our persistence mechanisms that we would **not** want
        // registered as a singleton (ask a friendly DBA how they feel about recovering an exhausted connection pool...)
        // Since the player repository manages players in-memory, we can register it as a singleton as our unit of work
        // will persist across requests, though this would better be served as a scoped dependency in the case of an actual DB.
        services.AddSingleton<IPlayerRepository, PlayerRepository>();
    }
}