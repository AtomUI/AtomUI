using AtomUI.Theme.Compilation;
using Avalonia;
using AtomUI.Theme.Schema;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Themes.Tests.Shadcn;

/// <summary>
/// The Nova control stages of the in-place controls: the input family's ring focus and input border, the card
/// surface and the button type weight.
/// </summary>
public class ShadcnControlTests
{
    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    [InlineData("zinc", false)]
    [InlineData("zinc", true)]
    public async Task Input_Focus_Border_Is_Ring(string family, bool dark)
    {
        var snapshot = await ShadcnOverlayTests.Apply(family, dark);
        var table = ShadcnAppendix.BaseColors();

        foreach (var id in new[] { "LineEdit", "TextArea", "Select", "Cascader", "DatePicker", "TimePicker", "AutoComplete", "Mentions" })
        {
            ControlGlobal<Color>(snapshot, id, "ColorPrimary").ShouldBe(table[(family, dark, "ring")], id);
            ControlGlobal<Color>(snapshot, id, "ColorPrimaryHover").ShouldBe(table[(family, dark, "ring")], id);
        }
    }

    [AvaloniaTheory]
    [InlineData("neutral", true)]
    [InlineData("zinc", true)]
    public async Task Input_Border_Is_Input_Color_In_Dark(string family, bool dark)
    {
        var snapshot = await ShadcnOverlayTests.Apply(family, dark);
        var table = ShadcnAppendix.BaseColors();

        foreach (var id in new[] { "LineEdit", "TextArea", "Select", "Cascader", "DatePicker", "TimePicker", "AutoComplete", "Mentions" })
        {
            ControlGlobal<Color>(snapshot, id, "ColorBorder").ShouldBe(table[(family, dark, "input")], id);
        }
    }

    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    public async Task Card_Uses_Card_Surface_And_Ring(string family, bool dark)
    {
        var snapshot = await ShadcnOverlayTests.Apply(family, dark);
        var table = ShadcnAppendix.BaseColors();
        var foreground = table[(family, dark, "foreground")];
        var ring = Color.FromArgb(0x1A, foreground.R, foreground.G, foreground.B);

        ControlGlobal<Color>(snapshot, "Card", "ColorBgContainer").ShouldBe(table[(family, dark, "card")]);
        var shadows = ShadcnOverlayTests.Own<BoxShadows>(snapshot, "Card", "CardShadows");
        shadows.Count.ShouldBe(1);
        shadows[0].Spread.ShouldBe(1);
        shadows[0].Color.ShouldBe(ring);
    }

    [AvaloniaFact]
    public async Task Button_Weight_Is_Medium()
    {
        var snapshot = await ShadcnOverlayTests.Apply("neutral", false);

        ShadcnOverlayTests.Own<FontWeight>(snapshot, "Button", "FontWeight").ShouldBe(FontWeight.Medium);
    }

    [AvaloniaFact]
    public async Task Sera_Button_Is_Semibold_12()
    {
        var snapshot = await ShadcnOverlayTests.Apply("Sera", "neutral", false);

        ShadcnOverlayTests.Own<FontWeight>(snapshot, "Button", "FontWeight").ShouldBe(FontWeight.SemiBold);
        ShadcnOverlayTests.Global<double>(snapshot, "FontSize").ShouldBe(12);
        ShadcnOverlayTests.Global<double>(snapshot, "ControlHeight").ShouldBe(40);
    }

    /// <summary>
    /// Appendix C (official registry classes): the button radii per size. Maia/Luma are rounded-4xl (26) at every
    /// size — NOT half-height capsules; Nova's small/large use min(var(--radius-md)=8, 10/12) = 8; Mira's small is
    /// rounded-sm = 6.
    /// </summary>
    public static readonly (string Style, double H, double SM, double LG, double XS, double Radius, double Font,
        double RadiusSM, double RadiusLG)[] Geometry =
    [
        ("Vega", 36, 32, 40, 24, 8, 14, 8, 8),
        ("Nova", 32, 28, 36, 24, 10, 14, 8, 8),
        ("Maia", 36, 32, 40, 24, 26, 14, 26, 26),
        ("Lyra", 32, 28, 36, 24, 0, 12, 0, 0),
        ("Mira", 28, 24, 32, 20, 8, 12, 6, 8),
        ("Luma", 36, 32, 40, 24, 26, 14, 26, 26),
        ("Sera", 40, 36, 44, 28, 0, 12, 0, 0),
        ("Rhea", 32, 28, 36, 24, 18, 14, 18, 18)
    ];

