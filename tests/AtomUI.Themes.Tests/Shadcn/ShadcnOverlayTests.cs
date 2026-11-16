using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Themes.Tests.Shadcn;

/// <summary>
/// The Nova control stages of the floating surfaces: menus and arrow flyouts, the dialog ring, the drawer shadow,
/// the mask and the inverted tooltip — against the appendix values and the design 5.6.1 shadow table.
/// </summary>
public class ShadcnOverlayTests
{
    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    [InlineData("zinc", false)]
    [InlineData("zinc", true)]
    public async Task Menu_Popup_Shadow_Is_Ring_Then_Md(string family, bool dark)
    {
        var snapshot = await Apply(family, dark);
        var table = ShadcnAppendix.BaseColors();
        var ring = Ring(table, family, dark);

        var shadows = Own<BoxShadows>(snapshot, "Popup", "PopupRootShadow");
        shadows.Count.ShouldBe(3);
        shadows[0].OffsetX.ShouldBe(0);
        shadows[0].OffsetY.ShouldBe(0);
        shadows[0].Blur.ShouldBe(0);
        shadows[0].Spread.ShouldBe(1);
        shadows[0].Color.ShouldBe(ring);
        shadows.ShouldBe(BoxShadows.Parse("0 0 0 1 " + ArgbHex(ring) + ", 0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000"));
        Own<BoxShadows>(snapshot, "Popup", "OverlayHostShadow").ShouldBe(shadows);
        Own<CornerRadius>(snapshot, "Popup", "PopupCornerRadius").ShouldBe(new CornerRadius(10));
    }

    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    [InlineData("zinc", false)]
    [InlineData("zinc", true)]
    public async Task Arrow_Flyout_Has_Integrated_Border_And_Plain_Shadow(string family, bool dark)
    {
        var snapshot = await Apply(family, dark);
        var table = ShadcnAppendix.BaseColors();
        var ring = Ring(table, family, dark);

        // The flyout shadow has no ring: the arrow box draws the integrated one-line border.
        Own<BoxShadows>(snapshot, "FlyoutHost", "PopupRootShadow")
            .ShouldBe(BoxShadows.Parse("0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000"));
        Own<BoxShadows>(snapshot, "FlyoutHost", "OverlayHostShadow")
            .ShouldBe(BoxShadows.Parse("0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000"));
        Own<Color>(snapshot, "ArrowDecoratedBox", "ContentBorderColor").ShouldBe(ring);
        Own<Thickness>(snapshot, "ArrowDecoratedBox", "ContentBorderThickness").ShouldBe(new Thickness(1));
    }

    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    public async Task Dialog_Has_Ring_Only(string family, bool dark)
    {
        var snapshot = await Apply(family, dark);
        var table = ShadcnAppendix.BaseColors();
        var ring = Ring(table, family, dark);

        // The dialog surface reads the global BoxShadows (DialogSurfaceTheme); Nova's dialog is the outline ring
        // alone, no shadow.
        Global<BoxShadows>(snapshot, "BoxShadows").Count.ShouldBe(1);
        Global<BoxShadows>(snapshot, "BoxShadows")[0].Spread.ShouldBe(1);
        Global<BoxShadows>(snapshot, "BoxShadows")[0].Color.ShouldBe(ring);
    }

    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    public async Task Drawer_Shadow_Is_Lg(string family, bool dark)
    {
        var snapshot = await Apply(family, dark);
        var lg = BoxShadows.Parse("0 10 15 -3 #1A000000, 0 4 6 -4 #1A000000");

        Own<BoxShadows>(snapshot, "Drawer", "BoxShadowDrawerLeft").ShouldBe(lg);
        Own<BoxShadows>(snapshot, "Drawer", "BoxShadowDrawerRight").ShouldBe(lg);
        Own<BoxShadows>(snapshot, "Drawer", "BoxShadowDrawerUp").ShouldBe(lg);
        Own<BoxShadows>(snapshot, "Drawer", "BoxShadowDrawerDown").ShouldBe(lg);
    }

    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    public async Task Mask_Is_Black_10(string family, bool dark)
    {
        var snapshot = await Apply(family, dark);

        Global<Color>(snapshot, "ColorBgMask").ShouldBe(Color.FromArgb(0x1A, 0, 0, 0));
    }

    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    public async Task Tooltip_Is_Inverted(string family, bool dark)
    {
        var snapshot = await Apply(family, dark);
        var table = ShadcnAppendix.BaseColors();

        Own<Color>(snapshot, "ToolTip", "ToolTipBackground").ShouldBe(table[(family, dark, "foreground")]);
        Own<Color>(snapshot, "ToolTip", "ToolTipColor").ShouldBe(table[(family, dark, "background")]);
        Own<CornerRadius>(snapshot, "ToolTip", "ToolTipCornerRadius").ShouldBe(new CornerRadius(8));
        Own<Thickness>(snapshot, "ToolTip", "ContentPadding").ShouldBe(new Thickness(12, 6, 12, 6));
    }

    private static Color Ring(IReadOnlyDictionary<(string, bool, string), Color> table, string family, bool dark)
    {
        // The outline ring of the floating surfaces: the foreground at 10% in both Nova appearances.
        var foreground = table[(family, dark, "foreground")];
        return Color.FromArgb(0x1A, foreground.R, foreground.G, foreground.B);
    }

    internal static async Task<ThemeSnapshot> Apply(string family, bool dark)
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        var config = new ThemeConfigBuilder().WithToken("ColorNeutral", Hex(Seed(family))).Build();
        var result = await manager.ApplyThemeAsync(
            new ThemeRequest(dark ? "ShadcnNova.Dark" : "ShadcnNova", config, ThemeTransitionReason.UserRequest));
        result.Status.ShouldBe(ThemeTransitionStatus.Committed);
        return manager.CurrentSnapshot!;
    }

    internal static Color Seed(string family) =>
        ShadcnAppendix.NeutralSeeds().Single(item => item.Family == family).Seed;

    internal static string Hex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    internal static string ArgbHex(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    internal static T Global<T>(ThemeSnapshot snapshot, string name)
    {
        snapshot.Registry.TryGetGlobalToken(name, out var token).ShouldBeTrue(name);
        return snapshot.GlobalTokenValues.Get<T>(token!.Slot);
    }

    internal static T Own<T>(ThemeSnapshot snapshot, string controlId, string tokenName)
    {
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", controlId), out var control)
                 .ShouldBeTrue(controlId);
        var token = control!.OwnTokens.Single(item => item.Name == tokenName);
        return snapshot.Controls[control.Slot].ControlTokenValues.Get<T>(token.Slot);
    }
}
