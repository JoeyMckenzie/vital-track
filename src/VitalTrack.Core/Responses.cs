using System.Net;

namespace VitalTrack.Core;

/// <summary>
///     Represents an API response returning data about the action taken
/// </summary>
/// <param name="StatusCode">HTTP status code of the response.</param>
/// <typeparam name="T">Data type of the serialized response.</typeparam>
public readonly record struct VitalTrackCoreResponse<T>(
    T Data,
    HttpStatusCode StatusCode,
    string Message = "The operation was successful."
);
