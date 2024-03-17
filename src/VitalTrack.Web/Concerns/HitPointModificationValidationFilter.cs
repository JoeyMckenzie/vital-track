using System.Net;

using VitalTrack.Core.Domain;

namespace VitalTrack.Web.Concerns;

/// <summary>
///     An endpoint filter for asserting valid hit point modification values. In a more real world context, I'd most
///     likely use FluentValidation as request models tend to be complex aggregates of many things. Our use case
///     here is simple enough that a small endpoint filter will do the trick for now.
/// </summary>
public class HitPointModificationValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        // A bit of ASP.NET magic here, as long as we except the hit point modification request as the second
        // route argument, we'll be able to quickly verify the request contains a valid whole number
        var hitPointModificationRequest = context.GetArgument<HitPointModifierRequest>(1);

        if (hitPointModificationRequest.Amount < 0)
        {
            throw new VitalTrackHttpException(
                "Hit modification values must be greater than 0.",
                HttpStatusCode.BadRequest
            );
        }

        return await next(context);
    }
}