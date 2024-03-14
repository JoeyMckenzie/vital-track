using VitalTrack.Core.Models;
using VitalTrack.Core.Services;

namespace VitalTrack.Infrastructure.Services;

/// <inheritdoc />
public class PlayerRepository : IPlayerRepository
{
    private readonly List<Player> _players = [];

    public async Task SeedPlayerFromTemplateAsync(
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        var player = await Player.FromTemplateAsync(filePath, cancellationToken);
        _players.Add(player);
    }

    public async Task<bool> PlayerExistsAsync(string name, CancellationToken cancellationToken)
    {
        var existingPlayer = await FindPlayerAsync(name, cancellationToken);

        return existingPlayer is not null;
    }

    public Task<Player?> FindPlayerAsync(string name, CancellationToken cancellationToken)
    {
        var player = _players.FirstOrDefault(p =>
            string.Equals(name, p.State.Name, StringComparison.CurrentCultureIgnoreCase)
        );

        return Task.FromResult(player);
    }
}
