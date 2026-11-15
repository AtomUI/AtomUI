using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Themes.Tests;

public class BootstrapThemePackageTests
{
    [AvaloniaFact]
    public void Registration_Preserves_Default_And_Provides_Four_Compiled_Profiles()
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        manager.AvailableThemes.Where(theme => theme.Id.StartsWith("Bootstrap", StringComparison.Ordinal))
            .Select(theme => theme.Id).ShouldBe(new[] { "Bootstrap", "Bootstrap.Dark", "Bootstrap.Compact", "Bootstrap.DarkCompact" }, ignoreOrder: true);
        manager.AvailableThemes.Single(theme => theme.IsDefault).Id.ShouldBe("DaybreakBlue");
        manager.CurrentTheme!.ThemeId.ShouldBe("DaybreakBlue");
        manager.ThemeCatalogDiagnostics.ShouldBeEmpty();
        manager.CompiledThemeCatalog.ResolverSlices.Single(slice => slice.ResolverId == "AtomUI.Themes.Bootstrap")
            .Definitions.ShouldAllBe(entry => entry.Revision.ProgramDigest != null);
    }

    [AvaloniaFact]
    public async Task Base_Maps_Approved_Bootstrap_Values()
    {
        var snapshot = await Apply("Bootstrap");
        Global<Color>(snapshot, "ColorPrimary").ShouldBe(Color.Parse("#0087FE"));
        Global<Color>(snapshot, "ColorSuccess").ShouldBe(Color.Parse("#00B15A"));
        Global<Color>(snapshot, "ColorWarning").ShouldBe(Color.Parse("#FFC900"));
        Global<Color>(snapshot, "ColorError").ShouldBe(Color.Parse("#E62845"));
        Global<Color>(snapshot, "ColorInfo").ShouldBe(Color.Parse("#00B4F8"));
        Global<double>(snapshot, "FontSize").ShouldBe(16);
        Global<double>(snapshot, "ControlHeight").ShouldBe(38);
        Global<double>(snapshot, "ControlHeightSM").ShouldBe(32);
        Global<double>(snapshot, "ControlHeightLG").ShouldBe(44);
        Global<CornerRadius>(snapshot, "BorderRadius").ShouldBe(new CornerRadius(8));
        Global<CornerRadius>(snapshot, "BorderRadiusLG").ShouldBe(new CornerRadius(12));
        Global<double>(snapshot, "ControlOutlineWidth").ShouldBe(3);
        Global<Color>(snapshot, "ColorControlOutline").ShouldBe(Color.Parse("#6FC8FF"));
        var inputActiveShadow = Global<BoxShadows>(snapshot, "InputActiveShadow")[0];
        inputActiveShadow.Spread.ShouldBe(3);
        inputActiveShadow.Color.ShouldBe(Color.Parse("#6FC8FF"));
    }

    [AvaloniaTheory]
    [InlineData("")]
    [InlineData(".Dark")]
    [InlineData(".Compact")]
    [InlineData(".DarkCompact")]
    public async Task Actual_Requests_Keep_Readable_Surfaces_And_Reduce_Compact_Density(string suffix)
    {
        var snapshot = await Apply("Bootstrap" + suffix);
        var dark = suffix.Contains("Dark");
        var compact = suffix.Contains("Compact");
        snapshot.Appearance.ShouldBe(dark ? ThemeAppearance.Dark : ThemeAppearance.Light);
        Global<double>(snapshot, "ControlHeight").ShouldBe(compact ? 34 : 38);
        Global<double>(snapshot, "ControlHeightSM").ShouldBe(compact ? 28 : 32);
        Global<double>(snapshot, "ControlHeightLG").ShouldBe(compact ? 40 : 44);
        Global<double>(snapshot, "PaddingContentHorizontal").ShouldBe(compact ? 8 : 12);
        Global<Color>(snapshot, "ColorBgContainer").ShouldBe(Color.Parse(dark ? "#080A0C" : "#FFFFFF"));
        Global<Color>(snapshot, "ColorText").ShouldBe(Color.Parse(dark ? "#F1F2F3" : "#171A1D"));
        Global<Color>(snapshot, "ColorControlOutline").ShouldBe(Color.Parse(dark ? "#0064BA" : "#6FC8FF"));
        Contrast(Global<Color>(snapshot, "ColorText"), Global<Color>(snapshot, "ColorBgContainer")).ShouldBeGreaterThan(7);
        // Bootstrap 6 pairs white contrast text with blue-500 (about 3.6:1); 3:1 is the WCAG floor for UI components.
        Contrast(Global<Color>(snapshot, "ColorTextLightSolid"), Global<Color>(snapshot, "ColorPrimary")).ShouldBeGreaterThan(3);
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", "CheckBox"), out var checkBox).ShouldBeTrue();
        snapshot.Registry.TryGetGlobalToken("ColorWhite", out var white).ShouldBeTrue();
        var checkMark = snapshot.Controls[checkBox!.Slot].GetEffectiveGlobalValue<Color>(snapshot.GlobalTokenValues, white!.Slot);
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", "RadioButton"), out var radio).ShouldBeTrue();
        var dot = radio!.OwnTokens.Single(token => token.Name == "RadioColor");
        var dotColor = snapshot.Controls[radio.Slot].ControlTokenValues.Get<Color>(dot.Slot);
        checkMark.ShouldBe(Colors.White);
        dotColor.ShouldBe(Colors.White);
        Contrast(checkMark, Global<Color>(snapshot, "ColorPrimary")).ShouldBeGreaterThan(3);
        Contrast(dotColor, Global<Color>(snapshot, "ColorPrimary")).ShouldBeGreaterThan(3);
    }

    private static async Task<ThemeSnapshot> Apply(string id)
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        var result = await manager.ApplyThemeAsync(new ThemeRequest(id, null, ThemeTransitionReason.UserRequest), TestContext.Current.CancellationToken);
        result.Status.ShouldBe(ThemeTransitionStatus.Committed);
        return manager.CurrentSnapshot!;
    }

    private static T Global<T>(ThemeSnapshot snapshot, string name)
    {
        snapshot.Registry.TryGetGlobalToken(name, out var token).ShouldBeTrue();
        return snapshot.GlobalTokenValues.Get<T>(token!.Slot);
    }

    private static double Contrast(Color first, Color second)
    {
        static double Linear(byte channel)
        {
            var value = channel / 255d;
            return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }
        static double Luminance(Color color) =>
            0.2126 * Linear(color.R) + 0.7152 * Linear(color.G) + 0.0722 * Linear(color.B);
        var a = Luminance(first);
        var b = Luminance(second);
        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }
}
