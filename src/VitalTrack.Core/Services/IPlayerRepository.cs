using VitalTrack.Core.Models;

namespace VitalTrack.Core.Services;

/// <summary>
///     A player storage repository for performing simple CRUD operations on a player. In a more real world context,
///     this would be a thin layer on top of a Dapper-like ORM, or a DbContext with DbSet<Player> in the case of EF Core.
/// </summary>
public interface IPlayerRepository
{
    /// <summary>
    ///     Constructs a player instance from a JSON template file and attempts to add them to in-memory storage.
    /// </summary>
    /// <param name="filePath">Local system file path to the player template JSON file.</param>
    /// <param name="cancellationToken">Default cancellation context.</param>
    /// <returns>OK, if the player was successfully seeded.</returns>
    Task SeedPlayerFromTemplateAsync(
        string filePath,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Verifies a player exists within storage given a player name.
    /// </summary>
    /// <param name="name">Player name.</param>
    /// <param name="cancellationToken">Default cancellation context.</param>
    /// <returns>True, if the player exists.</returns>
    Task<bool> PlayerExistsAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a player model from storage given the name of the player.
    /// </summary>
    /// <param name="name">Player name.</param>
    /// <param name="cancellationToken">Default cancellation context.</param>
    /// <returns>Player model, if they exist.</returns>
    Task<Player?> FindPlayerAsync(string name, CancellationToken cancellationToken = default);
}