using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Theme.Schema;

public static class ThemeResourceValue
{
    public static object? Project(SolidColorBrush? value)
    {
        return CloneSolidColorBrush(value);
    }

    public static object? Project<T>(T value)
    {
        return value switch
        {
            Color color  => new ImmutableSolidColorBrush(color),
            IBrush brush => brush.ToImmutable(),
            _            => value
        };
    }

    internal static object? CloneForConsumer(object? value)
    {
        // Ownership boundaries already copied BoxShadows' private backing array. Its public
        // indexer/enumerator return value copies, so reading the owned collection stays allocation-free.
        if (value is BoxShadows)
        {
            return value;
        }
        return TokenValueOperations.TryCopy(value, out var copy) ? copy : value;
    }

    internal static object? Freeze(object? value) =>
        TokenValueOperations.TryCopy(value, out var copy) ? copy : value;

    internal static SolidColorBrush? CloneSolidColorBrush(SolidColorBrush? value)
    {
        if (value is null)
        {
            return null;
        }

        return new SolidColorBrush(value.Color, value.Opacity)
        {
            Transform = value.Transform?.ToImmutable(),
            TransformOrigin = value.TransformOrigin
        };
    }
}
