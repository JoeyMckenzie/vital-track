namespace VitalTrack.Infrastructure.Aggregates;

/// <summary>
///     The player class aggregate is the combination of class and player class pivot table data.
/// </summary>
public record PlayerClassAggregate
{
    public required string ClassName { get; init; }

    public required int HitDiceValue { get; init; }

    public required int ClassLevel { get; init; }
}