    [AvaloniaTheory]
    [MemberData(nameof(StyleRows))]
    public async Task Every_Style_Geometry_Matches_The_Design_Table(string style)
    {
        var row = Geometry.Single(item => item.Style == style);
        var snapshot = await ShadcnOverlayTests.Apply(style, "neutral", false);

        ShadcnOverlayTests.Global<double>(snapshot, "ControlHeight").ShouldBe(row.H, style);
        ShadcnOverlayTests.Global<double>(snapshot, "ControlHeightSM").ShouldBe(row.SM, style);
        ShadcnOverlayTests.Global<double>(snapshot, "ControlHeightLG").ShouldBe(row.LG, style);
        ShadcnOverlayTests.Global<double>(snapshot, "ControlHeightXS").ShouldBe(row.XS, style);
        ShadcnOverlayTests.Global<CornerRadius>(snapshot, "BorderRadius").ShouldBe(new CornerRadius(row.Radius), style);
        ShadcnOverlayTests.Global<double>(snapshot, "FontSize").ShouldBe(row.Font, style);

        // The button scope carries the per-size ladder (rounded-4xl styles stay 26; min() formulas clamp Nova/Mira).
        ShadcnOverlayTests.ControlGlobal<CornerRadius>(snapshot, "Button", "BorderRadius").ShouldBe(new CornerRadius(row.Radius), style);
        ShadcnOverlayTests.ControlGlobal<CornerRadius>(snapshot, "Button", "BorderRadiusLG").ShouldBe(new CornerRadius(row.RadiusLG), style);
        ShadcnOverlayTests.ControlGlobal<CornerRadius>(snapshot, "Button", "BorderRadiusSM").ShouldBe(new CornerRadius(row.RadiusSM), style);
    }

    public static TheoryData<string> StyleRows()
    {
        var data = new TheoryData<string>();
        foreach (var (style, _, _, _, _, _, _, _, _) in Geometry)
        {
            data.Add(style);
        }

        return data;
    }

    private static T ControlGlobal<T>(ThemeSnapshot snapshot, string controlId, string globalName)
    {
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", controlId), out var control)
                 .ShouldBeTrue(controlId);
        snapshot.Registry.TryGetGlobalToken(globalName, out var token).ShouldBeTrue(globalName);
        return snapshot.Controls[control!.Slot]
                       .GetEffectiveGlobalValue<T>(snapshot.GlobalTokenValues, token!.Slot);
    }

    [AvaloniaTheory]
    [InlineData("Maia", 26)]   // rounded-4xl, one radius at every size
    [InlineData("Nova", 10)]
    [InlineData("Luma", 22)]
    [InlineData("Rhea", 18)]
    public async Task Input_Radius_Ladder_Matches_The_Style(string style, double radius)
    {
        var snapshot = await ShadcnOverlayTests.Apply(style, "neutral", false);

        foreach (var size in new[] { "BorderRadius", "BorderRadiusLG", "BorderRadiusSM" })
        {
            ShadcnOverlayTests.ControlGlobal<CornerRadius>(snapshot, "LineEdit", size)
                .ShouldBe(new CornerRadius(radius), $"{style} {size}");
        }
    }

    /// <summary>
    /// shadcn has no segmented component, so the Ant track pattern stays (grey track, elevated thumb, soft
    /// shadow) and only the palette flows in from the shadcn globals: the track reads --muted (without the
    /// override it would fall back to ColorBgLayout = --background and disappear into the page), the thumb
    /// --popover, the hover --secondary, and the radius ladder the global per-style BorderRadius.
    /// </summary>
    [AvaloniaTheory]
    [InlineData("Nova", 10d, 8d)]
    [InlineData("Vega", 8d, 8d)]
    [InlineData("Maia", 26d, 26d)]
    [InlineData("Luma", 26d, 26d)]
    [InlineData("Rhea", 18d, 18d)]
    public async Task Segmented_Keeps_The_Ant_Track_With_Shadcn_Colors(string style, double radius, double radiusSM)
    {
        var snapshot = await ShadcnOverlayTests.Apply(style, "neutral", false);
        var table = ShadcnAppendix.BaseColors();

        ShadcnOverlayTests.Own<Color>(snapshot, "Segmented", "TrackBg")
            .ShouldBe(table[("neutral", false, "muted")], $"{style} track");
        ShadcnOverlayTests.Own<Color>(snapshot, "Segmented", "ItemSelectedBg")
            .ShouldBe(table[("neutral", false, "popover")], $"{style} thumb");
        ShadcnOverlayTests.Own<Color>(snapshot, "Segmented", "ItemSelectedColor")
            .ShouldBe(table[("neutral", false, "foreground")], $"{style} selected text");
        ShadcnOverlayTests.Own<Color>(snapshot, "Segmented", "ItemHoverBg")
            .ShouldBe(table[("neutral", false, "secondary")], $"{style} hover");
        // The radius ladder falls back to the global per-style radius (clamped at draw time, f19f3a9fe).
        ShadcnOverlayTests.ControlGlobal<CornerRadius>(snapshot, "Segmented", "BorderRadius")
            .ShouldBe(new CornerRadius(radius), style);
        ShadcnOverlayTests.ControlGlobal<CornerRadius>(snapshot, "Segmented", "BorderRadiusSM")
            .ShouldBe(new CornerRadius(radiusSM), style);
        // The thumb keeps the Ant soft shadow (the shadcn empty override is gone).
        ShadcnOverlayTests.ControlGlobal<BoxShadows>(snapshot, "Segmented", "BoxShadowsTertiary")
            .Count.ShouldBeGreaterThan(0, style);
    }

