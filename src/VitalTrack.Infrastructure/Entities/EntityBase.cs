namespace VitalTrack.Infrastructure.Entities;

public record EntityBase
{
    public required int Id { get; init; }

    public DateTime? CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
