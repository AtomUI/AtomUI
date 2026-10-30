using AtomUI.Generated.AtomUICore;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;
using Shouldly;

namespace AtomUI.Core.Tests.Theme;

internal static class ThemeTestSnapshotFactory
{
    internal static ThemeSnapshot Compile(
        ThemeConfig? config = null,
        params ControlTokenDescriptor[] controls)
    {
        var registry = new ThemeSchemaRegistry(
            GeneratedThemeSchema.GetGlobalTokens(),
            controls,
            GeneratedThemeSchema.GetAlgorithms());
        registry.TryGetAlgorithm(ThemeAlgorithm.Default, out var defaultAlgorithm).ShouldBeTrue();
        var location = new ThemeSourceLocation("test", 1, 1, "/Theme");
        var definition = new PreparedThemeDefinition(
            "TestTheme",
            "Test Theme",
            ThemeAppearance.Light,
            ThemeAppearance.Light,
            true,
            [defaultAlgorithm!],
            Array.Empty<BoundTokenValue>(),
            Array.Empty<ControlThemeDefinition>(),
            location);
        var normalized = ThemeConfigNormalizer.Normalize(
            config ?? new ThemeConfigBuilder().Build(),
            registry);
        normalized.Success.ShouldBeTrue();
        var defaults = ThemeCompiler.CreateDefinitionDefaults(definition, registry);
        var effective = ThemeConfigMerger.Merge(
            defaults,
            null,
            normalized.Config!).EffectiveConfig;
        var result = new ThemeCompiler().Compile(new ThemeCompileInput(
            definition,
            new ThemeDefinitionRevision("test", "1", effective.Fingerprint.ToString()),
            effective,
            registry));

        result.Success.ShouldBeTrue();
        return result.Snapshot!;
    }

    internal static T Global<T>(this ThemeSnapshot snapshot, string name)
    {
        snapshot.Registry.TryGetGlobalToken(name, out var descriptor).ShouldBeTrue();
        return snapshot.GlobalTokenValues.Get<T>(descriptor!.Slot);
    }

    internal static T EffectiveGlobal<T>(
        this ThemeSnapshot snapshot,
        ControlTokenIdentity identity,
        string name)
    {
        snapshot.Registry.TryGetControl(identity, out var control).ShouldBeTrue();
        snapshot.Registry.TryGetGlobalToken(name, out var token).ShouldBeTrue();
        return snapshot.Controls[control!.Slot]
                       .GetEffectiveGlobalValue<T>(snapshot.GlobalTokenValues, token!.Slot);
    }

    internal static T Control<T>(
        this ThemeSnapshot snapshot,
        ControlTokenIdentity identity,
        int tokenSlot)
    {
        snapshot.Registry.TryGetControl(identity, out var control).ShouldBeTrue();
        return snapshot.Controls[control!.Slot].ControlTokenValues.Get<T>(tokenSlot);
    }
}

internal static class ThemeCompilerTests
{
    internal static ControlTokenDescriptor CreateCompilerButtonDescriptor()
    {
        var height = new TokenDescriptor(
            nameof(CompilerButtonToken.Height),
            0,
            TokenStage.Control,
            typeof(double),
            CompilerButtonTokenKind.Height,
            static value => ThemeTokenValueParser.Parse<double>(value),
            static value => ThemeTokenValueFormatter.Format((double)value!),
            static token => ((CompilerButtonToken)token).Height,
            static (token, value) => ((CompilerButtonToken)token).Height = (double)value!,
            static token => ((CompilerButtonToken)token).Height);
        return new ControlTokenDescriptor(
            ThemeTestControlTypes.For("AtomUI", CompilerButtonToken.ID),
            new ControlTokenIdentity("AtomUI", CompilerButtonToken.ID),
            [height],
            static () => new CompilerButtonToken(),
            static (token, appearance) =>
                ((CompilerButtonToken)token).CalculateTokenValues(appearance == ThemeAppearance.Dark));
    }
}

internal enum CompilerButtonTokenKind
{
    Height
}

internal sealed class CompilerButtonToken : AbstractControlDesignToken
{
    internal const string ID = "Button";

    public double Height { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        Height = EffectiveGlobalToken.ControlHeight;
    }
}
