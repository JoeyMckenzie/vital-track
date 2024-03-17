namespace VitalTrack.Core.Concerns;

public interface IMappableInto<out T>
{
    T Into();
}

public interface IMappableFrom<out T>
{
    T From();
}
