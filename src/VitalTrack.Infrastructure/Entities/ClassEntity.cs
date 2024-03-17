namespace VitalTrack.Infrastructure.Entities;

public record ClassEntity : EntityBase
{
    public required string ClassName { get; init; }
}