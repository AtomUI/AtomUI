using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Converts Token values into the values a theme resource exposes. The generated <c>ProjectResourceValue</c>
/// members of Token classes call <see cref="Project(SolidColorBrush)" /> for a Token typed
/// <see cref="SolidColorBrush" /> and <see cref="Project{T}(T)" /> for every other type.
/// </summary>
public static class ThemeResourceValue
{
    /// <summary>
    /// Returns a new mutable <see cref="SolidColorBrush" /> with the color, opacity, transform and transform
    /// origin of the given brush, or <see langword="null" /> when the brush is <see langword="null" />.
    /// </summary>
    public static object? Project(SolidColorBrush? value)
    {
        return CloneSolidColorBrush(value);
    }

    /// <summary>
    /// Returns the value in the form a theme resource exposes: a <see cref="Color" /> becomes an
    /// <see cref="ImmutableSolidColorBrush" />, a brush becomes immutable, and every other value is returned
    /// unchanged, <see langword="null" /> included.
    /// </summary>
    /// <typeparam name="T">
    /// The declared type of the Token value; the conversion follows the runtime type.
    /// </typeparam>
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
