using System.Net;
using System.Text.Json.Serialization;

namespace VitalTrack.Core.Domain;

/// <summary>
///     Represents an API response returning data about the action taken
/// </summary>
/// <param name="StatusCode">HTTP status code of the response.</param>
/// <typeparam name="T">Data type of the serialized response.</typeparam>
public readonly record struct VitalTrackResponse<T>(
    T Data,
    [property: JsonIgnore] HttpStatusCode StatusCode = HttpStatusCode.OK,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Message = null
);
