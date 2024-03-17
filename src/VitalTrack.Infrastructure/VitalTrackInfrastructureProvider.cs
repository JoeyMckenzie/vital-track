using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VitalTrack.Core.Services;
using VitalTrack.Infrastructure.Services;

namespace VitalTrack.Infrastructure;

public static class VitalTrackInfrastructureProvider
{
    /// <summary>
    ///     Adds the infrastructure dependencies for Vital Track.
    /// </summary>
    /// <param name="services">Dependency collection.</param>
    /// <param name="pgsqlConnectionString"></param>
    public static void AddVitalTrackInfrastructure(
        this IServiceCollection services,
        string? pgsqlConnectionString
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pgsqlConnectionString);
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        services.AddScoped<IHitPointManager, HitPointManager>();
        services.AddScoped<IPlayerRepository>(provider => new PlayerRepository(
            pgsqlConnectionString,
            provider.GetRequiredService<ILogger<PlayerRepository>>()
        ));
    }
}
