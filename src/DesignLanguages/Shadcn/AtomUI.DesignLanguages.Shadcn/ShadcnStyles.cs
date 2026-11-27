using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.Shadcn;

/// <summary>
/// The eight shadcn styles (ui.shadcn.com/create). <see cref="Nova" /> is the default; the others follow the official
/// order. A style is the language's appearance family: its geometry, type, focus ring, card and overlay shapes.
/// </summary>
public static class ShadcnStyles
{
    /// <summary>
    /// Vega: 36-high controls, radius 8, xs shadowed cards, a 16-padding popover.
    /// </summary>
    public static DesignStyle Vega { get; } = new("Vega");

    /// <summary>
    /// Nova: the default style — 32-high controls, radius 10, shadowless cards.
    /// </summary>
    public static DesignStyle Nova { get; } = new("Nova");

    /// <summary>
    /// Maia: pill buttons (radius 26), 2xl overlays, a 5% ring in light (10% in dark), an 80% mask.
    /// </summary>
    public static DesignStyle Maia { get; } = new("Maia");

    /// <summary>
    /// Lyra: square everything (radius 0), 12px type, a one-line focus ring.
    /// </summary>
    public static DesignStyle Lyra { get; } = new("Lyra");

    /// <summary>
    /// Mira: compact 28-high controls, 12px type with roomy line height, a 30% two-line focus ring.
    /// </summary>
    public static DesignStyle Mira { get; } = new("Mira");

    /// <summary>
    /// Luma: pill buttons, lg overlays with a 5% light ring, xl dialogs and drawers, a 30% mask.
    /// </summary>
    public static DesignStyle Luma { get; } = new("Luma");

    /// <summary>
    /// Sera: editorial — 40-high controls, square, 12px semibold type, generous padding, md dialogs.
    /// </summary>
    public static DesignStyle Sera { get; } = new("Sera");

    /// <summary>
    /// Rhea: rounded 18, lg overlays with a 5% light ring, sm cards, xl dialogs and drawers.
    /// </summary>
    public static DesignStyle Rhea { get; } = new("Rhea");

    /// <summary>
    /// The styles in the official order.
    /// </summary>
    public static IReadOnlyList<DesignStyle> All { get; } = [Vega, Nova, Maia, Lyra, Mira, Luma, Sera, Rhea];
}

/// <summary>
/// The overlay geometry of one shadcn style: the shadow level and alpha-white outline ring of each floating
/// surface, the tooltip shape, the mask strength. Shadow fields are level names resolved by
/// <see cref="ShadcnStyleTables.Shadow" />.
/// </summary>
internal sealed record ShadcnOverlayTable(
    string MenuShadow,
    double MenuRadius,
    double MenuPadding,
    string PopoverShadow,
    double PopoverRadius,
    double PopoverPadding,
    byte RingAlphaLight,
    byte RingAlphaDark,
    double TooltipRadius,
    double TooltipArrowRadius,
    string DialogShadow,
    double DialogRadius,
    double DialogPadding,
    string DrawerShadow,
    byte MaskAlpha);

/// <summary>
/// The geometry of one shadcn style: control heights, radii, paddings, type, focus ring, card shape and the
/// <see cref="Overlay" /> table.
/// </summary>
internal sealed record ShadcnStyleTable(
    string Id,
    double ControlHeight,
    double ControlHeightSM,
    double ControlHeightLG,
    double ControlHeightXS,
    double ButtonRadius,
    double ButtonRadiusSM,
    double ButtonRadiusLG,
    double InputRadius,
    double DarkInputFill,
    double CheckBoxRadius,
    double ProgressHeight,
    double ProgressRadius,
    double PaddingX,
    double PaddingXSM,
    double PaddingXLG,
    double FontSize,
    double FontSizeSM,
    FontWeight ButtonWeight,
    double FocusRingWidth,
    byte FocusRingAlpha,
    double CardRadius,
    double CardPadding,
    string CardShadow,
    byte CardRingAlpha,
    ShadcnOverlayTable Overlay);

