using Microsoft.Extensions.DependencyInjection;

namespace VitalTrack.Core;

/// <summary>
///     A provider for Vital Track's core dependencies, wiring up internal
///     services for use by consumers of the engine.
/// </summary>
public static class VitalTrackCoreProvider
{
    /// <summary>
    ///     Adds the core dependencies for Vital Track.
    /// </summary>
    /// <param name="services">Dependency collection.</param>
    public static void AddVitalTrackCore(this IServiceCollection services) { }
}
