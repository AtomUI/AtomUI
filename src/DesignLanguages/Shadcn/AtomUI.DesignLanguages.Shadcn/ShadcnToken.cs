using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.Shadcn;

/// <summary>
/// The shadcn vocabulary the global tokens have no place for: the base-color variables of the current appearance,
/// the resolved theme primary, the chart palette, and the geometry and overlay tables of the current style. The
/// control token rules read a value here only when the global tokens have no place for it.
/// </summary>
/// <remarks>
/// The neutral base color is the language seed <see cref="BaseColor" /> (the light <c>--muted-foreground</c>,
/// i.e. the 500 stop of one neutral family); the theme primary is the global <c>ColorPrimary</c> seed (the light
/// <c>--primary</c>).
/// Percent strengths are written as percents (<c>10</c> is <c>#1A</c>).
/// </remarks>
[LanguageDesignToken]
public partial class ShadcnToken : LanguageDesignToken
{
    /// <summary>
    /// The base color (ui.shadcn.com/create Base Color): the 500 stop of the neutral family the surfaces, borders and
    /// muted text derive from (Neutral #737373, Stone, Zinc, Mauve, Olive, Mist, Taupe). An adjustable seed.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial Color BaseColor { get; set; }

    #region Base colour variables

    /// <summary>
    /// <c>--background</c>: light white, dark stop 950.
    /// </summary>
    public partial Color Background { get; set; }

    /// <summary>
    /// <c>--foreground</c>: light stop 950, dark stop 50.
    /// </summary>
    public partial Color Foreground { get; set; }

    /// <summary>
    /// <c>--card</c>: light white, dark stop 900.
    /// </summary>
    public partial Color Card { get; set; }

    /// <summary>
    /// <c>--popover</c>: light white, dark stop 900.
    /// </summary>
    public partial Color Popover { get; set; }

    /// <summary>
    /// The base color's <c>--primary</c>: light stop 900, dark stop 200.
    /// </summary>
    public partial Color Primary { get; set; }

    /// <summary>
    /// The base color's <c>--primary-foreground</c>: light stop 50, dark stop 900.
    /// </summary>
    public partial Color PrimaryForeground { get; set; }

    /// <summary>
    /// <c>--secondary</c>: stop 100 / 800.
    /// </summary>
    public partial Color Secondary { get; set; }

    /// <summary>
    /// <c>--secondary-foreground</c>: stop 900 / 50.
    /// </summary>
    public partial Color SecondaryForeground { get; set; }

    /// <summary>
    /// <c>--muted</c>: stop 100 / 800.
    /// </summary>
    public partial Color Muted { get; set; }

    /// <summary>
    /// <c>--muted-foreground</c>: stop 500 / 400; the light value is the <see cref="BaseColor" /> seed.
    /// </summary>
    public partial Color MutedForeground { get; set; }

    /// <summary>
    /// <c>--accent</c>: stop 100 / 800.
    /// </summary>
    public partial Color Accent { get; set; }

    /// <summary>
    /// <c>--accent-foreground</c>: stop 900 / 50.
    /// </summary>
    public partial Color AccentForeground { get; set; }

    /// <summary>
    /// <c>--destructive</c>: red-600 / red-400.
    /// </summary>
    public partial Color Destructive { get; set; }

    /// <summary>
    /// <c>--border</c>: stop 200; dark white at 10%.
    /// </summary>
    public partial Color Border { get; set; }

    /// <summary>
    /// <c>--input</c>: stop 200; dark white at 15%.
    /// </summary>
    public partial Color Input { get; set; }

    /// <summary>
    /// <c>--ring</c>: stop 400 / 500.
    /// </summary>
    public partial Color Ring { get; set; }

    #endregion

    #region Theme

    /// <summary>
    /// The theme's <c>--primary</c> for the current appearance: the base color's primary for the neutral theme, the
    /// family's 700/800 stop (lime and yellow: 400/500) for an official theme, the seed (light) or its derived dark
    /// tone for a custom one.
    /// </summary>
    public partial Color ThemePrimary { get; set; }

    /// <summary>
    /// The theme's <c>--primary-foreground</c>.
    /// </summary>
    public partial Color ThemePrimaryForeground { get; set; }

    /// <summary>
    /// Whether the theme is one of the official color themes (not the neutral theme, not a custom seed).
    /// </summary>
    public partial bool HasColorTheme { get; set; }

