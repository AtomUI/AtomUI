using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.ContractTests;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Themes.Tests;

public class PrimeNGThemePackageTests
{
    [AvaloniaFact]
    public void UsePrimeNGThemes_Registers_Three_Preset_Families_Without_Changing_Default()
    {
        var manager = Application.Current!.GetThemeManager()!;
        var expected = new[] { "PrimeNGAura", "PrimeNGLara", "PrimeNGNora" }
            .SelectMany(id => new[] { id, id + ".Dark", id + ".Compact", id + ".DarkCompact" })
            .Append("DaybreakBlue");
        manager.AvailableThemes.Where(theme => theme.Id == "DaybreakBlue" || theme.Id.StartsWith("PrimeNG", StringComparison.Ordinal))
            .Select(theme => theme.Id).ShouldBe(expected, ignoreOrder: true);
        manager.AvailableThemes.Single(theme => theme.IsDefault).Id.ShouldBe("DaybreakBlue");
        manager.CurrentTheme!.ThemeId.ShouldBe("DaybreakBlue");
        manager.ThemeCatalogDiagnostics.ShouldBeEmpty();
    }

    [AvaloniaTheory]
    [InlineData("PrimeNGAura")]
    [InlineData("PrimeNGLara")]
    [InlineData("PrimeNGNora")]
    public async Task Each_Preset_Applies_Light_Dark_Compact(string id)
    {
        foreach (var suffix in new[] { "", ".Dark", ".Compact", ".DarkCompact" })
        {
            var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
            var result = await manager.ApplyThemeAsync(new ThemeRequest(id + suffix, null, ThemeTransitionReason.UserRequest), TestContext.Current.CancellationToken);
            result.Status.ShouldBe(ThemeTransitionStatus.Committed);
            manager.CurrentTheme!.Appearance.ShouldBe(suffix.Contains("Dark")
                ? ThemeAppearance.Dark : ThemeAppearance.Light);
            var snapshot = manager.CurrentSnapshot!;
            var normalHeight = id == "PrimeNGLara" ? 44d : 40d;
            Global<double>(snapshot, "ControlHeight").ShouldBe(normalHeight - (suffix.Contains("Compact") ? 4 : 0));
            if (suffix.Contains("Compact"))
            {
                var height = normalHeight - 4;
                Global<double>(snapshot, "ControlHeightSM").ShouldBe(height * 0.75);
                Global<double>(snapshot, "ControlHeightLG").ShouldBe(height * 1.25);
                Global<double>(snapshot, "PaddingContentHorizontal").ShouldBe(8);
            }
            if (id == "PrimeNGLara" && !suffix.Contains("Compact"))
            {
                Own<Thickness>(snapshot, "Select", "OptionPadding").ShouldBe(new Thickness(16, 10));
                Own<Thickness>(snapshot, "Select", "PopupContentPadding").ShouldBe(new Thickness(0, 8));
            }
            if (suffix.Contains("Dark"))
            {
                Global<Color>(snapshot, "ColorBgContainer").ShouldBe(Color.Parse("#09090B"));
                Global<Color>(snapshot, "ColorText").ShouldBe(Colors.White);
                Effective<Color>(snapshot, "Card", "ColorBgContainer").ShouldBe(Color.Parse("#18181B"));
                Own<Color>(snapshot, "ToolTip", "ToolTipColor").ShouldBe(Colors.White);
            }
        }
    }

