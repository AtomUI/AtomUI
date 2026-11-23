using AtomUI.Generated.AtomUICore;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// The single-pass pipeline of the compiler (spec 5): seeds, language tokens, global tokens, then each control's view,
/// with explicit values frozen against the derivations, every derivation run once, and failures reported where they
/// belong (spec 5.5).
/// </summary>
public class DesignLanguageCompilationTests
{
    [Fact]
    public void Each_Language_Step_Runs_Once_And_A_Control_View_Is_Derived_Once()
    {
        var language = new ProbeLanguage();
        ViewProbeDerivation.Calls = 0;
        var snapshot = Compile(language);

        language.DefineSeedsCalls.ShouldBe(1);
        language.DeriveLanguageCalls.ShouldBe(1);
        language.DeriveGlobalCalls.ShouldBe(1);
        ViewProbeDerivation.Calls.ShouldBe(0);
        _ = snapshot.ViewProbe();
        _ = snapshot.ViewProbe();
        ViewProbeDerivation.Calls.ShouldBe(1);
        language.DeriveGlobalCalls.ShouldBe(1);
    }

    [Fact]
    public void Explicit_Map_Value_Is_Frozen_Against_DeriveGlobal()
    {
        var snapshot = Compile(root: builder => builder.WithToken(nameof(DesignToken.ColorPrimaryHover), "#FF0000"));

        snapshot.Global<Color>(nameof(DesignToken.ColorPrimaryHover)).ShouldBe(Colors.Red);
    }

    [Fact]
    public void Explicit_Seed_Feeds_DeriveGlobal()
    {
        var snapshot = Compile(root: builder => builder.WithToken(nameof(DesignToken.ColorPrimary), "#00FF00"));

        snapshot.Global<Color>(nameof(DesignToken.ColorPrimaryHover)).ShouldBe(Color.FromArgb(0x80, 0, 0xFF, 0));
    }

    [Theory]
    [InlineData(false, false, 32d)]
    [InlineData(true, false, 24d)]
    [InlineData(false, true, 32d)]
    [InlineData(true, true, 24d)]
    public void The_Variant_Axes_Reach_The_Derivation(bool compact, bool dark, double height)
    {
        var snapshot = Compile(root: builder => builder
                                                .WithDensity(compact ? DesignDensity.Compact : DesignDensity.Default)
                                                .WithAppearance(dark ? ThemeAppearance.Dark : ThemeAppearance.Light));

        snapshot.Global<double>(nameof(DesignToken.ControlHeight)).ShouldBe(height);
        snapshot.Global<Color>(nameof(DesignToken.ColorBgBase)).ShouldBe(dark ? Colors.Black : Colors.White);
        snapshot.Variant.ShouldBe(new DesignVariant(ProbeLanguage.ProbeStyle,
            dark ? ThemeAppearance.Dark : ThemeAppearance.Light,
            compact ? DesignDensity.Compact : DesignDensity.Default, DesignIdiom.Desktop));
    }

    [Fact]
    public void A_Control_Derivation_Writes_The_Control_View()
    {
        var snapshot = Compile();

        snapshot.Own<double>(nameof(ViewProbeToken.BaseSize)).ShouldBe(32);
        snapshot.Own<double>(nameof(ViewProbeToken.DerivedHeight)).ShouldBe(42);
        snapshot.Own<Color>(nameof(ViewProbeToken.Accent)).ShouldBe(Colors.White);
    }

    [Fact]
    public void Explicit_Control_Token_Is_Frozen_And_Derived_Tokens_Follow_It()
    {
        var snapshot = Compile(control: local => local.WithToken(nameof(ViewProbeToken.BaseSize), "7"));

        snapshot.Own<double>(nameof(ViewProbeToken.BaseSize)).ShouldBe(7);
        snapshot.Own<double>(nameof(ViewProbeToken.DerivedHeight)).ShouldBe(17);
    }

    [Fact]
    public void An_Explicit_Zero_Is_Frozen_Like_Any_Other_Value()
    {
        var snapshot = Compile(control: local => local.WithToken(nameof(ViewProbeToken.BaseSize), "0"));

        snapshot.Own<double>(nameof(ViewProbeToken.BaseSize)).ShouldBe(0);
        snapshot.Own<double>(nameof(ViewProbeToken.DerivedHeight)).ShouldBe(10);
    }

