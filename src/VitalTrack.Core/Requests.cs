namespace VitalTrack.Core;

/// <summary>
///     Represents an API request to health the player's current health.
/// </summary>
/// <param name="Amount"></param>
public readonly record struct HitPointModifierRequest(int? Amount, string? DamageType = null)
{
    public override string ToString()
    {
        return $"""
            {nameof(DamageType)}: {DamageType ?? "<null>"}
            {nameof(Amount)}: {Amount ?? 0}
            """;
    }
}
