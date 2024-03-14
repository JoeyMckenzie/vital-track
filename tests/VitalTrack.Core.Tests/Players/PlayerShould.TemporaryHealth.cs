using FluentAssertions;

namespace VitalTrack.Core.Tests.Players;

public partial class PlayerShould
{
    [Fact(DisplayName = "Verify temporary hit points can be added to a player.")]
    public void AddTemporaryHitPoints()
    {
        // Arrange
        const int temporaryHitPoints = 10;
        _player.State.TemporaryHitPoints.Should().Be(0);

        // Act
        _player.AddTemporaryHitPoints(temporaryHitPoints);

        // Assert
        _player.State.TemporaryHitPoints.Should().Be(10);
    }
}
