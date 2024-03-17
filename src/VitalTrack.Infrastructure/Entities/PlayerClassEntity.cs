namespace VitalTrack.Infrastructure.Entities;

public record PlayerClassEntity : EntityBase
{
    public required int PlayerId { get; init; }

    public required int ClassId { get; init; }

    public required int HitDiceValue { get; init; }

    public required int ClassLevel { get; init; }
}