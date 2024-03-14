using System.Net;
using VitalTrack.Core;
using VitalTrack.Core.Models;
using VitalTrack.Core.Services;

namespace VitalTrack.Infrastructure.Services;

/// <inheritdoc />
public class HitPointManager(IPlayerRepository playerRepository) : IHitPointManager
{
    public async Task<VitalTrackCoreResponse<PlayerState>> DealDamageAsync(
        string playerName,
        HitPointModifierRequest request,
        CancellationToken cancellationToken
    )
    {
        var player = await playerRepository.FindPlayerAsync(playerName, cancellationToken);

        player!.DealDamage(request.DamageType ?? string.Empty, request.Amount ?? 0);

        return new VitalTrackCoreResponse<PlayerState>(player.State, HttpStatusCode.OK);
    }

    public async Task<VitalTrackCoreResponse<PlayerState>> HealHitPointsAsync(
        string playerName,
        int amount,
        CancellationToken cancellationToken
    )
    {
        var player = await playerRepository.FindPlayerAsync(playerName, cancellationToken);

        player!.Heal(amount);

        return new VitalTrackCoreResponse<PlayerState>(player.State, HttpStatusCode.OK);
    }

    public async Task<VitalTrackCoreResponse<PlayerState>> AddTemporaryHitPoints(
        string playerName,
        int amount,
        CancellationToken cancellationToken
    )
    {
        var player = await playerRepository.FindPlayerAsync(playerName, cancellationToken);

        player!.AddTemporaryHitPoints(amount);

        return new VitalTrackCoreResponse<PlayerState>(player.State, HttpStatusCode.OK);
    }
}
