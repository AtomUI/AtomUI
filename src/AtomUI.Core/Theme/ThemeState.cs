using AtomUI.Theme.Languages;

namespace AtomUI.Theme;

/// <summary>
/// The theme the root scope currently shows: the theme, its language and the variant it was derived for.
/// </summary>
public sealed record ThemeState
{
    /// <summary>
    /// Creates the state from values that initialize the properties of the same names.
    /// <paramref name="themeId" /> and <paramref name="language" /> must not be blank.
    /// </summary>
    public ThemeState(
        string themeId,
        string language,
        DesignVariant variant,
        ulong fingerprint,
        long transitionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        ThemeId      = themeId;
        Language     = language;
        Variant      = variant;
        Fingerprint  = fingerprint;
        TransitionId = transitionId;
    }

    /// <summary>
    /// The identity of the theme the root scope shows.
    /// </summary>
    public string ThemeId { get; }

    /// <summary>
    /// The design language identity.
    /// </summary>
    public string Language { get; }

    /// <summary>
    /// The style, appearance, density and idiom the root scope was derived for.
    /// </summary>
    public DesignVariant Variant { get; }

    /// <summary>
    /// The appearance the root scope shows; the same value as <see cref="DesignVariant.Appearance" /> of
    /// <see cref="Variant" />.
    /// </summary>
    public ThemeAppearance Appearance => Variant.Appearance;

    /// <summary>
    /// A hash of the content the theme was compiled from; states of the same theme differ here when their
    /// content differs.
    /// </summary>
    public ulong Fingerprint { get; }
    /// <summary>
    /// The identifier of the transition that produced this state; equal values mean the same transition.
    /// </summary>
    public long TransitionId { get; }
}
