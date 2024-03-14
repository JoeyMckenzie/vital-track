using FluentAssertions;

namespace VitalTrack.Core.Tests.Players;

public partial class PlayerShould
{
    [Fact(DisplayName = "Verify player health is added when a heal is received.")]
    public void AddHealthToPool()
    {
        // Arrange, we'll deal some damage to our player template
        const int healingValue = 10;
        _player.State.HitPoints.Should().Be(25);
        _player.DealDamage(string.Empty, 15);
        _player.State.HitPoints.Should().Be(10);

        // Act
        _player.Heal(healingValue);

        // Assert
        _player.State.HitPoints.Should().Be(20);
    }

    [Fact(
        DisplayName = "Verify player health is capped to their original max HP value when overhealed (typical resto druids...)"
    )]
    public void ResetHealthToCapWhenOverhealed()
    {
        // Arrange, we'll deal some damage to our player template
        const int healingValue = 420;
        _player.State.HitPoints.Should().Be(25);
        _player.DealDamage(string.Empty, 15);
        _player.State.HitPoints.Should().Be(10);

        // Act
        _player.Heal(healingValue);

        // Assert
        _player.State.HitPoints.Should().Be(25);
    }
}
