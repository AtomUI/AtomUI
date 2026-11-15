using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Themes.Tests;

public class ShadcnThemePackageTests
{
    [AvaloniaFact]
    public void Registration_Preserves_Default_And_Provides_All_Family_Profiles()
    {
        var manager = Application.Current!.GetThemeManager()!;
        var ids = manager.AvailableThemes.Select(theme => theme.Id).ToHashSet();
        foreach (var family in new[] { "ShadcnZinc", "ShadcnSlate", "ShadcnStone" })
        {
            foreach (var suffix in new[] { "", ".Dark", ".Compact", ".DarkCompact" })
            {
                ids.ShouldContain(family + suffix);
            }
        }
        manager.AvailableThemes.Single(theme => theme.IsDefault).Id.ShouldBe("DaybreakBlue");
        manager.CurrentTheme!.ThemeId.ShouldBe("DaybreakBlue");
        manager.ThemeCatalogDiagnostics.ShouldBeEmpty();
    }

    [AvaloniaTheory]
    [InlineData("ShadcnZinc", "#18181B", "#FAFAFA", "#E4E4E7", "#71717A")]
    [InlineData("ShadcnSlate", "#0F172A", "#F8FAFC", "#E2E8F0", "#64748B")]
    [InlineData("ShadcnStone", "#1C1917", "#FAFAF9", "#E7E5E4", "#78716C")]
    public async Task Base_Maps_Pinned_Values_And_Family_Neutrals(string id, string primary,
        string layout, string border, string secondary)
    {
        var snapshot = await Apply(id);
        Global<Color>(snapshot, "ColorPrimary").ShouldBe(Color.Parse(primary));
        Global<Color>(snapshot, "ColorTextLightSolid").ShouldBe(Color.Parse("#FAFAFA"));
        Global<Color>(snapshot, "ColorText").ShouldBe(Color.Parse(primary));
        Global<Color>(snapshot, "ColorBgLayout").ShouldBe(Color.Parse(layout));
        Global<Color>(snapshot, "ColorBorder").ShouldBe(Color.Parse(border));
        Global<Color>(snapshot, "ColorTextSecondary").ShouldBe(Color.Parse(secondary));
        Global<CornerRadius>(snapshot, "BorderRadius").ShouldBe(new CornerRadius(6));
        Global<double>(snapshot, "ControlHeight").ShouldBe(36);
        Global<double>(snapshot, "ControlHeightSM").ShouldBe(32);
        Global<double>(snapshot, "ControlHeightLG").ShouldBe(40);
        Global<double>(snapshot, "LineWidth").ShouldBe(1);
        Global<bool>(snapshot, "EnableWaveSpirit").ShouldBeFalse();
    }

    // shadcn/ui: dropdown, popover and select content use shadow-md (BoxShadowsSecondary, read by popups and menus);
    // dialog, sheet and toast use shadow-lg (BoxShadows). The same Tailwind values apply to every profile.
    [AvaloniaTheory]
    [InlineData("ShadcnZinc")]
    [InlineData("ShadcnSlate")]
    [InlineData("ShadcnStone")]
    public async Task Every_Profile_Maps_Popup_And_Dialog_Shadows(string id)
    {
        foreach (var suffix in new[] { "", ".Dark", ".Compact", ".DarkCompact" })
        {
            var snapshot = await Apply(id + suffix);
            Global<BoxShadows>(snapshot, "BoxShadowsSecondary")
                .ShouldBe(BoxShadows.Parse("0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000"), id + suffix);
            Global<BoxShadows>(snapshot, "BoxShadows")
                .ShouldBe(BoxShadows.Parse("0 10 15 -3 #1A000000, 0 4 6 -4 #1A000000"), id + suffix);
        }
    }

    // shadcn popups pair shadow-md with a 1px border; shadow-md casts downward, so without the border a popup's top edge
    // disappears on a same-coloured page. Popup draws the border as a spread ring in ColorBorder; arrow flyouts and
    // other BoxShadowsSecondary consumers keep the plain shadow.
    [AvaloniaTheory]
    [InlineData("ShadcnZinc")]
    [InlineData("ShadcnSlate")]
    [InlineData("ShadcnStone")]
    public async Task Popup_Shadows_Carry_The_Border_Ring(string id)
    {
        foreach (var suffix in new[] { "", ".Dark", ".Compact", ".DarkCompact" })
        {
            var snapshot = await Apply(id + suffix);
            var border = Global<Color>(snapshot, "ColorBorder");
            var expected = new BoxShadows(new BoxShadow { Spread = 1, Color = border },
                [.. BoxShadows.Parse("0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000")]);
            snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", "Popup"), out var popup).ShouldBeTrue();
            foreach (var name in new[] { "PopupRootShadow", "OverlayHostShadow" })
            {
                var token = popup!.OwnTokens.Single(token => token.Name == name);
                snapshot.Controls[popup.Slot].ControlTokenValues.Get<BoxShadows>(token.Slot)
                        .ShouldBe(expected, $"{id}{suffix} {name}");
            }
        }
    }