    #endregion

    #region Chart palette

    /// <summary>
    /// The first chart series color, <c>--chart-1</c>.
    /// </summary>
    public partial Color Chart1 { get; set; }

    /// <summary>
    /// The second chart series color, <c>--chart-2</c>.
    /// </summary>
    public partial Color Chart2 { get; set; }

    /// <summary>
    /// The third chart series color, <c>--chart-3</c>.
    /// </summary>
    public partial Color Chart3 { get; set; }

    /// <summary>
    /// The fourth chart series color, <c>--chart-4</c>.
    /// </summary>
    public partial Color Chart4 { get; set; }

    /// <summary>
    /// The fifth chart series color, <c>--chart-5</c>.
    /// </summary>
    public partial Color Chart5 { get; set; }

    #endregion

    #region Style geometry

    /// <summary>
    /// The style's control height (Map <c>ControlHeight</c> before the compact density).
    /// </summary>
    public partial double StyleControlHeight { get; set; }

    /// <summary>
    /// The style's control height at the small size (Map <c>ControlHeightSM</c>).
    /// </summary>
    public partial double StyleControlHeightSM { get; set; }

    /// <summary>
    /// The style's control height at the large size (Map <c>ControlHeightLG</c>).
    /// </summary>
    public partial double StyleControlHeightLG { get; set; }

    /// <summary>
    /// The style's control height at the extra-small size (Map <c>ControlHeightXS</c>).
    /// </summary>
    public partial double StyleControlHeightXS { get; set; }

    /// <summary>
    /// The button radius ladder; the global radius ladder follows it.
    /// </summary>
    public partial double ButtonRadius { get; set; }

    /// <summary>
    /// The small button radius of the style; it sets the global <c>BorderRadiusSM</c> and <c>BorderRadiusXS</c>.
    /// </summary>
    public partial double ButtonRadiusSM { get; set; }

    /// <summary>
    /// The large button radius of the style; it sets the global <c>BorderRadiusLG</c>.
    /// </summary>
    public partial double ButtonRadiusLG { get; set; }

    /// <summary>
    /// The form-field radius: the registry input has no size variants, one radius at every size.
    /// </summary>
    public partial double InputRadius { get; set; }

    /// <summary>
    /// The share of <see cref="Input" /> the dark field background takes (<c>dark:bg-input/30</c>).
    /// </summary>
    public partial double DarkInputFill { get; set; }

    /// <summary>
    /// The checkbox indicator radius.
    /// </summary>
    public partial double CheckBoxRadius { get; set; }

    /// <summary>
    /// The line progress height (one height per style).
    /// </summary>
    public partial double ProgressHeight { get; set; }

    /// <summary>
    /// The line progress radius; 100 or more is <c>rounded-full</c>.
    /// </summary>
    public partial double ProgressRadius { get; set; }

    /// <summary>
    /// The horizontal button padding at the default, small and large size.
    /// </summary>
    public partial double PaddingX { get; set; }

    /// <summary>
    /// The horizontal button padding at the small size. At present only the float button reads it, as its horizontal
    /// padding capped at 10.
    /// </summary>
    public partial double PaddingXSM { get; set; }

    /// <summary>
    /// The horizontal button padding at the large size. No shadcn rule reads it at present.
    /// </summary>
    public partial double PaddingXLG { get; set; }

    /// <summary>
    /// The style's body font size (Map <c>FontSize</c>) and small font size.
    /// </summary>
    public partial double StyleFontSize { get; set; }

    /// <summary>
    /// The style's small font size (Map <c>FontSizeSM</c>).
    /// </summary>
    public partial double StyleFontSizeSM { get; set; }

    /// <summary>
    /// The button font weight.
    /// </summary>
    public partial FontWeight ButtonWeight { get; set; }

    /// <summary>
    /// The focus ring width (<c>ring-[3px]</c>).
    /// </summary>
    public partial double FocusRingWidth { get; set; }

    /// <summary>
    /// The focus ring strength in percent (<c>ring-ring/50</c>).
    /// </summary>
    public partial double FocusRingAlpha { get; set; }

    /// <summary>
    /// The card radius.
    /// </summary>
    public partial double CardRadius { get; set; }