/// <summary>
/// The per-style tables of the official registry.
/// </summary>
internal static class ShadcnStyleTables
{
    public static ShadcnStyleTable Nova { get; } = new(
        "Nova",
        ControlHeight: 32, ControlHeightSM: 28, ControlHeightLG: 36, ControlHeightXS: 24,
        ButtonRadius: 10, ButtonRadiusSM: 8, ButtonRadiusLG: 8, InputRadius: 10, CheckBoxRadius: 4, ProgressHeight: 4, ProgressRadius: 100,
        DarkInputFill: 0.3,
        PaddingX: 10, PaddingXSM: 8, PaddingXLG: 12,
        FontSize: 14, FontSizeSM: 12.8,
        ButtonWeight: FontWeight.Medium,
        FocusRingWidth: 3, FocusRingAlpha: 50,
        CardRadius: 14, CardPadding: 16, CardShadow: "none", CardRingAlpha: 10,
        Overlay: new ShadcnOverlayTable(
            MenuShadow: "md", MenuRadius: 10, MenuPadding: 4,
            PopoverShadow: "md", PopoverRadius: 10, PopoverPadding: 10,
            RingAlphaLight: 10, RingAlphaDark: 10,
            TooltipRadius: 8, TooltipArrowRadius: 2,
            DialogShadow: "none", DialogRadius: 14, DialogPadding: 16,
            DrawerShadow: "lg", MaskAlpha: 10));

    public static ShadcnStyleTable Vega { get; } = new(
        "Vega",
        ControlHeight: 36, ControlHeightSM: 32, ControlHeightLG: 40, ControlHeightXS: 24,
        ButtonRadius: 8, ButtonRadiusSM: 8, ButtonRadiusLG: 8, InputRadius: 8, CheckBoxRadius: 4, ProgressHeight: 6, ProgressRadius: 100,
        DarkInputFill: 0.3,
        PaddingX: 10, PaddingXSM: 8, PaddingXLG: 12,
        FontSize: 14, FontSizeSM: 12.8,
        ButtonWeight: FontWeight.Medium,
        FocusRingWidth: 3, FocusRingAlpha: 50,
        CardRadius: 14, CardPadding: 24, CardShadow: "xs", CardRingAlpha: 10,
        Overlay: new ShadcnOverlayTable(
            MenuShadow: "md", MenuRadius: 8, MenuPadding: 4,
            PopoverShadow: "md", PopoverRadius: 8, PopoverPadding: 16,
            RingAlphaLight: 10, RingAlphaDark: 10,
            TooltipRadius: 8, TooltipArrowRadius: 2,
            DialogShadow: "none", DialogRadius: 14, DialogPadding: 24,
            DrawerShadow: "lg", MaskAlpha: 10));

    public static ShadcnStyleTable Maia { get; } = new(
        "Maia",
        ControlHeight: 36, ControlHeightSM: 32, ControlHeightLG: 40, ControlHeightXS: 24,
        ButtonRadius: 26, ButtonRadiusSM: 26, ButtonRadiusLG: 26, InputRadius: 26, CheckBoxRadius: 6, ProgressHeight: 12, ProgressRadius: 26,
        DarkInputFill: 0.3,
        PaddingX: 12, PaddingXSM: 8, PaddingXLG: 16,
        FontSize: 14, FontSizeSM: 12.8,
        ButtonWeight: FontWeight.Medium,
        FocusRingWidth: 3, FocusRingAlpha: 50,
        CardRadius: 18, CardPadding: 24, CardShadow: "none", CardRingAlpha: 10,
        Overlay: new ShadcnOverlayTable(
            MenuShadow: "2xl", MenuRadius: 18, MenuPadding: 4,
            PopoverShadow: "2xl", PopoverRadius: 18, PopoverPadding: 16,
            RingAlphaLight: 5, RingAlphaDark: 10,
            TooltipRadius: 18, TooltipArrowRadius: 2,
            DialogShadow: "none", DialogRadius: 26, DialogPadding: 24,
            DrawerShadow: "lg", MaskAlpha: 80));

