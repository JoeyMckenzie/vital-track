using System.Net;
using VitalTrack.Core;
using VitalTrack.Core.Services;

namespace VitalTrack.Web.Concerns;

/// <summary>
///     An endpoint filter for asserting the player exists for specific API endpoints.
/// </summary>
public class ExistingPlayerFilter : IEndpointFilter
{
    private readonly IPlayerRepository _playerRepository;

    public ExistingPlayerFilter(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        // A bit of ASP.NET magic here, as long as we except the player name as the first
        // route argument, we'll be able to quickly verify the player exists on all routes
        var playerName = context.GetArgument<string>(0);
        var playerExists = await _playerRepository.PlayerExistsAsync(playerName);

        if (!playerExists)
        {
            throw new VitalTrackHttpException(
                $"Player {playerName} was not found.",
                HttpStatusCode.NotFound
            );
        }

        return await next(context);
    }
}
