using System.Text.Json;
using VitalTrack.Core.Domain;

namespace VitalTrack.Core.Models;

/// <summary>
///     Represents a player within the Vital Track system responsible for
///     managing internal player state and actionable state transitions.
/// </summary>
public class Player
{
    /// <summary>
    ///     A player's health cap is the maximum amount of HP available to the player.
    ///     We'll keep this internally tracked value to keep players from healing
    ///     more than what they total health pool allows.
    /// </summary>
    private readonly int _healthCap;

    private Player(int healthCap)
    {
        _healthCap = healthCap;
    }

    /// <summary>
    ///     Internal player state, containing all player stats, items, and defenses.
    ///     State transitions are internal and cannot be performed outside of an action.
    /// </summary>
    public PlayerState State { get; private set; }

    /// <summary>
    ///     Deals damage to the player, producing a new internal state with reduced hit points. If the damage a player takes
    ///     reduces their health below zero, we'll set the health value to zero so subsequent damage dealt to the player is
    ///     idempotent - further actions taken on a player with zero damage will not produce a new internal state. If a
    ///     player has temporary hit points, those will be deducted before taking damage to their effective hit points.
    /// </summary>
    /// <param name="type">Damage type.</param>
    /// <param name="amount">Damage amount.</param>
    public void DealDamage(string type, int amount)
    {
        // If the player currently has no hit points, we won't take any actions
        if (State.HitPoints == 0)
        {
            return;
        }

        // We'll need to keep track of how many hit points to subtract
        // based on temporary health and current hit point value
        var hitPointsToSubtract = CalculateAdjustedDamageValue(type, amount);

        // If the player takes no damage based on the damage type, no state transition is required
        if (hitPointsToSubtract == 0)
        {
            return;
        }

        var predictedTemporaryHitPointsRemaining = State.TemporaryHitPoints - amount;

        // First, we'll check if there's temporary hit points we can deduct
        if (State.TemporaryHitPoints > 0)
        {
            if (predictedTemporaryHitPointsRemaining < 0)
            {
                State = State with { TemporaryHitPoints = 0 };
                hitPointsToSubtract = Math.Abs(predictedTemporaryHitPointsRemaining);
            }
            else
            {
                State = State with { TemporaryHitPoints = predictedTemporaryHitPointsRemaining };
                hitPointsToSubtract = 0;
            }
        }

        // Now, we'll only need to subtract the remaining damage value after mitigation from the temporary hit points
        if (hitPointsToSubtract > 0)
        {
            // If the damage dealt takes a player below 0 hit points, we'll bottom out the state
            var predictedHealth = State.HitPoints - hitPointsToSubtract;
            if (predictedHealth < 0)
            {
                State = State with { HitPoints = 0 };
            }
            else
            {
                State = State with { HitPoints = predictedHealth };
            }
        }
    }

    /// <summary>
    ///     Heals a player current health, producing new internal state with increased hit points.
    ///     If healing will take a player beyond their health cap, we'll top the hit points at the cap.
    /// </summary>
    /// <param name="amount">Amount of healing to receive.</param>
    public void Heal(int amount)
    {
        // We'll check the predicted health value from the healing
        // If the heal takes a player above their health cap,
        // set their current hit points to their original cap
        var predictedHealth = State.HitPoints + amount;
        if (predictedHealth > _healthCap)
        {
            State = State with { HitPoints = _healthCap };
        }
        else
        {
            State = State with { HitPoints = predictedHealth };
        }
    }

    /// <summary>
    ///     Adds temporary hit points to a player, allowing damage to the player to
    ///     be mitigated before taking damage to their effective hit points.
    /// </summary>
    /// <param name="amount">Amount of temporary hit points to add.</param>
    public void AddTemporaryHitPoints(int amount)
    {
        var temporaryHitPoints = State.TemporaryHitPoints + amount;
        State = State with { TemporaryHitPoints = temporaryHitPoints };
    }

    /// <summary>
    ///     Constructs a player object from a JSON player template from the provided file path.
    /// </summary>
    /// <param name="filePath">Local filepath to the JSON file.</param>
    /// <param name="cancellationToken">Default cancellation context.</param>
    /// <returns>Constructed player object.</returns>
    /// <exception cref="VitalTrackException">Throws when the file path is not found.</exception>
    public static async Task<Player> FromTemplateAsync(
        string filePath,
        CancellationToken cancellationToken
    )
    {
        if (!Path.Exists(filePath))
        {
            throw new VitalTrackException(
                $"The file path {filePath} does not exist, player template cannot be loaded."
            );
        }

        var templateContents = await File.ReadAllTextAsync(filePath, cancellationToken);
        var playerTemplate = JsonSerializer.Deserialize<PlayerState>(
            templateContents,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        // Adjust any player stats based on their held items
        playerTemplate = AdjustStatsForApplicableItems(playerTemplate);

        return new Player(playerTemplate.HitPoints) { State = playerTemplate };
    }

    /// <summary>
    ///     Calculates the damage amount a player should take based on the damage type and the player's defenses.
    /// </summary>
    /// <param name="damageType">Damage type on the request.</param>
    /// <param name="originalDamageValue">Damage amount on the request.</param>
    /// <returns>Adjusted damage value.</returns>
    private int CalculateAdjustedDamageValue(string damageType, int originalDamageValue)
    {
        var defensibleDamage = State.Defenses.FirstOrDefault(d =>
            string.Equals(damageType, d.Type, StringComparison.CurrentCultureIgnoreCase)
        );

        // If no defense type is matched, we'll deal the full damage to the player
        if (defensibleDamage is null)
        {
            return originalDamageValue;
        }

        return defensibleDamage.Defense switch
        {
            // If the player is immune to the damage type, no damage is taken
            "immunity" => 0,
            // If the player has resistance to the damage, take the floor of half the damage (player advantage)
            "resistance"
                => Convert.ToInt32(Math.Floor(Convert.ToDecimal(originalDamageValue) / 2)),
            _ => originalDamageValue
        };
    }

    /// <summary>
    ///     Adjusts any player stats based on the currently held items based on the player template.
    ///     Note, this is NOT an idempotent state transition, so multiple calls will increase stats
    ///     beyond what they may have been intended to improve upon.
    /// </summary>
    /// <param name="playerState">Original player state.</param>
    /// <returns>Newly computed player state.</returns>
    private static PlayerState AdjustStatsForApplicableItems(PlayerState playerState)
    {
        // If we don't have any items, there's no adjustments to the player's current state
        if (!playerState.Items.Any())
        {
            return playerState;
        }

        // Roll through each item and compute the new state based on the item's affected modifiers
        foreach (var item in playerState.Items)
        {
            if (
                string.Equals(
                    item.Modifier.AffectedObject,
                    "stats",
                    StringComparison.CurrentCultureIgnoreCase
                )
            )
            {
                playerState = item.Modifier.AffectedValue switch
                {
                    // TODO: Only doing constitution for now, but should eventually be fleshed out for others
                    "constitution"
                        => playerState with
                        {
                            Stats = playerState.Stats with
                            {
                                Constitution = playerState.Stats.Constitution + item.Modifier.Value
                            }
                        },
                    _ => playerState
                };
            }
        }

        return playerState;
    }
}