    public static ShadcnStyleTable Lyra { get; } = new(
        "Lyra",
        ControlHeight: 32, ControlHeightSM: 28, ControlHeightLG: 36, ControlHeightXS: 24,
        ButtonRadius: 0, ButtonRadiusSM: 0, ButtonRadiusLG: 0, InputRadius: 0, CheckBoxRadius: 0, ProgressHeight: 4, ProgressRadius: 0,
        DarkInputFill: 0.3,
        PaddingX: 10, PaddingXSM: 8, PaddingXLG: 12,
        FontSize: 12, FontSizeSM: 12,
        ButtonWeight: FontWeight.Medium,
        FocusRingWidth: 1, FocusRingAlpha: 50,
        CardRadius: 0, CardPadding: 16, CardShadow: "none", CardRingAlpha: 10,
        Overlay: new ShadcnOverlayTable(
            MenuShadow: "md", MenuRadius: 0, MenuPadding: 0,
            PopoverShadow: "md", PopoverRadius: 0, PopoverPadding: 10,
            RingAlphaLight: 10, RingAlphaDark: 10,
            TooltipRadius: 0, TooltipArrowRadius: 0,
            DialogShadow: "none", DialogRadius: 0, DialogPadding: 16,
            DrawerShadow: "lg", MaskAlpha: 10));

    public static ShadcnStyleTable Mira { get; } = new(
        "Mira",
        ControlHeight: 28, ControlHeightSM: 24, ControlHeightLG: 32, ControlHeightXS: 20,
        ButtonRadius: 8, ButtonRadiusSM: 6, ButtonRadiusLG: 8, InputRadius: 8, CheckBoxRadius: 4, ProgressHeight: 4, ProgressRadius: 8,
        DarkInputFill: 0.3,
        PaddingX: 8, PaddingXSM: 8, PaddingXLG: 10,
        FontSize: 12, FontSizeSM: 12,
        ButtonWeight: FontWeight.Medium,
        FocusRingWidth: 2, FocusRingAlpha: 30,
        CardRadius: 10, CardPadding: 16, CardShadow: "none", CardRingAlpha: 10,
        Overlay: new ShadcnOverlayTable(
            MenuShadow: "md", MenuRadius: 10, MenuPadding: 4,
            PopoverShadow: "md", PopoverRadius: 10, PopoverPadding: 10,
            RingAlphaLight: 10, RingAlphaDark: 10,
            TooltipRadius: 8, TooltipArrowRadius: 2,
            DialogShadow: "none", DialogRadius: 14, DialogPadding: 16,
            DrawerShadow: "lg", MaskAlpha: 80));

    public static ShadcnStyleTable Luma { get; } = new(
        "Luma",
        ControlHeight: 36, ControlHeightSM: 32, ControlHeightLG: 40, ControlHeightXS: 24,
        ButtonRadius: 26, ButtonRadiusSM: 26, ButtonRadiusLG: 26, InputRadius: 22, CheckBoxRadius: 5, ProgressHeight: 12, ProgressRadius: 100,
        DarkInputFill: 0.5,
        PaddingX: 12, PaddingXSM: 8, PaddingXLG: 16,
        FontSize: 14, FontSizeSM: 12.8,
        ButtonWeight: FontWeight.Medium,
        FocusRingWidth: 3, FocusRingAlpha: 30,
        CardRadius: 26, CardPadding: 24, CardShadow: "md", CardRingAlpha: 5,
        Overlay: new ShadcnOverlayTable(
            MenuShadow: "lg", MenuRadius: 22, MenuPadding: 6,
            PopoverShadow: "lg", PopoverRadius: 22, PopoverPadding: 10,
            RingAlphaLight: 5, RingAlphaDark: 10,
            TooltipRadius: 14, TooltipArrowRadius: 2,
            DialogShadow: "xl", DialogRadius: 26, DialogPadding: 24,
            DrawerShadow: "xl", MaskAlpha: 30));

