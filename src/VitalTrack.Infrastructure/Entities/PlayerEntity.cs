namespace VitalTrack.Infrastructure.Entities;

public record PlayerEntity : EntityBase
{
    public required string CharacterName { get; init; }

    public required int HitPoints { get; init; }

    public required int Level { get; init; }

    public required int Strength { get; init; }

    public required int Dexterity { get; init; }

    public required int Constitution { get; init; }

    public required int Intelligence { get; init; }

    public required int Wisdom { get; init; }

    public required int Charisma { get; init; }

    public required ICollection<PlayerDefenseEntity> Defenses { get; init; } =
        new List<PlayerDefenseEntity>();

    public required ICollection<PlayerItemEntity> Items { get; init; } =
        new List<PlayerItemEntity>();

    public required ICollection<PlayerClassEntity> Classes { get; init; } =
        new List<PlayerClassEntity>();
}