    [AvaloniaFact]
    public async Task Aura_Maps_Semantic_Tokens()
    {
        var snapshot = await Apply("PrimeNGAura");
        foreach (var (name, value) in new Dictionary<string, string>
                 {
                     ["ColorPrimary"] = "#10B981", ["ColorPrimaryHover"] = "#059669",
                     ["ColorPrimaryActive"] = "#047857", ["ColorText"] = "#334155",
                     ["ColorTextSecondary"] = "#64748B", ["ColorTextDescription"] = "#64748B",
                     ["ColorBorder"] = "#CBD5E1", ["ColorBorderSecondary"] = "#E2E8F0",
                     ["ColorBgContainer"] = "#FFFFFF", ["ColorFillTertiary"] = "#F1F5F9",
                     ["ColorBgContainerDisabled"] = "#E2E8F0", ["ColorBgMask"] = "#66000000",
                     ["ColorPrimaryBg"] = "#ECFDF5", ["ColorPrimaryText"] = "#047857",
                     ["ColorTextLightSolid"] = "#FFFFFF", ["ColorError"] = "#F87171"
                 })
        {
            Global<Color>(snapshot, name).ShouldBe(Color.Parse(value));
        }
        foreach (var (name, value) in new Dictionary<string, double>
                 {
                     ["ControlHeight"] = 40, ["ControlHeightSM"] = 36, ["ControlHeightLG"] = 48,
                     ["PaddingContentHorizontal"] = 12, ["FontSizeSM"] = 12, ["FontSizeLG"] = 16,
                     ["ControlOutlineWidth"] = 0
                 })
        {
            Global<double>(snapshot, name).ShouldBe(value);
        }
        Global<CornerRadius>(snapshot, "BorderRadius").ShouldBe(new CornerRadius(6));
        Global<CornerRadius>(snapshot, "BorderRadiusXS").ShouldBe(new CornerRadius(2));
        Global<CornerRadius>(snapshot, "BorderRadiusSM").ShouldBe(new CornerRadius(4));
        Global<CornerRadius>(snapshot, "BorderRadiusLG").ShouldBe(new CornerRadius(8));
        Global<Thickness>(snapshot, "FocusVisualBorderThickness").ShouldBe(new Thickness(1));
        Global<BoxShadows>(snapshot, "InputActiveShadow")[0].Spread.ShouldBe(0);
        Global<TimeSpan>(snapshot, "MotionDurationMid").ShouldBe(TimeSpan.FromMilliseconds(200));
        Global<BoxShadows>(snapshot, "BoxShadowsSecondary").Count.ShouldBe(2);
    }

    [AvaloniaFact]
    public async Task Aura_Control_Overrides_Reach_Their_Owners()
    {
        var snapshot = await Apply("PrimeNGAura");
        Own<FontWeight>(snapshot, "Button", "FontWeight").ShouldBe(FontWeight.Medium);
        Own<double>(snapshot, "CheckBox", "CheckIndicatorSize").ShouldBe(20);
        Own<double>(snapshot, "CheckBox", "CheckedMarkSize").ShouldBe(14);
        Own<Thickness>(snapshot, "Select", "OptionPadding").ShouldBe(new Thickness(12, 8));
        Own<Thickness>(snapshot, "Select", "PopupContentPadding").ShouldBe(new Thickness(4));
        Own<Thickness>(snapshot, "Card", "BodyPadding").ShouldBe(new Thickness(20));
        Own<BoxShadows>(snapshot, "Card", "RestingShadows").Count.ShouldBe(2);
        Own<Color>(snapshot, "ToolTip", "ToolTipBackground").ShouldBe(Color.Parse("#334155"));
        Own<CornerRadius>(snapshot, "ToolTip", "ToolTipCornerRadius").ShouldBe(new CornerRadius(6));
        Own<Thickness>(snapshot, "ToolTip", "ContentPadding").ShouldBe(new Thickness(12, 8));
        Effective<CornerRadius>(snapshot, "Card", "BorderRadiusLG").ShouldBe(new CornerRadius(12));
        Effective<CornerRadius>(snapshot, "Dialog", "BorderRadiusLG").ShouldBe(new CornerRadius(12));
        Own<Thickness>(snapshot, "Dialog", "ContentPadding").ShouldBe(new Thickness(20));
        Own<double>(snapshot, "Dialog", "HeaderFontSize").ShouldBe(20);
    }

    [AvaloniaTheory]
    [InlineData("PrimeNGAura", "#3F000000", "#E2E8F0")]
    [InlineData("PrimeNGAura.Dark", "#3FFFFFFF", "#3F3F46")]
    [InlineData("PrimeNGAura.Compact", "#3F000000", "#E2E8F0")]
    [InlineData("PrimeNGAura.DarkCompact", "#3FFFFFFF", "#3F3F46")]
    public async Task Aura_Disabled_Text_Stays_As_Readable_As_The_Old_Profiles(
        string id, string oldDisabledText, string oldDisabledBackground)
    {
        // The floor is what the pre-algorithm profiles compiled to (Ant's translucent disabled text on the explicit
        // disabled background): 1.79 light, 2.06 dark. Ant's formula on the PrimeNG neutrals would give 1.20 / 1.35.
        var floor = ContrastRatio.Of(Color.Parse(oldDisabledText), Color.Parse(oldDisabledBackground));
        var snapshot = await Apply(id);
        var ratio = ContrastRatio.Of(Global<Color>(snapshot, "ColorTextDisabled"),
            Global<Color>(snapshot, "ColorBgContainerDisabled"));
        ratio.ShouldBeGreaterThanOrEqualTo(floor, $"{id}: {ratio:F2} < {floor:F2}");
    }

