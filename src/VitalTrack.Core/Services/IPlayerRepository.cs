using VitalTrack.Core.Models;

namespace VitalTrack.Core.Services;

/// <summary>
///     A player storage repository for performing simple CRUD operations on a player.
/// </summary>
public interface IPlayerRepository
{
    Task SeedPlayerFromTemplateAsync(
        string filePath,
        CancellationToken cancellationToken = default
    );

    Task<bool> PlayerExistsAsync(string name, CancellationToken cancellationToken = default);

    Task<Player?> FindPlayerAsync(string name, CancellationToken cancellationToken = default);
}
