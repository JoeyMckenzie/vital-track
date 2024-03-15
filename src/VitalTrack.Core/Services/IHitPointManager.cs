using VitalTrack.Core.Domain;
using VitalTrack.Core.Models;

namespace VitalTrack.Core.Services;

/// <summary>
///     A hit point manager for orchestrating the various actions taken on a player.
/// </summary>
public interface IHitPointManager
{
    /// <summary>
    ///     Attempts to deal damage to the player's current health. If the damage dealt takes a player beyond the brink
    ///     of death, we'll zero out the player's current health value. No use in beating a dead horse...
    /// </summary>
    /// <param name="playerName">Name of the player.</param>
    /// <param name="request">Damage request context.</param>
    /// <param name="cancellationToken">Default cancellation context.</param>
    /// <returns>OK, if the damage was successfully dealt.</returns>
    Task<VitalTrackResponse<PlayerState>> DealDamageAsync(
        string playerName,
        HitPointModifierRequest request,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Attempts to deal damage to the player's current health. If the damage dealt takes a player beyond the brink
    ///     of death, we'll zero out the player's current health value. No use in beating a dead horse...
    /// </summary>
    /// <param name="playerName">Name of the player.</param>
    /// <param name="request">Damage request context.</param>
    /// <param name="cancellationToken">Default cancellation context.</param>
    /// <returns>OK, if the damage was successfully dealt.</returns>
    Task<VitalTrackResponse<PlayerState>> HealHitPointsAsync(
        string playerName,
        int amount,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Attempts to deal damage to the player's current health. If the damage dealt takes a player beyond the brink
    ///     of death, we'll zero out the player's current health value. No use in beating a dead horse...
    /// </summary>
    /// <param name="playerName">Name of the player.</param>
    /// <param name="request">Damage request context.</param>
    /// <param name="cancellationToken">Default cancellation context.</param>
    /// <returns>OK, if the damage was successfully dealt.</returns>
    Task<VitalTrackResponse<PlayerState>> AddTemporaryHitPoints(
        string playerName,
        int amount,
        CancellationToken cancellationToken
    );
}
