using AtomUI.DesignLanguages.PrimeNG.Presets;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.PrimeNG;

/// <summary>
/// Component values resolved from a PrimeUIX preset and the effective theme.
/// </summary>
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

    /// <summary>
    /// PrimeUIX focus.ring.offset scaled by the effective line width.
    /// </summary>
    public double FocusOutlineOffset => _global.LineWidth * _preset.Alias.FocusRingOffsetLineWidths;

    /// <summary>
    /// Whether the effective primary resolves to the current appearance's noir primary.
    /// </summary>
    public bool IsNoirPrimary => PrimeNGNoir.IsNoirPrimary(Rules, _variant.Appearance, _global.ColorPrimary);

    /// <summary>
    /// The font weight of a standard button's label.
    /// </summary>
    public FontWeight ButtonFontWeight => _preset.Controls.ButtonFontWeight;
    /// <summary>
    /// The horizontal padding around a button's label where the preset differs from the field padding, evaluated from
    /// the effective global tokens; <see langword="null" /> leaves the button on the horizontal <c>InputPadding</c>.
    /// </summary>
    public double? ButtonPaddingHorizontal => _preset.Controls.ButtonPaddingHorizontal?.Invoke(_global);
    /// <summary>
    /// The side length of the checkbox indicator (PrimeUIX <c>checkbox.width</c>).
    /// </summary>
    public double CheckIndicatorSize => _preset.Controls.CheckIndicatorSize;
    /// <summary>
    /// The size of the mark inside a checked checkbox (PrimeUIX <c>checkbox.icon.size</c>).
    /// </summary>
    public double CheckedMarkSize => _preset.Controls.CheckedMarkSize;
    /// <summary>
    /// The font size of a dialog's title (PrimeUIX <c>dialog.title.font.size</c>).
    /// </summary>
    public double DialogTitleFontSize => _preset.Controls.DialogHeaderFontSize;
    /// <summary>
    /// The distance between the anchor and an arrow flyout's box, which the arrow spans (PrimeUIX
    /// <c>popover.gutter</c>). The arrow is twice as wide as long, so its size is twice the gutter.
    /// </summary>
    public double PopoverGutter => _preset.Controls.PopoverGutter;
    /// <summary>
    /// The padding of an option row in dropdown lists such as Select and ComboBox, and of the group headers above
    /// such rows, evaluated from the effective global tokens.
    /// </summary>
    public Thickness ListOptionPadding => _preset.Controls.SelectOptionPadding(_global);
    /// <summary>
    /// The padding around a ToolTip's content.
    /// </summary>
    public Thickness TooltipPadding => _preset.Controls.ToolTipPadding;
    /// <summary>
    /// The ToolTip's text color: the preset's stop of the surface scale, resolved for the current appearance.
    /// </summary>
    public Color TooltipForeground => _preset.Controls.ToolTipColor.Resolve(Rules.Surface.AsSpan());
    /// <summary>
    /// The padding of a card's body on all four sides.
    /// </summary>
    public double CardBodyPadding => _preset.Controls.CardBodyPadding;
    /// <summary>
    /// The shadow of a card at rest. Despite its name it feeds the card's <c>RestingShadows</c> token; the
    /// <c>CardShadows</c> token stays the hover shadow.
    /// </summary>
    public BoxShadows CardShadows => _preset.Controls.CardRestingShadows;
    /// <summary>
    /// The corner radius of a card on all four corners: the base radius <c>BorderRadius</c> plus the preset's
    /// offset.
    /// </summary>
    public CornerRadius CardCornerRadius => new(_global.BorderRadius.TopLeft + _preset.Controls.CardBorderRadiusOffset);
    /// <summary>
    /// The card's own border color, a stop of the surface scale resolved for the current appearance;
    /// <see langword="null" /> leaves the card on the global <c>ColorBorderSecondary</c>.
    /// </summary>
    public Color? CardBorderColor
    {
        get
        {
            var rules = Rules;
            return rules.CardBorderSecondary?.Resolve(rules.Surface.AsSpan());
        }
    }

    /// <summary>
    /// The color of the unfilled part of a progress track (PrimeUIX <c>progressbar.background</c>), a stop of the
    /// surface scale resolved for the current appearance.
    /// </summary>
    public Color ProgressTrackBackground
    {
        get
        {
            var rules = Rules;
            return rules.ProgressTrack.Resolve(rules.Surface.AsSpan());
        }
    }

    /// <summary>
    /// The background and text colors of an active navigation item (PrimeUIX
    /// <c>navigation.item.active.background</c> and <c>navigation.item.active.color</c>), stops of the surface scale
    /// resolved for the current appearance. Menus use them for an open submenu's path; the NavMenu for its selected,
    /// hovered and keyboard-active items.
    /// </summary>
    public (Color Background, Color Foreground) NavigationItemActive
    {
        get
        {
            var rules = Rules;
            var surface = rules.Surface.AsSpan();
            return (rules.NavigationItemActive.Resolve(surface), rules.NavigationItemActiveColor.Resolve(surface));
        }
    }

    /// <summary>
    /// The ToggleSwitch's track and handle colors (PrimeUIX <c>toggleswitch</c>) for the current appearance, with
    /// every shade resolved against the surface.
    /// </summary>
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

