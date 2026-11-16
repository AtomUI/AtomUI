using AtomUI.Theme.Compilation;
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

    private static T ControlGlobal<T>(ThemeSnapshot snapshot, string controlId, string globalName)
    {
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", controlId), out var control)
                 .ShouldBeTrue(controlId);
        snapshot.Registry.TryGetGlobalToken(globalName, out var token).ShouldBeTrue(globalName);
        return snapshot.Controls[control!.Slot]
                       .GetEffectiveGlobalValue<T>(snapshot.GlobalTokenValues, token!.Slot);
    }
}
