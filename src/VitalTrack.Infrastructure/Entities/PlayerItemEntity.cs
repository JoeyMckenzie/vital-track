namespace VitalTrack.Infrastructure.Entities;

public record PlayerItemEntity : EntityBase
{
    public required int PlayerId { get; init; }

    public required int ItemId { get; init; }
}