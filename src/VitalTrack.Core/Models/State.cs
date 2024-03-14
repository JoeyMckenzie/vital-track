namespace VitalTrack.Core.Models;

/// <summary>
///     Internal player state tracked and managed by player objects.
/// </summary>
public readonly record struct PlayerState
{
    public PlayerState() { }

    public required string Name { get; init; }

    public required int Level { get; init; }

    public required int HitPoints { get; init; }

    public int TemporaryHitPoints { get; init; }

    public required IEnumerable<PlayerClass> Classes { get; init; }

    public required PlayerStats Stats { get; init; }

    public required IEnumerable<PlayerItem> Items { get; init; }

    public required IEnumerable<PlayerDefense> Defenses { get; init; }
}

public readonly record struct PlayerClass(string Name, int HitDiceValue, int ClassLevel);

public readonly record struct PlayerStats(
    int Strength,
    int Dexterity,
    int Constitution,
    int Intelligence,
    int Wisdom,
    int Charisma
);

public readonly record struct PlayerItem(string Name, PlayerItemModifier Modifier);

public readonly record struct PlayerItemModifier(
    string AffectedObject,
    string AffectedValue,
    int Value
);

public record PlayerDefense(string Type, string Defense);
