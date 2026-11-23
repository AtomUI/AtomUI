using AtomUI.DesignLanguages.PrimeNG.Presets;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.PrimeNG;

/// <summary>Component values resolved from a PrimeUIX preset and the effective theme.</summary>
public readonly ref struct PrimeNGVisualRecipes
{
    private readonly PrimeNGToken _language;
    private readonly DesignToken _global;
    private readonly DesignVariant _variant;
    private readonly PrimeNGPreset _preset;

    internal PrimeNGVisualRecipes(PrimeNGToken language, DesignToken global, DesignVariant variant)
    {
        _language = language;
        _global = global;
        _variant = variant;
        _preset = PrimeNGPresets.Of(variant.Style);
    }

    private PrimeNGAppearanceRules Rules => PrimeNGPresets.For(_preset, _variant.Appearance, _language);

    /// <summary>PrimeUIX focus.ring.offset scaled by the effective line width.</summary>
    public double FocusOutlineOffset => _global.LineWidth * _preset.Alias.FocusRingOffsetLineWidths;

    /// <summary>Whether the effective primary resolves to the current appearance's noir primary.</summary>
    public bool IsNoirPrimary => PrimeNGNoir.IsNoirPrimary(Rules, _variant.Appearance, _global.ColorPrimary);

    public FontWeight ButtonFontWeight => _preset.Controls.ButtonFontWeight;
    public double? ButtonPaddingHorizontal => _preset.Controls.ButtonPaddingHorizontal?.Invoke(_global);
    public double CheckIndicatorSize => _preset.Controls.CheckIndicatorSize;
    public double CheckedMarkSize => _preset.Controls.CheckedMarkSize;
    public double DialogTitleFontSize => _preset.Controls.DialogHeaderFontSize;
    public double PopoverGutter => _preset.Controls.PopoverGutter;
    public Thickness ListOptionPadding => _preset.Controls.SelectOptionPadding(_global);
    public Thickness TooltipPadding => _preset.Controls.ToolTipPadding;
    public Color TooltipForeground => _preset.Controls.ToolTipColor.Resolve(Rules.Surface.AsSpan());
    public double CardBodyPadding => _preset.Controls.CardBodyPadding;
    public BoxShadows CardShadows => _preset.Controls.CardRestingShadows;
    public CornerRadius CardCornerRadius => new(_global.BorderRadius.TopLeft + _preset.Controls.CardBorderRadiusOffset);
    public Color? CardBorderColor
    {
        get
        {
            var rules = Rules;
            return rules.CardBorderSecondary?.Resolve(rules.Surface.AsSpan());
        }
    }

    public Color ProgressTrackBackground
    {
        get
        {
            var rules = Rules;
            return rules.ProgressTrack.Resolve(rules.Surface.AsSpan());
        }
    }

    public (Color Background, Color Foreground) NavigationItemActive
    {
        get
        {
            var rules = Rules;
            var surface = rules.Surface.AsSpan();
            return (rules.NavigationItemActive.Resolve(surface), rules.NavigationItemActiveColor.Resolve(surface));
        }
    }

    public PrimeNGToggleSwitchRecipe ToggleSwitch
    {
        get
        {
            var rules = Rules;
            var rule = rules.Switch;
            var surface = rules.Surface.AsSpan();
            return new PrimeNGToggleSwitchRecipe(rule.Track.Resolve(surface), rule.TrackHover.Resolve(surface),
                rule.Handle.Resolve(surface), rule.HandleHover.Resolve(surface), rule.HandleChecked.Resolve(surface),
                rule.TrackBorder?.Resolve(surface), rule.CheckedHoverKeepsPrimary);
        }
    }
}

/// <summary>PrimeUIX toggle switch track and handle colours.</summary>
public readonly record struct PrimeNGToggleSwitchRecipe(Color Track, Color TrackHover, Color Handle, Color HandleHover,
    Color CheckedHandle, Color? TrackBorder, bool CheckedHoverKeepsPrimary);

/// <summary>The fixed colours for the PrimeUIX message and toast severity roles.</summary>
public enum PrimeNGFeedbackSeverity
{
    Information,
    Success,
    Warning,
    Error
}

/// <summary>A read-only colour scale for a PrimeUIX status or feedback role.</summary>
public readonly ref struct PrimeNGSeverityScale
{
    private readonly ReadOnlySpan<Color> _colors;

    internal PrimeNGSeverityScale(ReadOnlySpan<Color> colors)
    {
        _colors = colors;
    }

    public Color this[int stop] => _colors[PrimeNGPalette.IndexOf(stop)];

    public Color AtOpacity(int stop, double opacity) => PrimeNGPalette.WithOpacity(this[stop], opacity);
}
