using AtomUI.Theme;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class GlobalAliasExplicitOverrideTests
{
    private static readonly ControlTokenIdentity s_button = new("AtomUI", CompilerButtonToken.ID);

    [Fact]
    public void Explicit_ControlOutlineWidth_Drives_InputActiveShadow_Spread()
    {
        var snapshot = Compile(("ControlOutlineWidth", "0"));

        snapshot.Global<BoxShadows>("InputActiveShadow")[0].Spread.ShouldBe(0);
    }

    [Fact]
    public void Explicit_ColorControlOutline_Drives_InputActiveShadow_Color()
    {
        var snapshot = Compile(("ColorControlOutline", "#FF0000"));

        snapshot.Global<BoxShadows>("InputActiveShadow")[0].Color.ShouldBe(Colors.Red);
    }

    [Fact]
    public void Explicit_ColorErrorOutline_Drives_InputErrorActiveShadow_Color()
    {
        var snapshot = Compile(("ColorErrorOutline", "#FF0000"));

        snapshot.Global<BoxShadows>("InputErrorActiveShadow")[0].Color.ShouldBe(Colors.Red);
    }

    [Fact]
    public void Explicit_LineWidthFocus_Drives_FocusVisualBorderThickness()
    {
        var snapshot = Compile(("LineWidthFocus", "1"));

        snapshot.Global<Thickness>("FocusVisualBorderThickness").ShouldBe(new Thickness(1));
    }

    [Fact]
    public void Explicit_ControlPaddingHorizontal_Drives_InputPaddingLG_Left()
    {
        var snapshot = Compile(("ControlPaddingHorizontal", "20"));

        snapshot.Global<Thickness>("InputPaddingLG").Left
                .ShouldBe(20 - snapshot.Global<double>("LineWidth"));
    }

    [Fact]
    public void Explicit_UniformlyPaddingSM_Drives_InputPadding_Left()
    {
        var snapshot = Compile(("UniformlyPaddingSM", "5"));

        snapshot.Global<Thickness>("InputPadding").Left
                .ShouldBe(5 - snapshot.Global<double>("LineWidth"));
    }

    [Fact]
    public void Explicit_Derived_Alias_Still_Wins_Over_The_Recalculation()
    {
        var snapshot = Compile(
            ("ControlOutlineWidth", "0"),
            ("InputActiveShadow", "0 0 0 5 #FFFF0000"));

        var shadow = snapshot.Global<BoxShadows>("InputActiveShadow")[0];
        shadow.Spread.ShouldBe(5);
        shadow.Color.ShouldBe(Colors.Red);
    }

    [Fact]
    public void Explicit_Zero_Counts_As_Explicit()
    {
        var snapshot = Compile(("ControlOutlineWidth", "0"));

        snapshot.Global<double>("ControlOutlineWidth").ShouldBe(0);
        snapshot.Global<BoxShadows>("InputErrorActiveShadow")[0].Spread.ShouldBe(0);
        snapshot.Global<BoxShadows>("InputWarningActiveShadow")[0].Spread.ShouldBe(0);
    }

    [Fact]
    public void Control_Scoped_Override_Replaces_Only_That_Value()
    {
        var control = new ControlThemeConfigBuilder()
                      .WithToken("ControlOutlineWidth", "0")
                      .Build();
        var config = new ThemeConfigBuilder().WithControl(s_button, control).Build();

        var snapshot = ThemeTestSnapshotFactory.Compile(config, ThemeCompilerTests.CreateCompilerButtonDescriptor());

        // A control's Map or Alias override replaces that one value and triggers no derivation; only a control that
        // overrides a seed or fixes an axis is its own evaluation unit.
        snapshot.EffectiveGlobal<double>(s_button, "ControlOutlineWidth").ShouldBe(0);
        snapshot.EffectiveGlobal<BoxShadows>(s_button, "InputActiveShadow")[0].Spread.ShouldBe(2);
        snapshot.Global<double>("ControlOutlineWidth").ShouldBe(2);
    }

    [Theory]
    [InlineData("ControlOutlineWidth", "0",
        new[] { "ControlOutlineWidth", "InputActiveShadow", "InputErrorActiveShadow", "InputWarningActiveShadow" })]
    [InlineData("ColorControlOutline", "#FF0000", new[] { "ColorControlOutline", "InputActiveShadow" })]
    [InlineData("LineWidthFocus", "1", new[] { "FocusVisualBorderThickness", "LineWidthFocus" })]
    public void Explicit_Alias_Value_Changes_Only_Its_Documented_Derivations(
        string name, string value, string[] expectedChangedSlots)
    {
        // Every global slot is compared one by one with the same chain compiled without the explicit value; the
        // slots that differ are named, so a failure says which derivation appeared or disappeared.
        (ThemeAppearance Appearance, DesignDensity Density)[] variants =
        [
            (ThemeAppearance.Light, DesignDensity.Default),
            (ThemeAppearance.Dark, DesignDensity.Default),
            (ThemeAppearance.Light, DesignDensity.Compact),
            (ThemeAppearance.Dark, DesignDensity.Compact)
        ];
        foreach (var chain in variants)
        {
            ThemeConfigBuilder Variant() => new ThemeConfigBuilder().WithAppearance(chain.Appearance).WithDensity(chain.Density);
            var baseline = ThemeTestSnapshotFactory.Compile(Variant().Build());
            var overridden = ThemeTestSnapshotFactory.Compile(Variant().WithToken(name, value).Build());

            var changed = baseline.Registry.GlobalTokens
                                  .Where(descriptor => descriptor.Format(baseline.GlobalTokenValues.GetValue(descriptor.Slot)) !=
                                                       descriptor.Format(overridden.GlobalTokenValues.GetValue(descriptor.Slot)))
                                  .Select(static descriptor => descriptor.Name)
                                  .OrderBy(static slot => slot, StringComparer.Ordinal)
                                  .ToArray();

            changed.ShouldBe(expectedChangedSlots, chain.ToString());
        }
    }

    private static ThemeSnapshot Compile(params (string Name, string Value)[] tokens)
    {
        var builder = new ThemeConfigBuilder();
        foreach (var (name, value) in tokens)
        {
            builder.WithToken(name, value);
        }

        return ThemeTestSnapshotFactory.Compile(builder.Build());
    }
}
