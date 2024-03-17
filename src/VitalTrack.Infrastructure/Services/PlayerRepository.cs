using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using VitalTrack.Core.Models;
using VitalTrack.Core.Services;
using VitalTrack.Infrastructure.Aggregates;
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
        var playerExists = await PlayerExistsAsync(player.State.Name, cancellationToken);

        if (playerExists)
        {
            _logger.LogInformation(
                "Player {name} already exists, bypassing seed",
                player.State.Name
            );
            return;
        }

        _logger.LogInformation("Player file parsed");
        _logger.LogInformation("{player}", player.IntoLog());

        // Next, we'll spin up a transaction so we can rollback the player creation if something goes wrong
        using var transaction = _connection.BeginTransaction();
        using var currentConnection = transaction.Connection!;

        try
        {
            _logger.LogInformation("Creating player record for {name}", player.State.Name);

            // Create the player, returning the player ID so we can seed stats, items, and defenses
            var playerId = await currentConnection.ExecuteScalarAsync<int>(
                """
                INSERT INTO players (character_name, hit_points, health_cap, level, strength, dexterity, constitution, intelligence, wisdom, charisma)
                VALUES (@Name, @HitPoints, @HitPoints, @Level, @Strength, @Dexterity, @Constitution, @Intelligence, @Wisdom, @Charisma)
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
                    new
                    {
                        playerId,
                        ClassName = playerClass.Name,
                        playerClass.HitDiceValue,
                        playerClass.ClassLevel
                    }
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
                    new
                    {
                        playerId,
                        DefenseType = playerDefense.Type,
                        DefenseName = playerDefense.Defense
                    }
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
        // We're spreading character information across a few tables to keep our DBAs happy in a developer's attempt at poor man's normal form
        // We'll build the player aggregate based on the player information, defenses, items, and class table and combine them within our model mapping
        const string sql = """
            SELECT p.id,
                   p.character_name,
                   p.hit_points,
                   p.health_cap,
                   p.temporary_hit_points,
                   p.level,
                   p.strength,
                   p.dexterity,
                   p.constitution,
                   p.intelligence,
                   p.wisdom,
                   p.charisma,
                   pd.id as defense_id,
                   pd.defense,
                   pd.type,
                   i.id as item_id,
                   i.item_name,
                   i.affected_object,
                   i.affected_value,
                   c.id as class_id,
                   c.class_name,
                   pc.hit_dice_value,
                   pc.class_level
            FROM players p
            JOIN player_defenses pd ON p.id = pd.player_id
            JOIN player_items pi ON p.id = pi.player_id
            JOIN items i ON pi.item_id = i.id
            JOIN player_class pc ON p.id = pc.player_id
            JOIN classes c ON c.id = pc.class_id
            WHERE p.character_name = @Name
            """;

        // A neat Dapper trick I've learned over the years when model mapping aggregate queries, we can assign
        // the aggregate on the first model mapping pass to use in subsequent enumerator visits of the result set
        PlayerAggregate? playerAggregate = null;

        // We don't care about enumerating result sets themselves, as Dapper will handle that within the model mapping
        await _connection.QueryAsync<
            PlayerAggregate,
            PlayerDefenseEntity,
            ItemEntity,
            PlayerClassAggregate,
            PlayerAggregate
        >(
            sql,
            (player, playerDefense, playerItem, playerClass) =>
            {
                playerAggregate ??= player;

                // Since defenses, items, and classes are record types, we get value equality for free,
                // so no need to define our own IEqualityComparer for each record type, we can push them directly to the set
                playerAggregate.Defenses.Add(playerDefense);
                playerAggregate.Items.Add(playerItem);
                playerAggregate.Classes.Add(playerClass);

                return playerAggregate;
            },
            new { Name = name },
            splitOn: "defense_id, item_id, class_id"
        );

        // Once we have our aggregate, we'll map it out to a domain object for the core business logic to work with
        return playerAggregate is null
            ? null
            : Player.FromState(playerAggregate.HealthCap, playerAggregate.Into());
    }

    public Task UpdatePlayerAsync(PlayerState state, CancellationToken cancellationToken = default)
    {
        // Since we're only concerned about updating player health, we can keep this query rather simple
        return _connection.ExecuteAsync(
            """
            UPDATE players
            SET hit_points = @CurrentHitPoints,
                temporary_hit_points = @TemporaryHitPoints
            WHERE character_name = @Name
            """,
            new
            {
                CurrentHitPoints = state.HitPoints,
                state.TemporaryHitPoints,
                state.Name
            }
        );
    }

    public async ValueTask DisposeAsync()
    {
        // Hold my beer, GC... I think I know what I'm doing (news flash: I don't)
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
