using System.Security.Cryptography;
using System.Text;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Resources;
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
    public void Control_Scoped_Global_Override_Honors_The_Same_Order()
    {
        var control = new ControlThemeConfigBuilder()
                      .WithAlgorithm(ControlAlgorithmMode.Global)
                      .WithToken("ControlOutlineWidth", "0")
                      .Build();
        var config = new ThemeConfigBuilder().WithControl(s_button, control).Build();

        var snapshot = ThemeTestSnapshotFactory.Compile(config, ThemeCompilerTests.CreateCompilerButtonDescriptor());

        snapshot.EffectiveGlobal<BoxShadows>(s_button, "InputActiveShadow")[0].Spread.ShouldBe(0);
        snapshot.Global<BoxShadows>("InputActiveShadow")[0].Spread.ShouldBe(2);
    }

    [Theory]
    [InlineData("Default", "055D31ED995FCB1975559327CCEEBE63AB05061F19477E514733CF69D4160E2D")]
    [InlineData("Dark", "447D1F42CE75162510242E596BC27FB4D787B502AC75066E5544B2E3BDF4E7E9")]
    [InlineData("DefaultCompact", "5D61427B4535CBFF8297B898B64A30670689CD898FB26649F876EEC2A9B2A3F3")]
    [InlineData("DarkCompact", "9E974118BE431D68D6489243433B2348AD3D146A447606136F08448F73FFF5AC")]
    public void Without_Explicit_Alias_Values_The_Result_Is_Unchanged(string chain, string expectedHash)
    {
        // Hashes pin every global slot (Name=FormattedValue per line, in slot order) as compiled before
        // explicit Alias values were applied ahead of the default Alias calculation.
        ThemeAlgorithm[] algorithms = chain switch
        {
            "Default"        => [ThemeAlgorithm.Default],
            "Dark"           => [ThemeAlgorithm.Dark],
            "DefaultCompact" => [ThemeAlgorithm.Default, ThemeAlgorithm.Compact],
            "DarkCompact"    => [ThemeAlgorithm.Dark, ThemeAlgorithm.Compact],
            _                => throw new ArgumentOutOfRangeException(nameof(chain))
        };
        var snapshot = ThemeTestSnapshotFactory.Compile(new ThemeConfigBuilder().WithAlgorithms(algorithms).Build());

        var dump = new StringBuilder();
        foreach (var descriptor in snapshot.Registry.GlobalTokens)
        {
            dump.Append(descriptor.Name)
                .Append('=')
                .Append(descriptor.Format(snapshot.GlobalTokenValues.GetValue(descriptor.Slot)))
                .Append('\n');
        }

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(dump.ToString())));
        hash.ShouldBe(expectedHash, dump.ToString());
    }

    [Fact]
    public void Explicit_Bits_Are_Per_Slot_And_Can_Be_Reset()
    {
        var token = new DesignToken();
        var outlineWidth = (int)SharedTokenKind.ControlOutlineWidth;
        var outlineColor = (int)SharedTokenKind.ColorControlOutline;

        token.IsExplicitSlot(outlineWidth).ShouldBeFalse();
        token.MarkExplicit(outlineWidth);
        token.IsExplicitSlot(outlineWidth).ShouldBeTrue();
        token.IsExplicitSlot(outlineColor).ShouldBeFalse();
        token.ResetExplicit();
        token.IsExplicitSlot(outlineWidth).ShouldBeFalse();
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