    [AvaloniaTheory]
    [InlineData("ShadcnZinc", "#18181B", "#27272A", "#A1A1AA")]
    [InlineData("ShadcnSlate", "#0F172A", "#1E293B", "#94A3B8")]
    [InlineData("ShadcnStone", "#1C1917", "#292524", "#A8A29E")]
    public async Task Four_Actual_Requests_Keep_Readable_Dark_And_Reduce_Compact_Density(
        string id, string container, string border, string secondary)
    {
        foreach (var suffix in new[] { "", ".Dark", ".Compact", ".DarkCompact" })
        {
            var snapshot = await Apply(id + suffix);
            var dark = suffix.Contains("Dark");
            var compact = suffix.Contains("Compact");
            snapshot.Appearance.ShouldBe(dark ? ThemeAppearance.Dark : ThemeAppearance.Light);
            Global<double>(snapshot, "ControlHeight").ShouldBe(compact ? 32 : 36);
            Global<double>(snapshot, "ControlHeightSM").ShouldBe(compact ? 28 : 32);
            Global<double>(snapshot, "ControlHeightLG").ShouldBe(compact ? 36 : 40);
            Global<double>(snapshot, "PaddingContentHorizontal").ShouldBe(compact ? 8 : 12);
            Global<bool>(snapshot, "EnableWaveSpirit").ShouldBeFalse();
            if (dark)
            {
                Global<Color>(snapshot, "ColorBgContainer").ShouldBe(Color.Parse(container));
                Global<Color>(snapshot, "ColorText").ShouldBe(Color.Parse("#FAFAFA"));
                Global<Color>(snapshot, "ColorBorder").ShouldBe(Color.Parse(border));
                Global<Color>(snapshot, "ColorTextSecondary").ShouldBe(Color.Parse(secondary));
                snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", "CheckBox"), out var checkBox).ShouldBeTrue();
                snapshot.Registry.TryGetGlobalToken("ColorWhite", out var white).ShouldBeTrue();
                var checkMark = snapshot.Controls[checkBox!.Slot].GetEffectiveGlobalValue<Color>(snapshot.GlobalTokenValues, white!.Slot);
                checkMark.ShouldBe(Color.Parse(container));
                snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", "RadioButton"), out var radio).ShouldBeTrue();
                var dot = radio!.OwnTokens.Single(token => token.Name == "RadioColor");
                var dotColor = snapshot.Controls[radio.Slot].ControlTokenValues.Get<Color>(dot.Slot);
                dotColor.ShouldBe(Color.Parse(container));
                Contrast(checkMark, Global<Color>(snapshot, "ColorPrimary")).ShouldBeGreaterThan(7);
                Contrast(dotColor, Global<Color>(snapshot, "ColorPrimary")).ShouldBeGreaterThan(7);
                Global<Color>(snapshot, "ColorWhite").ShouldBe(Colors.White);
            }
            Contrast(Global<Color>(snapshot, "ColorText"), Global<Color>(snapshot, "ColorBgContainer")).ShouldBeGreaterThan(7);
            Contrast(Global<Color>(snapshot, "ColorTextLightSolid"), Global<Color>(snapshot, "ColorPrimary")).ShouldBeGreaterThan(7);
        }
    }

    [AvaloniaTheory]
    [InlineData("Zinc")]
    [InlineData("Slate")]
    [InlineData("Stone")]
    public void Profiles_Declare_Design_Language_And_Brand_Support(string family)
    {
        var manager = Application.Current!.GetThemeManager()!;
        var expected = new (string Suffix, ThemeAppearance Appearance, ThemeDensity Density)[]
        {
            ("", ThemeAppearance.Light, ThemeDensity.Normal),
            (".Dark", ThemeAppearance.Dark, ThemeDensity.Normal),
            (".Compact", ThemeAppearance.Light, ThemeDensity.Compact),
            (".DarkCompact", ThemeAppearance.Dark, ThemeDensity.Compact)
        };
        foreach (var (suffix, appearance, density) in expected)
        {
            var info = manager.AvailableThemes.Single(theme => theme.Id == "Shadcn" + family + suffix);
            info.DesignLanguage.ShouldBe("Shadcn." + family);
            info.BrandColor.ShouldBe(ThemeBrandColorSupport.Fixed);
            info.Appearance.ShouldBe(appearance);
            info.Density.ShouldBe(density);
        }

        manager.ThemeCatalogDiagnostics.ShouldBeEmpty();
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
