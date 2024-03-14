namespace VitalTrack.Core.Models;

/// <summary>
///     Internal player state tracked and managed by player objects containing various properties associated to a player.
/// </summary>
public readonly record struct PlayerState
{
    /// <summary>
    ///     Name of the player.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Player lever, can vary by their independent class.
    /// </summary>
    public required int Level { get; init; }

    /// <summary>
    ///     Player hit point cap.
    /// </summary>
    public required int HitPoints { get; init; }

    /// <summary>
    ///     Any temporary hit points the player may have.
    /// </summary>
    public int TemporaryHitPoints { get; init; }

    /// <summary>
    ///     Classes associated to the player.
    /// </summary>
    public required IEnumerable<PlayerClass> Classes { get; init; }

    /// <summary>
    ///     Stats associated to the player, can be modified by items.
    /// </summary>
    public required PlayerStats Stats { get; init; }

    /// <summary>
    ///     Items currently held by the player.
    /// </summary>
    public required IEnumerable<PlayerItem> Items { get; init; }

    /// <summary>
    ///     Defenses the player has, affecting damage intake.
    /// </summary>
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
