using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Localization;

/// <summary>
/// A text that is resolved on demand. The Gallery navigation menu accepts it as a node header and calls
/// <see cref="Resolve" /> when it builds its nodes.
/// </summary>
public interface IGalleryLocalizedText
{
    /// <summary>
    /// Returns the text to display. The result is produced on each call from the state of the running
    /// application, so call it when the text is shown rather than while the Gallery is being configured.
    /// </summary>
    /// <returns>
    /// The text to display.
    /// </returns>
    object Resolve();
}

/// <summary>
/// A localized text for one value of a language resource kind enum: it is looked up through the
/// localizer of the application, with a fallback text for when no localizer is available.
/// </summary>
/// <typeparam name="TResourceKind">
/// The enum type whose values name the language resources.
/// </typeparam>
public sealed class GalleryLocalizedText<TResourceKind> : IGalleryLocalizedText
    where TResourceKind : struct, Enum
{
    /// <summary>
    /// The language resource that <see cref="Resolve" /> asks the localizer for.
    /// </summary>
    public TResourceKind ResourceKind { get; }

    /// <summary>
    /// The text that <see cref="Resolve" /> returns when there is no current application or it has no
    /// localizer.
    /// </summary>
    public string Fallback { get; }

    /// <summary>
    /// Creates a localized text for one language resource.
    /// </summary>
    /// <param name="resourceKind">
    /// The language resource to look up.
    /// </param>
    /// <param name="fallback">
    /// The text to use when no localizer is available.
    /// </param>
    public GalleryLocalizedText(TResourceKind resourceKind, string fallback)
    {
        ResourceKind = resourceKind;
        Fallback     = fallback;
    }

    /// <summary>
    /// Asks the localizer of the current application for the text of <see cref="ResourceKind" />.
    /// </summary>
    /// <returns>
    /// The text from the localizer, or <see cref="Fallback" /> when there is no current application or it
    /// has no localizer.
    /// </returns>
    public object Resolve()
    {
        if (Application.Current is { } application &&
            global::AtomUI.ApplicationExtensions.GetLocalizer(application) is { } localizer)
        {
            return localizer.Get(ResourceKind);
        }

        return Fallback;
    }

    /// <summary>
    /// The text from <see cref="Resolve" /> as a string, or an empty string when it has no string form.
    /// </summary>
    public override string ToString()
    {
        return Resolve().ToString() ?? string.Empty;
    }
}
