using FluentAssertions;
using VitalTrack.Core.Models;

namespace VitalTrack.Core.Tests.Players;

public partial class PlayerShould
{
    private readonly Player _player;

    public PlayerShould()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var playerTemplatePath = $"{currentDirectory}{Path.DirectorySeparatorChar}briv.json";
        _player = Player
            .FromTemplateAsync(playerTemplatePath, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    [Fact(
        DisplayName = "Verify a player's internal state is correctly initialized from a template."
    )]
    public void ProperlyInitializesPlayerFromTemplate()
    {
        // Nothing to arrange/act on, only assert by spot checking a few things
        _player.State.Name.Should().Be("Briv");
        _player.State.Level.Should().Be(5);
        _player.State.HitPoints.Should().Be(25);
        _player.State.Classes.Should().NotBeEmpty();
        _player.State.Classes.Should().HaveCount(1);
        _player.State.Classes.First().HitDiceValue.Should().Be(10);
        _player.State.Classes.First().ClassLevel.Should().Be(5);
        _player.State.Stats.Strength.Should().Be(15);
        _player.State.Stats.Wisdom.Should().Be(10);
        _player.State.Stats.Charisma.Should().Be(8);
    }

    [Fact(
        DisplayName = "Verify a player's stats are affected when carrying an item that increases a stat."
    )]
    public void AdjustsStatsAccordinglyWhenCarryingItem()
    {
        // Verify the player instantiated from the template has 16 constitution (14 base, 2 with item)
        _player.State.Stats.Constitution.Should().Be(16);
    }
}
