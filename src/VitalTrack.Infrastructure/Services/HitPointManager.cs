using VitalTrack.Core.Domain;
using VitalTrack.Core.Models;
using VitalTrack.Core.Services;

namespace VitalTrack.Infrastructure.Services;

/// <inheritdoc />
public class HitPointManager(IPlayerRepository playerRepository) : IHitPointManager
{
    public async Task<VitalTrackResponse<PlayerState>> DealDamageAsync(
        string playerName,
        HitPointModifierRequest request,
        CancellationToken cancellationToken
    )
    {
        var player = await playerRepository.FindPlayerAsync(playerName, cancellationToken);

        player!.DealDamage(request.DamageType ?? string.Empty, request.Amount ?? 0);

        await playerRepository.UpdatePlayerAsync(player.State, cancellationToken);

        return new VitalTrackResponse<PlayerState>(player.State);
    }

    public async Task<VitalTrackResponse<PlayerState>> HealHitPointsAsync(
        string playerName,
        int amount,
        CancellationToken cancellationToken
    )
    {
        var player = await playerRepository.FindPlayerAsync(playerName, cancellationToken);

        player!.Heal(amount);

        await playerRepository.UpdatePlayerAsync(player.State, cancellationToken);

        return new VitalTrackResponse<PlayerState>(player.State);
    }

    public async Task<VitalTrackResponse<PlayerState>> AddTemporaryHitPoints(
        string playerName,
        int amount,
        CancellationToken cancellationToken
    )
    {
        var player = await playerRepository.FindPlayerAsync(playerName, cancellationToken);

        player!.AddTemporaryHitPoints(amount);

        await playerRepository.UpdatePlayerAsync(player.State, cancellationToken);

        return new VitalTrackResponse<PlayerState>(player.State);
    }
}
