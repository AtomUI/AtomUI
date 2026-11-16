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
}