    /// <summary>
    /// The card padding.
    /// </summary>
    public partial double CardPadding { get; set; }

    /// <summary>
    /// The card shadow level.
    /// </summary>
    public partial BoxShadows CardShadow { get; set; }

    /// <summary>
    /// The card ring strength in percent.
    /// </summary>
    public partial double CardRingAlpha { get; set; }

    #endregion

    #region Overlays

    /// <summary>
    /// The menu and select panel shadow level.
    /// </summary>
    public partial BoxShadows MenuShadow { get; set; }

    /// <summary>
    /// The corner radius of the menu panel. The overlay recipes also use it for every overlay kind other than popover
    /// and dialog.
    /// </summary>
    public partial double MenuRadius { get; set; }

    /// <summary>
    /// The padding of the menu panel. The overlay recipes also use it for every overlay kind other than popover and
    /// dialog.
    /// </summary>
    public partial double MenuPadding { get; set; }

    /// <summary>
    /// The popover shadow level.
    /// </summary>
    public partial BoxShadows PopoverShadow { get; set; }

    /// <summary>
    /// The corner radius of the popover panel.
    /// </summary>
    public partial double PopoverRadius { get; set; }

    /// <summary>
    /// The padding of the popover panel.
    /// </summary>
    public partial double PopoverPadding { get; set; }

    /// <summary>
    /// The outline ring strength of the floating surfaces in percent, light and dark.
    /// </summary>
    public partial double RingAlphaLight { get; set; }

    /// <summary>
    /// The outline ring strength of the floating surfaces in percent in the dark appearance.
    /// </summary>
    public partial double RingAlphaDark { get; set; }

    /// <summary>
    /// The outline ring color of the floating surfaces in the current appearance: <see cref="Foreground" /> at the
    /// ring strength.
    /// </summary>
    public partial Color OutlineRing { get; set; }

    /// <summary>
    /// The corner radius of the tooltip.
    /// </summary>
    public partial double TooltipRadius { get; set; }

    /// <summary>
    /// The corner radius of the tooltip arrow. No shadcn rule reads it at present.
    /// </summary>
    public partial double TooltipArrowRadius { get; set; }

    /// <summary>
    /// The dialog shadow level.
    /// </summary>
    public partial BoxShadows DialogShadow { get; set; }

    /// <summary>
    /// The corner radius of the dialog panel.
    /// </summary>
    public partial double DialogRadius { get; set; }

    /// <summary>
    /// The padding of the dialog panel.
    /// </summary>
    public partial double DialogPadding { get; set; }

    /// <summary>
    /// The drawer (sheet) shadow level.
    /// </summary>
    public partial BoxShadows DrawerShadow { get; set; }

    /// <summary>
    /// The dialog mask strength in percent.
    /// </summary>
    public partial double MaskAlpha { get; set; }

    #endregion

    /// <summary>
    /// Provides the table roles from the control's effective global view.
    /// </summary>
    public ShadcnTableRecipe GetTableRecipe(DesignToken global) => new(global);

    /// <summary>
    /// Provides chart series colors from the current language tokens.
    /// </summary>
    public ShadcnChartRecipe GetChartRecipe() => new(this);

    /// <summary>
    /// Provides the interactive list item roles from the control's effective global view.
    /// </summary>
    public ShadcnListItemRecipe GetListItemRecipe(DesignToken global) => new(global);

    /// <summary>
    /// Provides the menu panel roles from the current theme.
    /// </summary>
    public ShadcnOverlayRecipe GetMenuOverlayRecipe(DesignToken global) =>
        new(this, global, ShadcnOverlayKind.Menu);

    /// <summary>
    /// Provides the popover panel roles from the current theme.
    /// </summary>
    public ShadcnOverlayRecipe GetPopoverOverlayRecipe(DesignToken global) =>
        new(this, global, ShadcnOverlayKind.Popover);

    /// <summary>
    /// Provides the dialog panel roles from the current theme.
    /// </summary>
    public ShadcnOverlayRecipe GetDialogOverlayRecipe(DesignToken global) =>
        new(this, global, ShadcnOverlayKind.Dialog);

    /// <summary>
    /// Provides the form-field roles from the current theme.
    /// </summary>
    public ShadcnFieldRecipe GetFieldRecipe(DesignToken global, DesignVariant variant) => new(this, global, variant);
}
