using System.Net;

namespace VitalTrack.Core;

public class VitalTrackHttpException : Exception
{
    public VitalTrackHttpException(string message, HttpStatusCode statusCode)
        : base(message) { }

    public HttpStatusCode StatusCode { get; }

    public int ConvertedStatusCode => (int)StatusCode;
}

#pragma warning disable CS9113 // Parameter is unread.
public class VitalTrackException(string Message) : Exception;
#pragma warning restore CS9113 // Parameter is unread.
