using VitalTrack.Core.Concerns;
using VitalTrack.Core.Models;
using VitalTrack.Infrastructure.Entities;

namespace VitalTrack.Infrastructure.Aggregates;

/// <summary>
///     The player aggregate query represents our player as it exists within the database as a purely anemic domain model.
/// </summary>
public record PlayerAggregate : EntityBase, IMappableInto<PlayerState>
{
    public required string CharacterName { get; init; }

    public required int HitPoints { get; init; }

    public required int TemporaryHitPoints { get; init; }

    public required int HealthCap { get; init; }

    public required int Level { get; init; }

    public required int Strength { get; init; }

    public required int Dexterity { get; init; }

    public required int Constitution { get; init; }

    public required int Intelligence { get; init; }

    public required int Wisdom { get; init; }

    public required int Charisma { get; init; }

    public required ISet<PlayerDefenseEntity> Defenses { get; init; } =
        new HashSet<PlayerDefenseEntity>();

    public required ISet<ItemEntity> Items { get; init; } = new HashSet<ItemEntity>();

    public required ISet<PlayerClassAggregate> Classes { get; init; } =
        new HashSet<PlayerClassAggregate>();

    public PlayerState Into()
    {
        return new PlayerState
        {
            Name = CharacterName,
            Level = Level,
            HitPoints = HitPoints,
            TemporaryHitPoints = TemporaryHitPoints,
            Classes = Classes.Select(c => new PlayerClass(
                c.ClassName,
                c.HitDiceValue,
                c.ClassLevel
            )),
            Stats = new PlayerStats(
                Strength,
                Dexterity,
                Constitution,
                Intelligence,
                Wisdom,
                Charisma
            ),
            Items = Items.Select(i => new PlayerItem(
                i.ItemName,
                new PlayerItemModifier(i.AffectedObject, i.AffectedValue, i.Value)
            )),
            Defenses = Defenses.Select(d => new PlayerDefense(d.Type, d.Defense))
        };
    }
}