    [AvaloniaFact]
    public async Task Nora_Disables_Motion_And_Uses_Radius_2()
    {
        var snapshot = await Apply("PrimeNGNora");
        Global<bool>(snapshot, "EnableMotion").ShouldBeFalse();
        Global<CornerRadius>(snapshot, "BorderRadius").ShouldBe(new CornerRadius(2));
        Global<Color>(snapshot, "ColorPrimary").ShouldBe(Color.Parse("#059669"));
        Global<Color>(snapshot, "ColorText").ShouldBe(Color.Parse("#0F172A"));
        Global<Color>(snapshot, "ColorBorderSecondary").ShouldBe(Color.Parse("#00000000"));
        Own<FontWeight>(snapshot, "Button", "FontWeight").ShouldBe(FontWeight.Bold);
        Effective<CornerRadius>(snapshot, "Card", "BorderRadiusLG").ShouldBe(new CornerRadius(2));
        foreach (var profile in new[] { "PrimeNGNora.Dark", "PrimeNGNora.DarkCompact" })
        {
            var dark = await Apply(profile);
            Global<Color>(dark, "ColorPrimaryBg").ShouldBe(Color.Parse("#10B981"));
            Global<Color>(dark, "ColorPrimaryText").ShouldBe(Color.Parse("#09090B"));
            Own<Color>(dark, "Steps", "FinishIconColor").ShouldBe(Color.Parse("#09090B"));
            Own<Color>(dark, "Steps", "ProcessIconColor").ShouldBe(Color.Parse("#09090B"));
            Own<Color>(dark, "Steps", "FinishTitleColor").ShouldBe(Colors.White);
        }
    }

    [AvaloniaFact]
    public async Task Lara_Uses_Outline_Glow_And_Larger_Controls()
    {
        var snapshot = await Apply("PrimeNGLara");
        Global<double>(snapshot, "ControlOutlineWidth").ShouldBe(3);
        Global<Color>(snapshot, "ColorControlOutline").ShouldBe(Color.Parse("#A7F3D0"));
        var inputActiveShadow = Global<BoxShadows>(snapshot, "InputActiveShadow")[0];
        inputActiveShadow.Spread.ShouldBe(3);
        inputActiveShadow.Color.ShouldBe(Color.Parse("#A7F3D0"));
        Global<Thickness>(snapshot, "FocusVisualBorderThickness").ShouldBe(new Thickness(0));
        Global<double>(snapshot, "ControlHeight").ShouldBe(44);
        Own<double>(snapshot, "CheckBox", "CheckIndicatorSize").ShouldBe(24);
        Own<Thickness>(snapshot, "Select", "OptionPadding").ShouldBe(new Thickness(16, 10));
        Own<Thickness>(snapshot, "Select", "PopupContentPadding").ShouldBe(new Thickness(0, 8));
        Own<FontWeight>(snapshot, "Button", "FontWeight").ShouldBe(FontWeight.SemiBold);
    }

    private static async Task<ThemeSnapshot> Apply(string id)
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        (await manager.ApplyThemeAsync(new ThemeRequest(id, null, ThemeTransitionReason.UserRequest), TestContext.Current.CancellationToken))
            .Status.ShouldBe(ThemeTransitionStatus.Committed);
        return manager.CurrentSnapshot!;
    }

    private static T Global<T>(ThemeSnapshot snapshot, string name)
    {
        snapshot.Registry.TryGetGlobalToken(name, out var token).ShouldBeTrue();
        return snapshot.GlobalTokenValues.Get<T>(token!.Slot);
    }

    private static ControlTokenDescriptor Owner(ThemeSnapshot snapshot, string id)
    {
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", id), out var control).ShouldBeTrue();
        return control!;
    }

    private static T Own<T>(ThemeSnapshot snapshot, string owner, string name)
    {
        var control = Owner(snapshot, owner);
        var token = control.OwnTokens.Single(token => token.Name == name);
        return snapshot.Controls[control.Slot].ControlTokenValues.Get<T>(token.Slot);
    }

    private static T Effective<T>(ThemeSnapshot snapshot, string owner, string name)
    {
        var control = Owner(snapshot, owner);
        snapshot.Registry.TryGetGlobalToken(name, out var token).ShouldBeTrue();
        return snapshot.Controls[control.Slot].GetEffectiveGlobalValue<T>(snapshot.GlobalTokenValues, token!.Slot);
    }
}