/// <summary>
/// PrimeUIX toggle switch track and handle colors.
/// </summary>
public readonly record struct PrimeNGToggleSwitchRecipe(Color Track, Color TrackHover, Color Handle, Color HandleHover,
    Color CheckedHandle, Color? TrackBorder, bool CheckedHoverKeepsPrimary);

/// <summary>
/// The fixed colors for the PrimeUIX message and toast severity roles.
/// </summary>
public enum PrimeNGFeedbackSeverity
{
    /// <summary>
    /// The information severity; its feedback scale is the blue palette.
    /// </summary>
    Information,
    /// <summary>
    /// The success severity; its feedback scale is the green palette.
    /// </summary>
    Success,
    /// <summary>
    /// The warning severity; its feedback scale is the yellow palette.
    /// </summary>
    Warning,
    /// <summary>
    /// The error severity; its feedback scale is the red palette.
    /// </summary>
    Error
}

/// <summary>
/// A read-only color scale for a PrimeUIX status or feedback role.
/// </summary>
public readonly ref struct PrimeNGSeverityScale
{
    private readonly ReadOnlySpan<Color> _colors;

    /// <summary>
    /// Wraps a scale laid out by palette stop, without copying it.
    /// </summary>
    /// <param name="colors">
    /// One color per palette stop, in the order of the palette's stop table.
    /// </param>
    internal PrimeNGSeverityScale(ReadOnlySpan<Color> colors)
    {
        _colors = colors;
    }

    /// <summary>
    /// Gets the color of the scale at a palette stop.
    /// </summary>
    /// <param name="stop">
    /// A stop number of the PrimeNG palette table (50 to 950).
    /// </param>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown when <paramref name="stop" /> is not a stop number of the palette table.
    /// </exception>
    public Color this[int stop] => _colors[PrimeNGPalette.IndexOf(stop)];

    /// <summary>
    /// Gets the color of the scale at a palette stop with an opacity applied on top of the stop's own alpha, as
    /// PrimeUIX <c>color-mix</c> with transparent does.
    /// </summary>
    /// <param name="stop">
    /// A stop number of the PrimeNG palette table (50 to 950).
    /// </param>
    /// <param name="opacity">
    /// The factor the stop's own alpha is multiplied with: 1 keeps it, 0 makes the color fully transparent.
    /// </param>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown when <paramref name="stop" /> is not a stop number of the palette table.
    /// </exception>
    public Color AtOpacity(int stop, double opacity) => PrimeNGPalette.WithOpacity(this[stop], opacity);
}
