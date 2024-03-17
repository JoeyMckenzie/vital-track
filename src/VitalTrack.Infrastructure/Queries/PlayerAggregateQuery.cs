using VitalTrack.Core.Concerns;
using VitalTrack.Core.Models;

namespace VitalTrack.Infrastructure.Queries;

public record PlayerAggregateQuery : IMappableInto<PlayerState>
{
    public required string Name { get; init; }

    public required int HitPoints { get; init; }

    public required int Level { get; init; }

    public required int Strength { get; init; }

    public required int Dexterity { get; init; }

    public required int Constitution { get; init; }

    public required int Intelligence { get; init; }

    public required int Wisdom { get; init; }

    public required int Charisma { get; init; }

    public PlayerState Into()
    {
        return new PlayerState
        {
            Name = Name,
            Level = Level,
            HitPoints = HitPoints,
            TemporaryHitPoints = 0,
            Classes = ArraySegment<PlayerClass>.Empty,
            Items = ArraySegment<PlayerItem>.Empty,
            Defenses = ArraySegment<PlayerDefense>.Empty,
            Stats = new PlayerStats
            {
                Strength = 0,
                Dexterity = 0,
                Constitution = 0,
                Intelligence = 0,
                Wisdom = 0,
                Charisma = 0
            }
        };
    }
}
