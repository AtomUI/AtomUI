using AtomUI.Theme.Algorithms;
using Avalonia.Media;

namespace AtomUI.Themes.Shadcn.Algorithms;

/// <summary>
/// The overlay geometry of one shadcn style: the shadow level and alpha-white outline ring of each floating
/// surface, the tooltip shape, the mask strength. Shadow fields are level names resolved by
/// <see cref="ShadcnStyles.Shadow"/>.
/// </summary>
internal sealed record ShadcnOverlay(
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
/// <see cref="Overlay"/> table (design 5.5 and 5.6.1 of the alignment spec).
/// </summary>
internal sealed record ShadcnStyle(
    string Id,
    double ControlHeight,
    double ControlHeightSM,
    double ControlHeightLG,
    double ControlHeightXS,
    double ButtonRadius,
    double ButtonRadiusSM,
    double ButtonRadiusLG,
    double InputRadius,
    double CheckBoxRadius,
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
    ShadcnOverlay Overlay);

/// <summary>
/// The eight shadcn styles. Task order: Nova lands with the shared algorithm; the remaining seven styles join
/// this table with their profiles.
/// </summary>
internal static class ShadcnStyles
{
    public const string Catalog = "Shadcn";

    /// <summary>Nova: the default style of ui.shadcn.com/create — 32-high controls, radius 10, shadowless cards.</summary>
    public static ShadcnStyle Nova { get; } = new(
        "Nova",
        ControlHeight: 32, ControlHeightSM: 28, ControlHeightLG: 36, ControlHeightXS: 24,
        ButtonRadius: 10, ButtonRadiusSM: 8, ButtonRadiusLG: 8, InputRadius: 10, CheckBoxRadius: 4,
        PaddingX: 10, PaddingXSM: 8, PaddingXLG: 12,
        FontSize: 14, FontSizeSM: 12.8,
        ButtonWeight: FontWeight.Medium,
        FocusRingWidth: 3, FocusRingAlpha: 50,
        CardRadius: 14, CardPadding: 16, CardShadow: "none", CardRingAlpha: 10,
        Overlay: new ShadcnOverlay(
            MenuShadow: "md", MenuRadius: 10, MenuPadding: 4,
            PopoverShadow: "md", PopoverRadius: 10, PopoverPadding: 10,
            RingAlphaLight: 10, RingAlphaDark: 10,
            TooltipRadius: 8, TooltipArrowRadius: 2,
            DialogShadow: "none", DialogRadius: 14, DialogPadding: 16,
            DrawerShadow: "lg", MaskAlpha: 10));

    /// <summary>Vega: 36-high controls, radius 8, xs shadowed cards, a 16-padding popover.</summary>
    public static ShadcnStyle Vega { get; } = new(
        "Vega",
        ControlHeight: 36, ControlHeightSM: 32, ControlHeightLG: 40, ControlHeightXS: 24,
        ButtonRadius: 8, ButtonRadiusSM: 8, ButtonRadiusLG: 8, InputRadius: 8, CheckBoxRadius: 4,
        PaddingX: 10, PaddingXSM: 8, PaddingXLG: 12,
        FontSize: 14, FontSizeSM: 12.8,
        ButtonWeight: FontWeight.Medium,
        FocusRingWidth: 3, FocusRingAlpha: 50,
        CardRadius: 14, CardPadding: 24, CardShadow: "xs", CardRingAlpha: 10,
        Overlay: new ShadcnOverlay(
            MenuShadow: "md", MenuRadius: 8, MenuPadding: 4,
            PopoverShadow: "md", PopoverRadius: 8, PopoverPadding: 16,
            RingAlphaLight: 10, RingAlphaDark: 10,
            TooltipRadius: 8, TooltipArrowRadius: 2,
            DialogShadow: "none", DialogRadius: 14, DialogPadding: 24,
            DrawerShadow: "lg", MaskAlpha: 10));

    /// <summary>Maia: pill buttons (radius 26), 2xl overlays, a 5% ring in light (10% in dark), an 80% mask.</summary>
    public static ShadcnStyle Maia { get; } = new(
        "Maia",
        ControlHeight: 36, ControlHeightSM: 32, ControlHeightLG: 40, ControlHeightXS: 24,
        ButtonRadius: 26, ButtonRadiusSM: 26, ButtonRadiusLG: 26, InputRadius: 26, CheckBoxRadius: 6,
        PaddingX: 12, PaddingXSM: 8, PaddingXLG: 16,
        FontSize: 14, FontSizeSM: 12.8,
        ButtonWeight: FontWeight.Medium,
        FocusRingWidth: 3, FocusRingAlpha: 50,
        CardRadius: 18, CardPadding: 24, CardShadow: "none", CardRingAlpha: 10,
        Overlay: new ShadcnOverlay(
            MenuShadow: "2xl", MenuRadius: 18, MenuPadding: 4,
            PopoverShadow: "2xl", PopoverRadius: 18, PopoverPadding: 16,
            RingAlphaLight: 5, RingAlphaDark: 10,
            TooltipRadius: 18, TooltipArrowRadius: 2,
            DialogShadow: "none", DialogRadius: 26, DialogPadding: 24,
            DrawerShadow: "lg", MaskAlpha: 80));

    /// <summary>Lyra: square everything (radius 0), 12px type, a one-line focus ring.</summary>
    public static ShadcnStyle Lyra { get; } = new(
        "Lyra",
        ControlHeight: 32, ControlHeightSM: 28, ControlHeightLG: 36, ControlHeightXS: 24,
        ButtonRadius: 0, ButtonRadiusSM: 0, ButtonRadiusLG: 0, InputRadius: 0, CheckBoxRadius: 0,
        PaddingX: 10, PaddingXSM: 8, PaddingXLG: 12,
        FontSize: 12, FontSizeSM: 12,
        ButtonWeight: FontWeight.Medium,
        FocusRingWidth: 1, FocusRingAlpha: 50,
        CardRadius: 0, CardPadding: 16, CardShadow: "none", CardRingAlpha: 10,
        Overlay: new ShadcnOverlay(
            MenuShadow: "md", MenuRadius: 0, MenuPadding: 0,
            PopoverShadow: "md", PopoverRadius: 0, PopoverPadding: 10,
            RingAlphaLight: 10, RingAlphaDark: 10,
            TooltipRadius: 0, TooltipArrowRadius: 0,
            DialogShadow: "none", DialogRadius: 0, DialogPadding: 16,
            DrawerShadow: "lg", MaskAlpha: 10));

    /// <summary>Mira: compact 28-high controls, 12px type with roomy line height, a 30% two-line focus ring.</summary>
    public static ShadcnStyle Mira { get; } = new(
        "Mira",
        ControlHeight: 28, ControlHeightSM: 24, ControlHeightLG: 32, ControlHeightXS: 20,
        ButtonRadius: 8, ButtonRadiusSM: 6, ButtonRadiusLG: 8, InputRadius: 8, CheckBoxRadius: 4,
        PaddingX: 8, PaddingXSM: 8, PaddingXLG: 10,
        FontSize: 12, FontSizeSM: 12,
        ButtonWeight: FontWeight.Medium,
        FocusRingWidth: 2, FocusRingAlpha: 30,
        CardRadius: 10, CardPadding: 16, CardShadow: "none", CardRingAlpha: 10,
        Overlay: new ShadcnOverlay(
            MenuShadow: "md", MenuRadius: 10, MenuPadding: 4,
            PopoverShadow: "md", PopoverRadius: 10, PopoverPadding: 10,
            RingAlphaLight: 10, RingAlphaDark: 10,
            TooltipRadius: 8, TooltipArrowRadius: 2,
            DialogShadow: "none", DialogRadius: 14, DialogPadding: 16,
            DrawerShadow: "lg", MaskAlpha: 80));

    /// <summary>Luma: pill buttons, lg overlays with a 5% light ring, xl dialogs and drawers, a 30% mask.</summary>
    public static ShadcnStyle Luma { get; } = new(
        "Luma",
        ControlHeight: 36, ControlHeightSM: 32, ControlHeightLG: 40, ControlHeightXS: 24,
        ButtonRadius: 26, ButtonRadiusSM: 26, ButtonRadiusLG: 26, InputRadius: 22, CheckBoxRadius: 5,
        PaddingX: 12, PaddingXSM: 8, PaddingXLG: 16,
        FontSize: 14, FontSizeSM: 12.8,
        ButtonWeight: FontWeight.Medium,
        FocusRingWidth: 3, FocusRingAlpha: 30,
        CardRadius: 26, CardPadding: 24, CardShadow: "md", CardRingAlpha: 5,
        Overlay: new ShadcnOverlay(
            MenuShadow: "lg", MenuRadius: 22, MenuPadding: 6,
            PopoverShadow: "lg", PopoverRadius: 22, PopoverPadding: 10,
            RingAlphaLight: 5, RingAlphaDark: 10,
            TooltipRadius: 14, TooltipArrowRadius: 2,
            DialogShadow: "xl", DialogRadius: 26, DialogPadding: 24,
            DrawerShadow: "xl", MaskAlpha: 30));

    /// <summary>Sera: editorial — 40-high controls, square, 12px semibold type, generous padding, md dialogs.</summary>
    public static ShadcnStyle Sera { get; } = new(
        "Sera",
        ControlHeight: 40, ControlHeightSM: 36, ControlHeightLG: 44, ControlHeightXS: 28,
        ButtonRadius: 0, ButtonRadiusSM: 0, ButtonRadiusLG: 0, InputRadius: 0, CheckBoxRadius: 0,
        PaddingX: 24, PaddingXSM: 16, PaddingXLG: 32,
        FontSize: 12, FontSizeSM: 12,
        ButtonWeight: FontWeight.SemiBold,
        FocusRingWidth: 2, FocusRingAlpha: 30,
        CardRadius: 0, CardPadding: 32, CardShadow: "sm", CardRingAlpha: 5,
        Overlay: new ShadcnOverlay(
            MenuShadow: "md", MenuRadius: 0, MenuPadding: 6,
            PopoverShadow: "md", PopoverRadius: 0, PopoverPadding: 10,
            RingAlphaLight: 10, RingAlphaDark: 10,
            TooltipRadius: 0, TooltipArrowRadius: 0,
            DialogShadow: "md", DialogRadius: 0, DialogPadding: 24,
            DrawerShadow: "md", MaskAlpha: 20));

    /// <summary>Rhea: rounded 18, lg overlays with a 5% light ring, sm cards, xl dialogs and drawers.</summary>
    public static ShadcnStyle Rhea { get; } = new(
        "Rhea",
        ControlHeight: 32, ControlHeightSM: 28, ControlHeightLG: 36, ControlHeightXS: 24,
        ButtonRadius: 18, ButtonRadiusSM: 18, ButtonRadiusLG: 18, InputRadius: 18, CheckBoxRadius: 5,
        PaddingX: 12, PaddingXSM: 8, PaddingXLG: 16,
        FontSize: 14, FontSizeSM: 12.8,
        ButtonWeight: FontWeight.Medium,
        FocusRingWidth: 3, FocusRingAlpha: 30,
        CardRadius: 24, CardPadding: 20, CardShadow: "sm", CardRingAlpha: 5,
        Overlay: new ShadcnOverlay(
            MenuShadow: "lg", MenuRadius: 18, MenuPadding: 4,
            PopoverShadow: "lg", PopoverRadius: 22, PopoverPadding: 10,
            RingAlphaLight: 5, RingAlphaDark: 10,
            TooltipRadius: 14, TooltipArrowRadius: 2,
            DialogShadow: "xl", DialogRadius: 24, DialogPadding: 24,
            DrawerShadow: "xl", MaskAlpha: 30));

    public static readonly IReadOnlyList<ShadcnStyle> All =
    [
        Vega,
        Nova,
        Maia,
        Lyra,
        Mira,
        Luma,
        Sera,
        Rhea
    ];

    /// <summary>
    /// Gets the style of the algorithm chain, by its <c>Shadcn:&lt;Style&gt;</c> or <c>Shadcn:&lt;Style&gt;.Dark</c>
    /// entry (the last one wins), like <c>PrimeNGPresets.Resolve</c>.
    /// </summary>
    public static ShadcnStyle Resolve(in ThemeAlgorithmContext context)
    {
        var algorithms = context.Algorithms;
        for (var index = algorithms.Count - 1; index >= 0; index--)
        {
            var algorithm = algorithms[index];
            if (!string.Equals(algorithm.Catalog, Catalog, StringComparison.Ordinal))
            {
                continue;
            }

            var id = algorithm.Id;
            if (id.EndsWith(".Dark", StringComparison.Ordinal))
            {
                id = id[..^".Dark".Length];
            }

            var style = All.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.Ordinal));
            if (style is not null)
            {
                return style;
            }
        }

        throw new InvalidOperationException("The algorithm chain contains no Shadcn style.");
    }

    /// <summary>Resolves a shadow level name to the official Tailwind v4 shadow definition (design 5.6.1).</summary>
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

    /// <summary>Alpha white or any colour at a percent strength (10 → <c>#1A</c>, 50 → <c>#80</c>).</summary>
    public static Color WithAlpha(Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);

    /// <summary>The outline ring colour of the floating surfaces: the foreground at the style's ring alpha.</summary>
    public static Color OutlineRing(ShadcnSurface surface, bool dark, ShadcnStyle style)
    {
        return WithAlpha(surface.Foreground, AlphaByte(dark ? style.Overlay.RingAlphaDark : style.Overlay.RingAlphaLight));
    }

    /// <summary>
    /// The outline ring (<c>ring-1</c>: a 1px spread shadow) first, then the shadow level: CSS paints the ring
    /// above the shadow, and AtomUI paints the first shadow on top.
    /// </summary>
    public static BoxShadows RingThenShadow(Color ring, string level)
    {
        var shadows = Shadow(level);
        var ringShadow = new BoxShadow { Spread = 1, Color = ring };
        if (shadows.Count == 0)
        {
            return new BoxShadows(ringShadow);
        }

        var rest = new BoxShadow[shadows.Count];
        for (var i = 0; i < shadows.Count; i++)
        {
            rest[i] = shadows[i];
        }

        return new BoxShadows(ringShadow, rest);
    }

    /// <summary>Converts a percent strength to its alpha byte the way Tailwind rounds (25.5 → 26).</summary>
    public static byte AlphaByte(double percent) => (byte)Math.Clamp(Math.Round(percent * 255 / 100d, MidpointRounding.AwayFromZero), 0, 255);
}
