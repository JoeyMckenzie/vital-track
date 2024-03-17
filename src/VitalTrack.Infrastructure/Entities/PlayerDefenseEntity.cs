namespace VitalTrack.Infrastructure.Entities;

public record PlayerDefenseEntity : EntityBase
{
    public required int PlayerId { get; init; }

    public required string Type { get; init; }

    public required string Defense { get; init; }
}