    [Fact]
    public void A_Global_Write_Of_A_Control_Derivation_Applies_To_That_Control_Only()
    {
        var snapshot = Compile();

        snapshot.EffectiveGlobal<double>(ViewProbeToken.ControlIdentity, nameof(DesignToken.LineWidthBold)).ShouldBe(3);
        snapshot.Global<double>(nameof(DesignToken.LineWidthBold)).ShouldNotBe(3);
        snapshot.ViewProbe().EffectiveGlobalTokenDelta.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Control_Scoped_Global_Override_Is_Frozen_Against_The_Control_Derivation()
    {
        var snapshot = Compile(control: local => local.WithToken(nameof(DesignToken.LineWidthBold), "5"));

        snapshot.EffectiveGlobal<double>(ViewProbeToken.ControlIdentity, nameof(DesignToken.LineWidthBold)).ShouldBe(5);
    }

    [Fact]
    public void A_Control_Derivation_That_Writes_A_Seed_Fails_When_The_Control_Is_First_Queried()
    {
        ViewProbeDerivation.WriteSeed = true;
        try
        {
            var snapshot = Compile();

            var exception = Should.Throw<ThemeDerivationException>(() => snapshot.ViewProbe());
            exception.Language.ShouldBe("Probe");
            exception.Control.ShouldBe(ViewProbeToken.ControlIdentity);
            exception.InnerException!.Message.ShouldContain(nameof(DesignToken.ColorPrimary));
        }
        finally
        {
            ViewProbeDerivation.WriteSeed = false;
        }
    }

    [Fact]
    public void A_Control_That_Fixes_The_Appearance_Is_Its_Own_Evaluation_Unit()
    {
        var language = new ProbeLanguage();
        var snapshot = Compile(language, control: local => local.WithAppearance(ThemeAppearance.Dark));

        // The control's view is derived from the global tokens of the dark variant; the scope stays light.
        snapshot.Own<Color>(nameof(ViewProbeToken.Accent)).ShouldBe(Colors.Black);
        snapshot.Global<Color>(nameof(DesignToken.ColorBgBase)).ShouldBe(Colors.White);
        language.DeriveGlobalCalls.ShouldBe(2);
    }

    [Fact]
    public void A_Failing_Global_Derivation_Fails_The_Whole_Compilation_And_Names_The_Language()
    {
        var language = new ProbeLanguage { FailGlobal = true };
        var result = CompileResult(language, null, null);

        result.Success.ShouldBeFalse();
        var exception = result.Exception.ShouldBeOfType<ThemeDerivationException>();
        exception.Language.ShouldBe("Probe");
        exception.Control.ShouldBeNull();
    }

    [Fact]
    public void A_Control_Without_A_Derivation_In_An_Enabled_Language_Names_The_Missing_Value_Package()
    {
        var exception = Should.Throw<ThemeSchemaException>(() => new ThemeSchemaRegistry(
            GeneratedThemeSchema.GetGlobalTokens(), [ViewProbeToken.CreateDescriptor()],
            [TestDesignLanguages.Probe()], null, []));

        exception.Message.ShouldContain($"'{ViewProbeToken.ControlIdentity}' under 'Probe'");
        // A hand-written identity has no owner type, so the package is named after the catalog.
        exception.Message.ShouldContain("install and enable 'Test.Themes.Probe'");
    }

    // ---------------------------------------------------------------- Helpers

    private static ThemeSnapshot Compile(
        ProbeLanguage? language = null,
        Action<ThemeConfigBuilder>? root = null,
        Action<ControlThemeConfigBuilder>? control = null)
    {
        var result = CompileResult(language ?? new ProbeLanguage(), root, control);
        result.Exception.ShouldBeNull();
        result.Success.ShouldBeTrue();
        return result.Snapshot!;
    }

    private static ThemeCompileResult CompileResult(
        ProbeLanguage language,
        Action<ThemeConfigBuilder>? root,
        Action<ControlThemeConfigBuilder>? control)
    {
        var descriptor = TestDesignLanguages.Probe(language);
        var registry = new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(), [ViewProbeToken.CreateDescriptor()],
            [descriptor], null, [ControlDerivationDescriptor.Create<ProbeLanguageToken, ViewProbeToken, ViewProbeDerivation>()]);
        var builder = new ThemeConfigBuilder();
        root?.Invoke(builder);
        if (control is not null)
        {
            var local = new ControlThemeConfigBuilder();
            control(local);
            builder.WithControl(ViewProbeToken.ControlIdentity, local.Build());
        }

        var normalized = ThemeConfigNormalizer.Normalize(builder.Build(), registry);
        normalized.Success.ShouldBeTrue(string.Join("; ", normalized.Diagnostics.Select(static d => d.Message)));
        var definition = ThemeTestSnapshotFactory.CreateLanguageDefinition(descriptor);
        var defaults = ThemeCompiler.CreateDefinitionDefaults(definition, registry);
        var effective = ThemeConfigMerger.Merge(defaults, null, normalized.Config!).EffectiveConfig;
        return new ThemeCompiler().Compile(new ThemeCompileInput(definition,
            new ThemeDefinitionRevision("test", "1", effective.Fingerprint.ToString()), effective, registry));
    }
}

