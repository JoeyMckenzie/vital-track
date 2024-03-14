using FluentAssertions;

namespace VitalTrack.Core.Tests.Players;

public partial class PlayerShould
{
    [Fact(DisplayName = "Verify hit points are removed when a player takes non-lethal damage.")]
    public void RemovesHitPointsWhenDamageIsTaken()
    {
        // Arrange
        const int damageValue = 7;
        _player.State.HitPoints.Should().Be(25);

        // Act
        _player.DealDamage(string.Empty, damageValue);

        // Assert
        _player.State.HitPoints.Should().Be(18);
    }

    [Fact(
        DisplayName = "Verify hit points are zero when damage taken is greater than the player's hit point value."
    )]
    public void RemovesAllHitPointsOnLethalDamage()
    {
        // Arrange
        const int damageValue = 420;
        _player.State.HitPoints.Should().Be(25);

        // Act
        _player.DealDamage(string.Empty, damageValue);

        // Assert
        _player.State.HitPoints.Should().Be(0);
    }

    [Fact(
        DisplayName = "Verify hit points are not deducted when temporary health mitigates all the incoming damage."
    )]
    public void TemporaryHitPointsMitigateAllIncomingDamage()
    {
        // Arrange
        const int damageValue = 10;
        _player.State.HitPoints.Should().Be(25);
        _player.State.TemporaryHitPoints.Should().Be(0);
        _player.AddTemporaryHitPoints(15);
        _player.State.TemporaryHitPoints.Should().Be(15);

        // Act, we'll first add some temporary hit points
        _player.DealDamage(string.Empty, damageValue);

        // Assert
        _player.State.HitPoints.Should().Be(25);
        _player.State.TemporaryHitPoints.Should().Be(5);
    }

    [Fact(
        DisplayName = "Verify hit points are deducted when temporary health cannot mitigate all incoming damage."
    )]
    public void HitPointsAreDeductedWhenTemporaryHealthIsDepleted()
    {
        // Arrange
        const int damageValue = 10;
        _player.State.HitPoints.Should().Be(25);
        _player.State.TemporaryHitPoints.Should().Be(0);
        _player.AddTemporaryHitPoints(5);
        _player.State.TemporaryHitPoints.Should().Be(5);

        // Act, we'll first add some temporary hit points
        _player.DealDamage(string.Empty, damageValue);

        // Assert, we'll verify all 5 temp HPs are depleted along with 5 HPs taken from the player's health pool
        _player.State.HitPoints.Should().Be(20);
        _player.State.TemporaryHitPoints.Should().Be(0);
    }

    [Fact(
        DisplayName = "Verify hit points and temporary health depleted when incoming damage is more than both pools."
    )]
    public void HitPointsAndTemporaryHealthAreDepleted()
    {
        // Arrange
        const int damageValue = 420;
        _player.State.HitPoints.Should().Be(25);
        _player.State.TemporaryHitPoints.Should().Be(0);
        _player.AddTemporaryHitPoints(10);
        _player.State.TemporaryHitPoints.Should().Be(10);

        // Act, we'll first add some temporary hit points
        _player.DealDamage(string.Empty, damageValue);

        // Assert, we'll verify all 5 temp HPs are depleted along with 5 HPs taken from the player's health pool
        _player.State.HitPoints.Should().Be(0);
        _player.State.TemporaryHitPoints.Should().Be(0);
    }

    [Fact(
        DisplayName = "Verify only half damage is taken when the player has resistance to the damage type."
    )]
    public void TakesHalfDamageWhenThePlayerHasResistance()
    {
        // Arrange
        const int damageValue = 12;
        _player.State.HitPoints.Should().Be(25);

        // Act
        _player.DealDamage("slashing", damageValue);

        // Assert, verify the player only takes half the damage value (6 in this case)
        _player.State.HitPoints.Should().Be(19);
    }

    [Fact(
        DisplayName = "Verify the floor of half damage is taken when the player has resistance to the damage type."
    )]
    public void TakesFlooredHalfDamageWhenThePlayerHasResistance()
    {
        // Arrange
        const int damageValue = 15;
        _player.State.HitPoints.Should().Be(25);

        // Act
        _player.DealDamage("slashing", damageValue);

        // Assert, verify the player only floored takes half the damage value (7.5 with 7 floored damage in this case)
        _player.State.HitPoints.Should().Be(18);
    }

    [Fact(DisplayName = "Verify no damage is taken when the player is immune to the damage type.")]
    public void TakesNoDamageWhenThePlayerIsImmune()
    {
        // Arrange
        const int damageValue = 15;
        _player.State.HitPoints.Should().Be(25);

        // Act
        _player.DealDamage("fire", damageValue);

        // Assert
        _player.State.HitPoints.Should().Be(25);
    }
}
