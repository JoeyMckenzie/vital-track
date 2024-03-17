using System.Data;

using Dapper;

using Microsoft.Extensions.Logging;

using Npgsql;

using VitalTrack.Core.Models;
using VitalTrack.Core.Services;
using VitalTrack.Infrastructure.Entities;

namespace VitalTrack.Infrastructure.Services;

/// <inheritdoc />
public class PlayerRepository : IPlayerRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<PlayerRepository> _logger;

    public PlayerRepository(string connectionString, ILogger<PlayerRepository> logger)
    {
        _logger = logger;
        _connection = new NpgsqlConnection(connectionString);

        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }
    }

    public async Task SeedPlayerFromTemplateAsync(
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Seeding user from file path {filePath}", filePath);

        // First, we'll create a player domain object based on the JSON character template
        var player = await Player.FromTemplateAsync(filePath, cancellationToken);

        _logger.LogInformation("Player file parsed");
        _logger.LogInformation(player.IntoLog());

        // Next, we'll spin up a transaction so we can rollback the player creation if something goes wrong
        using var transaction = _connection.BeginTransaction();
        using var currentConnection = transaction.Connection!;

        try
        {
            _logger.LogInformation("Creating player record for {name}", player.State.Name);

            // Create the player, returning the player ID so we can seed stats, items, and defenses
            var playerId = await currentConnection.ExecuteScalarAsync<int>(
                """
                INSERT INTO players (character_name, hit_points, level, strength, dexterity, constitution, intelligence, wisdom, charisma)
                VALUES (@Name, @HitPoints, @Level, @Strength, @Dexterity, @Constitution, @Intelligence, @Wisdom, @Charisma)
                RETURNING id
                """,
                new
                {
                    Name = player.State.Name.ToLower(),
                    player.State.HitPoints,
                    player.State.Level,
                    player.State.Stats.Strength,
                    player.State.Stats.Dexterity,
                    player.State.Stats.Constitution,
                    player.State.Stats.Intelligence,
                    player.State.Stats.Wisdom,
                    player.State.Stats.Charisma
                }
            );

            foreach (var playerClass in player.State.Classes)
            {
                _logger.LogInformation("Creating player class {name}", playerClass.Name);

                // Next, add the player class records
                // TODO: This can be executed as a single query with Postgres batch inserts, but let's save that for a rainy day
                await currentConnection.ExecuteAsync(
                    """
                    INSERT INTO player_class (player_id, class_id, hit_dice_value, class_level)
                    VALUES (@playerId, (SELECT id FROM classes WHERE class_name = @ClassName), @HitDiceValue, @ClassLevel)
                    """,
                    new { playerId, ClassName = playerClass.Name, playerClass.HitDiceValue, playerClass.ClassLevel }
                );
            }

            foreach (var playerDefense in player.State.Defenses)
            {
                _logger.LogInformation("Creating player defense {defense}", playerDefense.Defense);

                // Next, roll through the defenses and a record for each one
                await currentConnection.ExecuteAsync(
                    """
                    INSERT INTO player_defenses (player_id, type, defense)
                    VALUES (@playerId, @DefenseType, @DefenseName)
                    """,
                    new { playerId, DefenseType = playerDefense.Type, DefenseName = playerDefense.Defense }
                );
            }

            foreach (var playerItem in player.State.Items)
            {
                // Since items can be used by multiple players, we'll create a many-to-many between players and items
                var itemId = await currentConnection.ExecuteScalarAsync<int>(
                    """
                    INSERT INTO items (item_name, affected_object, affected_value, value)
                    VALUES (@ItemName, @AffectedObject, @AffectedValue, @Value)
                    ON CONFLICT DO NOTHING
                    RETURNING id
                    """,
                    new
                    {
                        ItemName = playerItem.Name,
                        playerItem.Modifier.AffectedValue,
                        playerItem.Modifier.AffectedObject,
                        playerItem.Modifier.Value
                    }
                );

                // Roll through the player's items and add a record for each
                await currentConnection.ExecuteAsync(
                    """
                    INSERT INTO player_items (player_id, item_id)
                    VALUES (@playerId, @itemId)
                    """,
                    new { playerId, itemId }
                );
            }

            transaction.Commit();

            _logger.LogInformation("Player {name} successfully seeded", player.State.Name);
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Failed to create player from template {filePath}: {message}",
                filePath,
                e.Message
            );
            transaction.Rollback();
        }
    }

    public async Task<bool> PlayerExistsAsync(string name, CancellationToken cancellationToken)
    {
        var results = await _connection.QueryAsync(
            """
            SELECT 1
            FROM players
            WHERE character_name = @Name
            """,
            new { Name = name.ToLower() }
        );

        return results.Any();
    }

    public async Task<Player?> FindPlayerAsync(string name, CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT *
                           FROM players p
                           JOIN public.player_defenses pd on p.id = pd.player_id
                           JOIN public.player_items pi on p.id = pi.player_id
                           join public.player_class pc on p.id = pc.player_id
                           WHERE p.character_name = @Name
                           """;

        var player = await _connection.QueryAsync<
            PlayerEntity,
            PlayerDefenseEntity,
            PlayerItemEntity,
            PlayerClassEntity,
            PlayerEntity
        >(
            sql,
            (player, playerDefense, playerItem, playerClass) =>
            {
                player.Defenses.Add(playerDefense);
                player.Items.Add(playerItem);
                player.Classes.Add(playerClass);
                return player;
            },
            new { Name = name }
        );

        return null;
    }

    public Task UpdatePlayerAsync(PlayerState state, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (_connection is IAsyncDisposable connectionAsyncDisposable)
        {
            await connectionAsyncDisposable.DisposeAsync();
        }
        else
        {
            _connection.Dispose();
        }
    }
}