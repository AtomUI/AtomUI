using Avalonia;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Adds setters to a <see cref="StyleBase" /> from code. Every method returns the style it was called on,
/// so calls can be chained.
/// </summary>
public static class StyleExtensions
{
    /// <summary>
    /// Adds a setter that assigns <paramref name="value" /> to <paramref name="targetProperty" /> on every
    /// element the style matches.
    /// </summary>
    /// <param name="style">
    /// The style that receives the setter.
    /// </param>
    /// <param name="targetProperty">
    /// The property the setter assigns.
    /// </param>
    /// <param name="value">
    /// The value assigned to the property.
    /// </param>
    /// <returns>
    /// The <paramref name="style" /> that was passed in.
    /// </returns>
    public static StyleBase Add(this StyleBase style, AvaloniaProperty targetProperty, object? value)
    {
        style.Add(new Setter(targetProperty, value));
        return style;
    }

    /// <summary>
    /// Adds a setter that assigns to <paramref name="targetProperty" /> an object produced by
    /// <paramref name="valueFactory" /> for every element the style matches, so that the elements do not share
    /// one value object.
    /// </summary>
    /// <typeparam name="T">
    /// The type of object the factory produces.
    /// </typeparam>
    /// <param name="style">
    /// The style that receives the setter.
    /// </param>
    /// <param name="targetProperty">
    /// The property the setter assigns.
    /// </param>
    /// <param name="valueFactory">
    /// Called once for each element the setter is applied to.
    /// </param>
    /// <returns>
    /// The <paramref name="style" /> that was passed in.
    /// </returns>
    public static StyleBase Add<T>(this StyleBase style, AvaloniaProperty targetProperty, Func<T?> valueFactory)
        where T : class
    {
        style.Add(new Setter(targetProperty, new SetterValueFactory<T>(valueFactory)));
        return style;
    }

    /// <summary>
    /// Adds a setter that reads the resource keyed by <paramref name="resourceKey" /> as a dynamic resource,
    /// so that <paramref name="targetProperty" /> follows resource changes.
    /// </summary>
    /// <typeparam name="TTokenKind">
    /// The enum type of the Token kinds.
    /// </typeparam>
    /// <param name="style">
    /// The style that receives the setter.
    /// </param>
    /// <param name="targetProperty">
    /// The property the setter assigns.
    /// </param>
    /// <param name="resourceKey">
    /// The Token kind whose enum value is the resource key.
    /// </param>
    /// <returns>
    /// The <paramref name="style" /> that was passed in.
    /// </returns>
    public static StyleBase Add<TTokenKind>(this StyleBase style, AvaloniaProperty targetProperty, TTokenKind resourceKey)
        where TTokenKind : Enum
    {
        style.Add(new Setter(targetProperty, new DynamicResourceExtension(resourceKey)));
        return style;
    }
}