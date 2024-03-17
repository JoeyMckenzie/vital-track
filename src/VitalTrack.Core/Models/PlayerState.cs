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

    /// <summary>
    ///     Adjusts any player stats based on the currently held items based on the player template.
    ///     Note, this is NOT an idempotent state transition, so multiple calls will increase stats
    ///     beyond what they may have been intended to improve upon.
    /// </summary>
    /// <returns>Newly computed player state.</returns>
    public PlayerState AdjustStatsForApplicableItems()
    {
        // If we don't have any items, there's no adjustments to the player's current state
        if (!Items.Any())
        {
            return this;
        }

        // Roll through each item and compute the new state based on the item's affected modifiers
        foreach (var item in Items)
        {
            if (
                string.Equals(
                    item.Modifier.AffectedObject,
                    "stats",
                    StringComparison.CurrentCultureIgnoreCase
                )
            )
            {
                var playerState = item.Modifier.AffectedValue switch
                {
                    // TODO: Only doing constitution for now, but should eventually be fleshed out for others
                    "constitution"
                        => this with
                        {
                            Stats = Stats with
                            {
                                Constitution = Stats.Constitution + item.Modifier.Value
                            }
                        },
                    _ => this
                };
            }
        }

        return this;
    }
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