    public static ShadcnStyleTable Sera { get; } = new(
        "Sera",
        ControlHeight: 40, ControlHeightSM: 36, ControlHeightLG: 44, ControlHeightXS: 28,
        ButtonRadius: 0, ButtonRadiusSM: 0, ButtonRadiusLG: 0, InputRadius: 0, CheckBoxRadius: 0, ProgressHeight: 2, ProgressRadius: 0,
        DarkInputFill: 0,
        PaddingX: 24, PaddingXSM: 16, PaddingXLG: 32,
        FontSize: 12, FontSizeSM: 12,
        ButtonWeight: FontWeight.SemiBold,
        FocusRingWidth: 2, FocusRingAlpha: 30,
        CardRadius: 0, CardPadding: 32, CardShadow: "sm", CardRingAlpha: 5,
        Overlay: new ShadcnOverlayTable(
            MenuShadow: "md", MenuRadius: 0, MenuPadding: 6,
            PopoverShadow: "md", PopoverRadius: 0, PopoverPadding: 10,
            RingAlphaLight: 10, RingAlphaDark: 10,
            TooltipRadius: 0, TooltipArrowRadius: 0,
            DialogShadow: "md", DialogRadius: 0, DialogPadding: 24,
            DrawerShadow: "md", MaskAlpha: 20));

    public static ShadcnStyleTable Rhea { get; } = new(
        "Rhea",
        ControlHeight: 32, ControlHeightSM: 28, ControlHeightLG: 36, ControlHeightXS: 24,
        ButtonRadius: 18, ButtonRadiusSM: 18, ButtonRadiusLG: 18, InputRadius: 18, CheckBoxRadius: 5, ProgressHeight: 8, ProgressRadius: 18,
        DarkInputFill: 0.5,
        PaddingX: 12, PaddingXSM: 8, PaddingXLG: 16,
        FontSize: 14, FontSizeSM: 12.8,
        ButtonWeight: FontWeight.Medium,
        FocusRingWidth: 3, FocusRingAlpha: 30,
        CardRadius: 24, CardPadding: 20, CardShadow: "sm", CardRingAlpha: 5,
        Overlay: new ShadcnOverlayTable(
            MenuShadow: "lg", MenuRadius: 18, MenuPadding: 4,
            PopoverShadow: "lg", PopoverRadius: 22, PopoverPadding: 10,
            RingAlphaLight: 5, RingAlphaDark: 10,
            TooltipRadius: 14, TooltipArrowRadius: 2,
            DialogShadow: "xl", DialogRadius: 24, DialogPadding: 24,
            DrawerShadow: "xl", MaskAlpha: 30));

    private static readonly IReadOnlyList<ShadcnStyleTable> s_all = [Vega, Nova, Maia, Lyra, Mira, Luma, Sera, Rhea];

    /// <summary>
    /// The table of a declared style; the compiler has already rejected undeclared styles.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="style" /> is not one of the declared styles.
    /// </exception>
    public static ShadcnStyleTable Of(DesignStyle style)
    {
        foreach (var table in s_all)
        {
            if (string.Equals(table.Id, style.Id, StringComparison.Ordinal))
            {
                return table;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(style), style.Id, "Not a shadcn style.");
    }

    /// <summary>
    /// Resolves a shadow level name to the official Tailwind v4 shadow definition.
    /// </summary>
    public static BoxShadows Shadow(string level) => level switch
    {
        "none" => default,
        "xs"   => BoxShadows.Parse("0 1 2 0 #0D000000"),
        "sm"   => BoxShadows.Parse("0 1 3 0 #1A000000, 0 1 2 -1 #1A000000"),
        "md"   => BoxShadows.Parse("0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000"),
        "lg"   => BoxShadows.Parse("0 10 15 -3 #1A000000, 0 4 6 -4 #1A000000"),
        "xl"   => BoxShadows.Parse("0 20 25 -5 #1A000000, 0 8 10 -6 #1A000000"),
        "2xl"  => BoxShadows.Parse("0 25 50 -12 #40000000"),
        _      => throw new ArgumentOutOfRangeException(nameof(level), level, "A shadcn shadow level is one of none, xs, sm, md, lg, xl, 2xl.")
    };
}
