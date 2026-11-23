using AtomUI.Theme.Languages;

namespace AtomUI.Theme;

/// <summary>The theme the root scope currently shows: the theme, its language and the variant it was derived for.</summary>
public sealed record ThemeState
{
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

    public string ThemeId { get; }

    /// <summary>The design language identity.</summary>
    public string Language { get; }

    /// <summary>The style, appearance, density and idiom the root scope was derived for.</summary>
    public DesignVariant Variant { get; }

    public ThemeAppearance Appearance => Variant.Appearance;

    public ulong Fingerprint { get; }
    public long TransitionId { get; }
}
