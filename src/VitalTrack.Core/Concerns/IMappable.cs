namespace VitalTrack.Core.Concerns;

/// <summary>
///     Allows a model to map into another model type, useful for transitioning
///     between anemic models into their rich domain equivalents.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IMappableInto<out T>
{
    T Into();
}