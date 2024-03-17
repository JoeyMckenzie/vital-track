using System.Data;
using Dapper;
using Npgsql;
using VitalTrack.Core.Models;
using VitalTrack.Core.Services;
using VitalTrack.Infrastructure.Queries;

namespace VitalTrack.Infrastructure.Services;

/// <inheritdoc />
public class PlayerRepository : IPlayerRepository
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _currentTransaction;

    public PlayerRepository(string connectionString)
    {
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
        var player = await Player.FromTemplateAsync(filePath, cancellationToken);

        using var transaction = _connection.BeginTransaction();
        using var currentConnection = transaction.Connection!;
        var playerId = await currentConnection.ExecuteScalarAsync<int>(
            """
            INSERT INTO players (character_name, hit_points, level)
            VALUES (@Name, @HitPoints, @Level)
            RETURNING id
            """,
            new
            {
                Name = player.State.Name.ToLower(),
                player.State.HitPoints,
                player.State.Level
            }
        );
        await currentConnection.ExecuteAsync(
            """
            INSERT INTO player_stats (player_id, strength, dexterity, constitution, intelligence, wisdom, charisma)
            VALUES (@playerId, @Strength, @Dexterity, @Constitution, @Intelligence, @Wisdom, @Charisma)
            """,
            new
            {
                playerId,
                player.State.Stats.Strength,
                player.State.Stats.Dexterity,
                player.State.Stats.Constitution,
                player.State.Stats.Intelligence,
                player.State.Stats.Wisdom,
                player.State.Stats.Charisma
            }
        );
        transaction.Commit();
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
            SELECT p.id,
                   character_name,
                   hit_points,
                   level,
                   strength,
                   dexterity,
                   constitution,
                   intelligence,
                   wisdom,
                   charisma
            FROM players p
            JOIN public.player_stats ps on p.id = ps.player_id
            WHERE p.character_name = @Name
            """;

        var player = await _connection.QueryFirstOrDefaultAsync<PlayerAggregateQuery>(
            sql,
            new { Name = name }
        );

        return player is null ? null : Player.FromState(player.Into());
    }

    public Task UpdatePlayerAsync(PlayerState state, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Start()
    {
        _currentTransaction = _connection.BeginTransaction();
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

    public void Commit(bool dispose = true)
    {
        try
        {
            _currentTransaction?.Commit();

            if (dispose)
            {
                _currentTransaction?.Dispose();
            }
        }
        catch (Exception)
        {
            _currentTransaction?.Rollback();
        }
    }
}
