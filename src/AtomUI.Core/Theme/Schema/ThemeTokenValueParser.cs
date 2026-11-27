using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Parses the literal text of Token values back into values of their type with the registered value converters.
/// </summary>
public static class ThemeTokenValueParser
{
    private static readonly IReadOnlyDictionary<Type, ITokenValueConverter> s_converters =
        TokenValueConverterRegistry.Create();

    /// <summary>
    /// Parses the literal text of a Token value of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">
    /// The Token value type. A converter must be registered for it.
    /// </typeparam>
    /// <param name="value">
    /// The literal text to parse.
    /// </param>
    /// <returns>
    /// The parsed value, never <see langword="null" />.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// No converter is registered for <typeparamref name="T" />, or the converter returned a value that is not a
    /// <typeparamref name="T" />.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    public static T Parse<T>(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!s_converters.TryGetValue(typeof(T), out var converter))
        {
            throw new InvalidOperationException(
                $"No theme Token value converter is registered for '{typeof(T).FullName}'.");
        }

        var converted = converter.Convert(value);
        if (converted is T typed)
        {
            return typed;
        }

        throw new InvalidOperationException(
            $"Theme Token value converter for '{typeof(T).FullName}' returned an incompatible value.");
    }
}