internal static class ViewProbeSnapshotExtensions
{
    internal static ControlThemeSnapshot ViewProbe(this ThemeSnapshot snapshot)
    {
        snapshot.Registry.TryGetControl(ViewProbeToken.ControlIdentity, out var control).ShouldBeTrue();
        return snapshot.Controls[control!.Slot];
    }

    internal static T Own<T>(this ThemeSnapshot snapshot, string name)
    {
        snapshot.Registry.TryGetControl(ViewProbeToken.ControlIdentity, out var control).ShouldBeTrue();
        var slot = control!.OwnTokens.Single(token => token.Name == name).Slot;
        return snapshot.ViewProbe().ControlTokenValues.Get<T>(slot);
    }
}

public enum ViewProbeTokenKind
{
    Accent,
    BaseSize,
    DerivedHeight
}

/// <summary>A control token written by hand the way the generator writes one: every setter checks the freeze.</summary>
public sealed class ViewProbeToken : AbstractControlDesignToken, IControlToken
{
    public static ControlTokenIdentity ControlIdentity { get; } = new("Test", "ViewProbe");

    public Color Accent
    {
        get => field;
        set
        {
            if (!IsFrozen(nameof(Accent)))
            {
                field = value;
            }
        }
    }

    public double BaseSize
    {
        get => field;
        set
        {
            if (!IsFrozen(nameof(BaseSize)))
            {
                field = value;
            }
        }
    }

    public double DerivedHeight
    {
        get => field;
        set
        {
            if (!IsFrozen(nameof(DerivedHeight)))
            {
                field = value;
            }
        }
    }

    internal static ControlTokenDescriptor CreateDescriptor() => new(
        typeof(ViewProbeControl),
        ControlIdentity,
        [
            Descriptor<Color>(nameof(Accent), 0, ViewProbeTokenKind.Accent, static token => token.Accent,
                static (token, value) => token.Accent = value),
            Descriptor<double>(nameof(BaseSize), 1, ViewProbeTokenKind.BaseSize, static token => token.BaseSize,
                static (token, value) => token.BaseSize = value),
            Descriptor<double>(nameof(DerivedHeight), 2, ViewProbeTokenKind.DerivedHeight,
                static token => token.DerivedHeight, static (token, value) => token.DerivedHeight = value)
        ],
        static () => new ViewProbeToken());

    private static TokenDescriptor Descriptor<T>(string name, int slot, ViewProbeTokenKind kind,
        Func<ViewProbeToken, T> get, Action<ViewProbeToken, T> set) where T : notnull =>
        new(name, slot, TokenStage.Control, typeof(T), kind,
            value => ThemeTokenValueParser.Parse<T>(value),
            value => ThemeTokenValueFormatter.Format((T)value!),
            token => get((ViewProbeToken)token),
            (token, value) => set((ViewProbeToken)token, (T)value!),
            token => get((ViewProbeToken)token));

    private sealed class ViewProbeControl : Control;
}

/// <summary>
/// The probe control's view: the base size follows the control height, the derived height follows the base size, the
/// accent follows the background base, and the control draws with a thicker line.
/// </summary>
internal sealed class ViewProbeDerivation : IControlDerivation<ProbeLanguageToken, ViewProbeToken>
{
    // Set by single tests of DesignLanguageCompilationTests; the tests of one class do not run in parallel.
    internal static int Calls;
    internal static bool WriteSeed;

    public void Derive(in DerivationContext<ProbeLanguageToken> context, ControlView<ViewProbeToken> view)
    {
        Calls++;
        if (WriteSeed)
        {
            view.Global.ColorPrimary = Colors.Red;
        }

        view.Global.LineWidthBold = 3;
        view.Token.BaseSize = view.Global.ControlHeight;
        view.Token.DerivedHeight = view.Token.BaseSize + 10;
        view.Token.Accent = view.Global.ColorBgBase ?? Colors.Transparent;
    }
}