    /// <summary>
    /// The Empty preset illustration draws its tones from the fill ramp. shadcn maps every level of that ramp onto
    /// one neutral stop (accent/secondary/muted are all stop 100), which flattened the built-in illustration into a
    /// single washed-out tone; Empty therefore carries its own ladder.
    /// </summary>
    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    [InlineData("zinc", false)]
    [InlineData("zinc", true)]
    public async Task Empty_Illustration_Keeps_A_Fill_Ramp(string family, bool dark)
    {
        var snapshot = await ShadcnOverlayTests.Apply(family, dark);

        var background = ShadcnOverlayTests.Global<Color>(snapshot, "ColorBgLayout");
        var accent     = ShadcnOverlayTests.ControlGlobal<Color>(snapshot, "Empty", "ColorFill");
        var body       = ShadcnOverlayTests.ControlGlobal<Color>(snapshot, "Empty", "ColorFillTertiary");
        var faint      = ShadcnOverlayTests.ControlGlobal<Color>(snapshot, "Empty", "ColorFillQuaternary");

        // The accent must clearly stand out from the body tone, and the body from the faintest tone, in the
        // direction away from the page background (dark mode runs the ramp the other way round).
        Contrast(accent, background).ShouldBeGreaterThan(Contrast(body, background) + 0.06, $"{family}/{dark} accent");
        Contrast(body, background).ShouldBeGreaterThan(Contrast(faint, background), $"{family}/{dark} body");
    }

    /// <summary>
    /// The line progress of each style (appendix C): one bar height and one radius per style, the indicator on
    /// --primary and the track on --muted - the antd defaults (info-blue indicator, fill track, 8/6/4 ladder)
    /// break every neutral style.
    /// </summary>
    [AvaloniaTheory]
    [InlineData("Nova", 4d, 100d)]
    [InlineData("Vega", 6d, 100d)]
    [InlineData("Maia", 12d, 26d)]
    [InlineData("Lyra", 4d, 0d)]
    [InlineData("Mira", 4d, 8d)]
    [InlineData("Luma", 12d, 100d)]
    [InlineData("Sera", 2d, 0d)]
    [InlineData("Rhea", 8d, 18d)]
    public async Task Progress_Follows_The_Style_Height_Radius_And_Palette(string style, double height, double radius)
    {
        var snapshot = await ShadcnOverlayTests.Apply(style, "neutral", false);
        var table = ShadcnAppendix.BaseColors();

        ShadcnOverlayTests.Own<Color>(snapshot, "ProgressBar", "DefaultColor")
            .ShouldBe(table[("neutral", false, "primary")], $"{style} indicator");
        ShadcnOverlayTests.Own<Color>(snapshot, "ProgressBar", "RemainingColor")
            .ShouldBe(table[("neutral", false, "muted")], $"{style} track");
        ShadcnOverlayTests.Own<double>(snapshot, "ProgressBar", "LineStrokeThickness").ShouldBe(height, style);
        ShadcnOverlayTests.Own<double>(snapshot, "ProgressBar", "LineStrokeThicknessLG").ShouldBe(height, style);
        ShadcnOverlayTests.Own<double>(snapshot, "ProgressBar", "LineStrokeThicknessSM").ShouldBe(height, style);
        var expectedRadius = radius >= 100 ? 100d : radius;
        ShadcnOverlayTests.Own<CornerRadius>(snapshot, "ProgressBar", "LineBorderRadius")
            .ShouldBe(new CornerRadius(expectedRadius), style);
    }

    private static double Contrast(Color left, Color right) => Math.Abs(Luminance(left) - Luminance(right));

    private static double Luminance(Color color) => (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255d;

    /// <summary>
    /// The checkbox radius of each style (appendix C, <c>rounded-[4px]</c>/<c>[5px]</c>/<c>[6px]</c>/<c>none</c>), not
    /// the button radius ladder: on a 16px indicator a 26px radius collapses into a circle.
    /// </summary>
    [AvaloniaTheory]
    [InlineData("Nova", 4d)]
    [InlineData("Vega", 4d)]
    [InlineData("Maia", 6d)]
    [InlineData("Lyra", 0d)]
    [InlineData("Mira", 4d)]
    [InlineData("Luma", 5d)]
    [InlineData("Sera", 0d)]
    [InlineData("Rhea", 5d)]
    public async Task CheckBox_Uses_The_Style_Checkbox_Radius(string style, double expected)
    {
        var snapshot = await ShadcnOverlayTests.Apply(style, "neutral", false);

        ShadcnOverlayTests.ControlGlobal<CornerRadius>(snapshot, "CheckBox", "BorderRadiusSM")
            .ShouldBe(new CornerRadius(expected), style);
    }
}
