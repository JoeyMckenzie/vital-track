using Microsoft.AspNetCore.Mvc;
using VitalTrack.Core;
using VitalTrack.Core.Services;

namespace VitalTrack.Web;

/// <summary>
///     Represents the various hit point operations a client may take on a player.
/// </summary>
public static class Endpoints
{
    /// <summary>
    ///     Represents an endpoint for dealing damage to a player's current health.
    /// </summary>
    /// <param name="playerName">Name of the player.</param>
    /// <param name="request">HP modifier request.</param>
    /// <param name="hitPointManager">HP manager, pulled from the container.</param>
    /// <param name="cancellationToken">Default ASP.NET cancellation context.</param>
    /// <returns>OK, if the damage was successfully dealt.</returns>
    private static async Task<IResult> DealDamageAsync(
        [FromRoute] string playerName,
        [FromBody] HitPointModifierRequest request,
        IHitPointManager hitPointManager,
        CancellationToken cancellationToken
    )
    {
        var response = await hitPointManager.DealDamageAsync(
            playerName,
            request,
            cancellationToken
        );

        return Results.Ok(response);
    }

    /// <summary>
    ///     Provides an operation for healing the current health the player.
    /// </summary>
    /// <param name="playerName">Name of the player.</param>
    /// <param name="request">HP modifier request.</param>
    /// <param name="hitPointManager">HP manager, pulled from the container.</param>
    /// <param name="cancellationToken">Default ASP.NET cancellation context.</param>
    /// <returns>OK, if the player was successfully healed.</returns>
    private static async Task<IResult> HealCurrentHealthAsync(
        [FromRoute] string playerName,
        [FromBody] HitPointModifierRequest request,
        IHitPointManager hitPointManager,
        CancellationToken cancellationToken
    )
    {
        var response = await hitPointManager.HealHitPointsAsync(
            playerName,
            request.Amount ?? 0,
            cancellationToken
        );

        return Results.Ok(response);
    }

    /// <summary>
    ///     Provides an operation for adding temporary hit points to the player's health.
    /// </summary>
    /// <param name="playerName">Name of the player.</param>
    /// <param name="request">HP modifier request.</param>
    /// <param name="hitPointManager">HP manager, pulled from the container.</param>
    /// <param name="cancellationToken">Default ASP.NET cancellation context.</param>
    /// <returns>OK, if the temporary health was successfully added to the player.</returns>
    private static async Task<IResult> AddTemporaryHealth(
        [FromRoute] string playerName,
        [FromBody] HitPointModifierRequest request,
        IHitPointManager hitPointManager,
        CancellationToken cancellationToken
    )
    {
        var response = await hitPointManager.AddTemporaryHitPoints(
            playerName,
            request.Amount ?? 0,
            cancellationToken
        );

        return Results.Ok(response);
    }

    /// <summary>
    ///     Provides an endpoint for retrieving the current health status of the player.
    /// </summary>
    /// <param name="playerName">Name of the player.</param>
    /// <param name="playerRepository">Player repository, pulled from the container.</param>
    /// <param name="cancellationToken">Default ASP.NET cancellation context.</param>
    /// <returns>OK, if the player information was successfully found.</returns>
    private static async Task<IResult> GetCurrentPlayerInfo(
        [FromRoute] string playerName,
        IPlayerRepository playerRepository,
        CancellationToken cancellationToken
    )
    {
        var player = await playerRepository.FindPlayerAsync(playerName, cancellationToken);

        // We can assert non-nullability here as the endpoint filter validates the player exists
        return Results.Ok(player!.State);
    }

    /// <summary>
    ///     Consolidates the addition of the Vital Track routes into a single route group.
    /// </summary>
    /// <param name="group">Current route grouping context.</param>
    public static RouteGroupBuilder MapVitalTrackRoutes(this RouteGroupBuilder group)
    {
        group
            .MapGet("info", GetCurrentPlayerInfo)
            .WithName("Retrieve current health information for the player")
            .WithOpenApi();

        group
            .MapPost("damage", DealDamageAsync)
            .WithName("Deal damage to player's health")
            .WithOpenApi();

        group
            .MapPost("heal", HealCurrentHealthAsync)
            .WithName("Heal a player's current health")
            .WithOpenApi();

        group
            .MapPost("temp", AddTemporaryHealth)
            .WithName("Add temporary health to a player's hit points")
            .WithOpenApi();

        return group;
    }
}
