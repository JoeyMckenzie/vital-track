namespace VitalTrack.Core.Concerns;

/// <summary>
///     A loggable representation inheritors can provide for their callers.
/// </summary>
public interface ILoggable
{
    /// <summary>
    ///     Builds the log-friendly stringified message of the class.
    /// </summary>
    string IntoLog();
}
