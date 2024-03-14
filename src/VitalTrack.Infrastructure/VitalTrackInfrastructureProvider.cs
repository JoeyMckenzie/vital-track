using Microsoft.Extensions.DependencyInjection;
using VitalTrack.Core.Services;
using VitalTrack.Infrastructure.Services;

namespace VitalTrack.Infrastructure;

public static class VitalTrackInfrastructureProvider
{
    /// <summary>
    ///     Adds the core dependencies for Vital Track.
    /// </summary>
    /// <param name="services">Dependency collection.</param>
    public static void AddVitalTrackInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IHitPointManager, HitPointManager>();
        services.AddSingleton<IPlayerRepository, PlayerRepository>();
    }
}
