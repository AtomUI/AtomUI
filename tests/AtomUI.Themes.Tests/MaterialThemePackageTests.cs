using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Themes.Tests;

public class MaterialThemePackageTests
{
    [AvaloniaFact]
    public void Registration_Preserves_Default_And_Provides_Four_Compiled_Profiles()
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        manager.AvailableThemes.Where(theme => theme.Id.StartsWith("Material", StringComparison.Ordinal))
            .Select(theme => theme.Id).ShouldBe(new[] { "Material", "Material.Dark", "Material.Compact", "Material.DarkCompact" }, ignoreOrder: true);
        manager.AvailableThemes.Single(theme => theme.IsDefault).Id.ShouldBe("DaybreakBlue");
        manager.CurrentTheme!.ThemeId.ShouldBe("DaybreakBlue");
        manager.ThemeCatalogDiagnostics.ShouldBeEmpty();
        manager.CompiledThemeCatalog.ResolverSlices.Single(slice => slice.ResolverId == "AtomUI.Themes.Material")
            .Definitions.ShouldAllBe(entry => entry.Revision.ProgramDigest != null);
    }

    [AvaloniaFact]
    public async Task Base_Maps_Approved_Material_Values()
    {
        var snapshot = await Apply("Material");
        Global<Color>(snapshot, "ColorPrimary").ShouldBe(Color.Parse("#6750A4"));
        Global<Color>(snapshot, "ColorTextLightSolid").ShouldBe(Colors.White);
        Global<CornerRadius>(snapshot, "BorderRadius").ShouldBe(new CornerRadius(4));
        Global<double>(snapshot, "ControlHeight").ShouldBe(40);
        Global<FontWeight>(snapshot, "FontWeightStrong").ShouldBe(FontWeight.Medium);
        Global<BoxShadows>(snapshot, "BoxShadowsSecondary").ShouldBe(BoxShadows.Parse("0 1 3 0 #33000000"));
        Global<BoxShadows>(snapshot, "BoxShadows").ShouldBe(BoxShadows.Parse("0 4 8 3 #26000000"));
    }

    [AvaloniaTheory]
    [InlineData("")]
    [InlineData(".Dark")]
    [InlineData(".Compact")]
    [InlineData(".DarkCompact")]
    public async Task Actual_Requests_Keep_Readable_Surfaces_And_Reduce_Compact_Density(string suffix)
    {
        var snapshot = await Apply("Material" + suffix);
        var dark = suffix.Contains("Dark");
        var compact = suffix.Contains("Compact");
        snapshot.Appearance.ShouldBe(dark ? ThemeAppearance.Dark : ThemeAppearance.Light);
        Global<double>(snapshot, "ControlHeight").ShouldBe(compact ? 36 : 40);
        Global<double>(snapshot, "PaddingContentHorizontal").ShouldBe(compact ? 8 : 12);
        Global<Color>(snapshot, "ColorBgContainer").ShouldBe(Color.Parse(dark ? "#1C1B1F" : "#FFFBFE"));
        Global<Color>(snapshot, "ColorText").ShouldBe(Color.Parse(dark ? "#E6E1E5" : "#1C1B1F"));
        Contrast(Global<Color>(snapshot, "ColorText"), Global<Color>(snapshot, "ColorBgContainer")).ShouldBeGreaterThan(7);
        Contrast(Global<Color>(snapshot, "ColorTextLightSolid"), Global<Color>(snapshot, "ColorPrimary")).ShouldBeGreaterThan(4.5);
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", "CheckBox"), out var checkBox).ShouldBeTrue();
        snapshot.Registry.TryGetGlobalToken("ColorWhite", out var white).ShouldBeTrue();
        var checkMark = snapshot.Controls[checkBox!.Slot].GetEffectiveGlobalValue<Color>(snapshot.GlobalTokenValues, white!.Slot);
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", "RadioButton"), out var radio).ShouldBeTrue();
        var dot = radio!.OwnTokens.Single(token => token.Name == "RadioColor");
        var dotColor = snapshot.Controls[radio.Slot].ControlTokenValues.Get<Color>(dot.Slot);
        var onPrimary = Color.Parse(dark ? "#381E72" : "#FFFFFF");
        if (!dark)
        {
            Global<Color>(snapshot, "ColorPrimary").ShouldBe(Color.Parse("#6750A4"));
        }
        Global<Color>(snapshot, "ColorTextLightSolid").ShouldBe(onPrimary);
        Global<Color>(snapshot, "ColorWhite").ShouldBe(Colors.White);
        checkMark.ShouldBe(onPrimary);
        dotColor.ShouldBe(onPrimary);
        Contrast(checkMark, Global<Color>(snapshot, "ColorPrimary")).ShouldBeGreaterThan(4.5);
        Contrast(dotColor, Global<Color>(snapshot, "ColorPrimary")).ShouldBeGreaterThan(4.5);
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
