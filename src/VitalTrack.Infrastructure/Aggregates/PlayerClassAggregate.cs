namespace VitalTrack.Infrastructure.Aggregates;

public record PlayerClassAggregate
{
    public required string ClassName { get; init; }

    public required int HitDiceValue { get; init; }

    public required int ClassLevel { get; init; }
}