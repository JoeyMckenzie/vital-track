namespace VitalTrack.Infrastructure.Entities;

public record ItemEntity : EntityBase
{
    public required string ItemName { get; init; }

    public required string AffectedObject { get; init; }

    public required string AffectedValue { get; init; }

    public required int Value { get; init; }
}